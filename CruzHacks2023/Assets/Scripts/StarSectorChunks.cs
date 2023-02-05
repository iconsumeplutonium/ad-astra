using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSectorChunks : MonoBehaviour {

    public float maxViewDist = 300;
    public int sectorSize = 100;
    public Transform viewer;

    public static Vector2 camPos;
    public int renderDist;

    public Dictionary<Vector2, StarSector> sectorDict = new Dictionary<Vector2, StarSector>();
    public List<StarSector> sectorsVisibleLastFrame = new List<StarSector>();

    public int seed;

    public float minStarSize;
    public float maxStarSize;
    public float starDistThreshold; //minimum distance between stars

    public int systemExistenceProbability;

    public GameObject selectionPrefab;

    public UIManager manager;

    public bool canGenerateUniverse;

    private void Update() {
        if (canGenerateUniverse) {
            camPos = new Vector2(viewer.position.x, viewer.position.z);
            UpdateSectors();
        }
    }

    public void UpdateSectors() {
        if (!manager.hasUIElementsOpen) {
            for (int i = 0; i < sectorsVisibleLastFrame.Count; i++) {
                sectorsVisibleLastFrame[i].SetVisible(false);
            }
            sectorsVisibleLastFrame.Clear();

            int currentSectorX = Mathf.RoundToInt(camPos.x / sectorSize);
            int currentSectorY = Mathf.RoundToInt(camPos.y / sectorSize);

            for (int y = -renderDist; y <= renderDist; y++) {
                for (int x = -renderDist; x <= renderDist; x++) {
                    Vector2 viewedSectorCoord = new Vector2(currentSectorX + x, currentSectorY + y);

                    if (sectorDict.ContainsKey(viewedSectorCoord)) {
                        sectorDict[viewedSectorCoord].UpdateSector(maxViewDist);
                        if (sectorDict[viewedSectorCoord].IsVisible())
                            sectorsVisibleLastFrame.Add(sectorDict[viewedSectorCoord]);
                    }
                    else {
                        sectorDict.Add(viewedSectorCoord, new StarSector(viewedSectorCoord, sectorSize, seed, minStarSize, maxStarSize, systemExistenceProbability, starDistThreshold, selectionPrefab));
                    }
                }
            }
        }
    }


    public class StarSector {
        public GameObject sectorObj;
        public GameObject plane;
        public List<StarSystem> systems;
        public Vector3 position;

        public StarSector(Vector2 pos, int size, int seed, float minStarSize, float maxStarsize, int systemProb, float systemSeparationThreshold, GameObject selectionPrefab) {
            position = new Vector3(pos.x * size, 0, pos.y * size);
            sectorObj = new GameObject($"P({position.x}, {position.z})");
            sectorObj.transform.position = position;
            sectorObj.tag = "Chunk";

            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.parent = sectorObj.transform;
            plane.transform.position = sectorObj.transform.position;
            plane.transform.localScale = Vector3.one * size / 10f;
            plane.SetActive(false);

            systems = new List<StarSystem>();
            Vector3[] localVertices = plane.GetComponent<MeshFilter>().mesh.vertices;
            Vector3 topLeft = plane.transform.TransformPoint(localVertices[0]);
            Vector3 bottomRight = plane.transform.TransformPoint(localVertices[120]);

            //First pass: System Generation
            for (int i = (int)bottomRight.x; i < topLeft.x; i++) {
                for (int j = (int)bottomRight.z; j < topLeft.z; j++) {

                    //Should a system exist at this location?
                    if (DoesSystemExistAtCoordinate(i, j, seed, systemProb)) {
                        //Makes sure that if a star can exist here, it does not overlap with other stars
                        if (DoesntOverlapWithOtherStars(systems, new Vector3Int(i, 0, j), systemSeparationThreshold)) {
                            Vector3Int systemPosition = new Vector3Int(i, 0, j);

                            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            StarSystem s = o.AddComponent<StarSystem>();
                            s.InitializeSystem(systemPosition, sectorObj.transform, seed, minStarSize, maxStarsize, selectionPrefab);
                            systems.Add(s);
                        }
                    }
                }
            }

            SetVisible(false);
        }

        public void UpdateSector(float maxViewDist) {
            float sqrX = (position.x - camPos.x) * (position.x - camPos.x);
            float sqrY = (position.z - camPos.y) * (position.z - camPos.y);
            float distToCamera = Mathf.Sqrt(sqrX + sqrY);
            bool isVisible = distToCamera <= maxViewDist;
            SetVisible(isVisible);

            for (int i = 0; i < systems.Count; i++) {
                Collider[] c = Physics.OverlapSphere(systems[i].position, systems[i].star.size);
                if (c.Length > 0) {
                    for (int j = 0; j < c.Length; j++) {
                        if (c[j] != systems[i].star.obj.GetComponent<Collider>())
                            c[j].transform.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void SetVisible(bool visible) {
            sectorObj.SetActive(visible);
        }

        public bool IsVisible() {
            return sectorObj.activeSelf;
        }

        private bool DoesSystemExistAtCoordinate(int x, int y, int seed, int probability) {
            System.Random p = new System.Random(Utilities.cash(x, y, seed));
            return p.Next(1, probability) == 1;
        }

        private bool DoesntOverlapWithOtherStars(List<StarSystem> systems, Vector3Int pos, float threshold) {
            foreach (var s in systems) {
                if (Vector3Int.Distance(s.position, pos) < threshold)
                    return false;
            }
            return true;
        }
    }

}

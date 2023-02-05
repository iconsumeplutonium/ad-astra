using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject StarInfoUIPanel;
    public TMP_Text starClass;
    public TMP_Text starSize;
    public TMP_Text starTemp;

    public GameObject planetOverviewUI;
    public Slider slider;

    public GameObject mainMenu;
    public TMP_InputField field;

    public bool hasUIElementsOpen;
    public bool transitionCoroutineIsActive;
    public bool isInPlanetaryViewMode;

    //public Vector3 originalPosWhenUIClicked;
    public GameObject viewer;

    public MatrixBlender mBlender;
    public AssetContainer container;
    public StarSectorChunks chunks;
    public Controller controller;

    private Collider[] collidersNearSelectedStar;

    public List<GameObject> instantiatedPlanetsInViewMode;


    private void Start() {
        StarInfoUIPanel.SetActive(false);
        planetOverviewUI.SetActive(false);
        mainMenu.SetActive(true);
        hasUIElementsOpen = true;
    }

    public void EnableStarInfo(StarSystem s/*Utilities.StellarClass sClass, float size*/) {
        //writes star data to textboxes
        starClass.text = s.star.sClass.ToString();
        starSize.text = Utilities.ConvertUnitsToMiles(s.star.size, chunks.minStarSize, chunks.maxStarSize).ToString("0.##E+00");
        starTemp.text = s.star.temperature.ToString() + 'K';
        StarInfoUIPanel.SetActive(true);

        //writes planet data to textboxes
        for (int i = 0; i < 8; i++) {
            if (i + 1 > s.planets.Length)
                container.texts[i].parent.SetActive(false);
            else {
                container.texts[i].parent.SetActive(true);
                container.texts[i].typeText.text = s.planets[i].type.ToString();
                container.texts[i].sizeText.text = s.planets[i].diameter.ToString("N0") + "mi";
            }
        }

        collidersNearSelectedStar = Physics.OverlapSphere(s.position, 100f);
        for (int i = 0; i < collidersNearSelectedStar.Length; i++) {
            collidersNearSelectedStar[i].transform.gameObject.SetActive(false);
        }
        s.gameObject.SetActive(true);

        hasUIElementsOpen = true;
    }

    public void DisableStarInfo() {
        StarInfoUIPanel.SetActive(false);
        hasUIElementsOpen = false;
        //Camera.main.transform.position = new Vector3(0f, 5f, 0f);
        //StartCoroutine(ZoomToLocation(viewer, originalPosWhenUIClicked));
        //Camera.main.orthographic = true;

        float orthographicSize = 5f;
        float aspect = (float)Screen.width / (float)Screen.height;
        float near = 0.3f;
        float far = 1000f;
        Matrix4x4 ortho = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, near, far);
        mBlender.BlendToMatrix(ortho, 0.5f);

        StartCoroutine(ZoomToLocation(viewer, new Vector3(viewer.transform.position.x, 5, viewer.transform.position.z)));
        if (isInPlanetaryViewMode) {
            InverseRotateIntoPlanetaryObservationPosition(viewer, controller.selectedSystem.position);
            //Camera.main.transform.position = controller.selectedSystem.position + new Vector3(-0.7f, 0f, 0f);
            //Camera.main.transform.rotation = new Quaternion(90f, 0f, 0f, 0f);
        }
        Camera.main.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        for (int i = 0; i < collidersNearSelectedStar.Length; i++) {
            collidersNearSelectedStar[i].transform.gameObject.SetActive(true);
        }
        Array.Clear(collidersNearSelectedStar, 0, collidersNearSelectedStar.Length);
    }

    public IEnumerator ZoomToLocation(GameObject viewer, Vector3 targetLocation) {
        transitionCoroutineIsActive = true;
        while (viewer.transform.position != targetLocation) {
            viewer.transform.position = Vector3.MoveTowards(viewer.transform.position, targetLocation, Time.deltaTime * 10f);
            yield return null;
        }
        transitionCoroutineIsActive = false;
        //Camera.main.fieldOfView = 34;
    }

    public IEnumerator RotateIntoPlanetaryObservationPosition(GameObject viewer, Vector3 pos) {
        transitionCoroutineIsActive = true;
        Vector3[] points = {
            new Vector3(2.87f, 3.59f, -1.73f),
            new Vector3(4.71f, 2.15f, -0.51f),
            new Vector3(5.91f, 0.42f, -0.22f),
            new Vector3(6.31f, -1.27f, -0.07f) //3.01f
        };

        for (int i = 0; i < 4; i++) {
            while (viewer.transform.position != points[i] + pos) {
                viewer.transform.position = Vector3.MoveTowards(viewer.transform.position, points[i] + pos, Time.deltaTime * 10f);
                //Camera.main.transform.RotateAround(pos, Vector3.forward, Time.deltaTime * 10f);
                Camera.main.transform.LookAt(pos, Vector3.forward);
                yield return null;
            }
        }

        //because for some reason it ends up with its x rotation at -11
        //its 4:30am and i dont care enough to come up with a proper fix
        //this code tries to smoothly transition it to where it should be
        float a = -10f;
        for (int i = 0; i < 10; i++) {
            Camera.main.transform.rotation = Quaternion.Euler(a, -90f, -90f);
            a += 1f;
            yield return null;
        }

        Camera.main.transform.rotation = Quaternion.Euler(0f, -90f, -90f);
        transitionCoroutineIsActive = false;
        isInPlanetaryViewMode = true;
    }

    public IEnumerator InverseRotateIntoPlanetaryObservationPosition(GameObject viewer, Vector3 pos) {
        transitionCoroutineIsActive = true;
        Vector3[] points = {
            new Vector3(2.87f, 3.59f, -1.73f),
            new Vector3(4.71f, 2.15f, -0.51f),
            new Vector3(5.91f, 0.42f, 1.16f),
            new Vector3(6.31f, -1.27f, 3.01f)
        };
        Vector3 finalVector = pos + new Vector3(-0.7f, 5f, 0f);

        for (int i = 3; i > -1; i--) {
            while (viewer.transform.position != points[i] + pos) {
                viewer.transform.position = Vector3.MoveTowards(viewer.transform.position, points[i] + pos, Time.deltaTime * 10f);
                //Camera.main.transform.RotateAround(pos, Vector3.forward, Time.deltaTime * 10f);
                Camera.main.transform.LookAt(pos, Vector3.forward);
                yield return new WaitForSeconds(2f);
            }
        }

        while (viewer.transform.position != finalVector) {
            viewer.transform.position = Vector3.MoveTowards(viewer.transform.position, finalVector, Time.deltaTime * 10f);
            //Camera.main.transform.RotateAround(pos, Vector3.forward, Time.deltaTime * 10f);
            
            yield return null;
        }

        Camera.main.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        transitionCoroutineIsActive = false;
        isInPlanetaryViewMode = false;
    }


    public void OnRotat() {
        StartCoroutine(RotateIntoPlanetaryObservationPosition(viewer, controller.selectedSystem.position));
        StarInfoUIPanel.SetActive(false);
        planetOverviewUI.SetActive(true);
        slider.value = 0f;
        GameObject light = Instantiate(container.lightPrefab, controller.selectedSystem.position + new Vector3(0f, -2f, 0f), Quaternion.Euler(90f, 0f, 0f));
        float dist = 3f;
        for (int i = 0; i < controller.selectedSystem.planets.Length; i++) {
            Vector3 newPos = controller.selectedSystem.position + new Vector3(0f, -dist, 0f);
            GameObject o = Instantiate(container.planetPrefabs[(int)controller.selectedSystem.planets[i].type], newPos, Quaternion.identity);
            o.transform.parent = controller.selectedSystem.star.obj.transform;
            instantiatedPlanetsInViewMode.Add(o);
            dist += 3;
            
        }
    }

    public void OnSliderValueChanged() {
        Vector3 start = controller.selectedSystem.position;
        Vector3 end = instantiatedPlanetsInViewMode[instantiatedPlanetsInViewMode.Count - 1].transform.position;
        Vector3 lerpedVector = Vector3.Lerp(start, end, slider.value);
        viewer.transform.position = new Vector3(viewer.transform.position.x, lerpedVector.y, viewer.transform.position.z);
    }

    public void OnReturnToPreviousScreen() {
        planetOverviewUI.SetActive(false);
        //StartCoroutine(InverseRotateIntoPlanetaryObservationPosition(viewer, controller.selectedSystem.position));
        viewer.transform.position = controller.selectedSystem.position + new Vector3(-0.7f, 5f, 0f);
        Camera.main.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        for (int i = 0; i < instantiatedPlanetsInViewMode.Count; i++) {
            Destroy(instantiatedPlanetsInViewMode[i]);
        }
        StarInfoUIPanel.SetActive(true);
    }

    public void OnGenerateUniverse() {
        int seed;
        if (field.text != null) {
            int.TryParse(field.text, out seed);
        } else {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
        chunks.seed = seed;
        mainMenu.SetActive(false);
        chunks.canGenerateUniverse = true;
        hasUIElementsOpen = false;
    }


}

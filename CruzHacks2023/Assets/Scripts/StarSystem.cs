using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSystem : MonoBehaviour {

    public Vector3Int position;
    public Star star;
    public Transform sector;

    public float minStarSize;
    public float maxStarSize;

    public GameObject selectionPrefab;
    public GameObject selectionObj;

    public Planet[] planets;


    public void InitializeSystem(Vector3Int position, Transform parent, int seed, float minSize, float maxSize, GameObject selectionPrefab) {
        this.position = position;
        sector = parent;
        minStarSize = minSize;
        maxStarSize = maxSize;
        this.selectionPrefab = selectionPrefab;

        GenerateSystem(seed);
        InstantiateSystem();
    }

    public void GenerateSystem(int seed) {
        System.Random prng = new System.Random(Utilities.cash(position.x, position.z, seed));

        //resorting to seeding Random.Range because System.Random only works with ints, and using workarounds
        //to trick it into giving me floats causes it to have an aneurysm. thank you, c#, very cool.
        //update: system.random only gives me the same thing regardless of whats put into it for some fucking reason.
        //i will now use system.random for planet generation too. 
        Random.State originalState = Random.state;
        Random.InitState(Utilities.cash(position.x, position.z, seed));
        float tentativeStarSize = Random.Range(minStarSize, maxStarSize);
        star.size = Utilities.AdjustFreq(tentativeStarSize / maxStarSize) * maxStarSize;
        

        star.sClass = StellarClassGivenSize(star.size, minStarSize, maxStarSize);
        AssetContainer a = GameObject.FindGameObjectWithTag("Brain").GetComponent<AssetContainer>();

        //picks random number of planets
        //for each planet, it picks a random type
        //depending on that type it picks a size, and whether not it has rings
        //terrestrial planet sizes are between 3031mi and 7091mi (diameter of the smallest known planet, Mercury, and the diameter of the largest known Super Earth TOI-1075b)
        planets = new Planet[Random.Range(1, 9)];
        for (int i = 0; i < planets.Length; i++) {
            planets[i].type = (Utilities.PlanetType)Random.Range(0, 3);
            planets[i].diameter = (planets[i].type == Utilities.PlanetType.Terrestrial) ? prng.Next(3031, 14181) : prng.Next(30599, 139880);
            planets[i].hasRings = (planets[i].type == Utilities.PlanetType.GasGiant || planets[i].type == Utilities.PlanetType.IceGiant) ? Random.Range(1, 3) == 1 : false;
        }

        //float r = prng.Next(1, 9999) / 9999f;
        //float g = prng.Next(1, 9999) / 9999f;
        //float b = prng.Next(1, 9999) / 9999f;
        star.material = a.StarMats[(int)star.sClass];//new Color(r, g, b);
        star.temperature = TemperatureGivenClassification(prng, star.sClass);

        Random.state = originalState;
    }

    public void InstantiateSystem() {
        star.obj = transform.gameObject; //GameObject.CreatePrimitive(PrimitiveType.Sphere);
        star.obj.transform.position = position;
        star.obj.transform.localScale = Vector3.one * star.size;
        star.obj.name = $"{star.sClass} Star (x:{position.x} z:{position.z})";
        star.obj.transform.parent = sector;

        //Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        //m.SetColor("_EmissionColor", star.color * Mathf.Pow(2, 1.2f));
        //m.EnableKeyword("_EMISSION");
        star.obj.GetComponent<Renderer>().material = star.material;

        selectionObj = GameObject.Instantiate(selectionPrefab);
        selectionObj.transform.position = position;
        selectionObj.transform.localScale = Vector3.one * ((star.size / 10) + 0.025f);
        selectionObj.name = "SelectionOutline";
        selectionObj.transform.parent = star.obj.transform;
        selectionObj.SetActive(false);

    }

    public Utilities.StellarClass StellarClassGivenSize(float size, float minSize, float maxSize) {
        float seventh = (maxSize - minSize) / 7;
        if (size >= minSize && size <= minSize + seventh)
            return Utilities.StellarClass.M;
        else if (size > minSize + seventh && size <= minSize + seventh * 2)
            return Utilities.StellarClass.K;
        else if (size > minSize + seventh * 2 && size <= minSize + seventh * 3)
            return Utilities.StellarClass.G;
        else if (size > minSize + seventh * 3 && size <= minSize + seventh * 4)
            return Utilities.StellarClass.F;
        else if (size > minSize + seventh * 4 && size <= minSize + seventh * 5)
            return Utilities.StellarClass.A;
        else if (size > minSize + seventh * 5 && size <= minSize + seventh * 6)
            return Utilities.StellarClass.B;
        else if (size > minSize + seventh * 6 && size <= maxSize)
            return Utilities.StellarClass.O;

        return Utilities.StellarClass.M;
    }

    public string SystemProperties() {
        string s = $"{star.sClass} class, {planets.Length} planets. ";

        for (int i = 0; i < planets.Length; i++) {
            s += $"({i + 1}: {planets[i].type}, s{planets[i].diameter}, hR{planets[i].hasRings}.) ";
        }    

        return s;
    }

    public int TemperatureGivenClassification(System.Random rand, Utilities.StellarClass c) {
        switch(c) {
            case Utilities.StellarClass.M:
                return rand.Next(24, 37) * 100;
            case Utilities.StellarClass.K:
                return rand.Next(37, 52) * 100;
            case Utilities.StellarClass.G:
                return rand.Next(52, 60) * 100;
            case Utilities.StellarClass.F:
                return rand.Next(60, 75) * 100;
            case Utilities.StellarClass.A:
                return rand.Next(75, 100) * 100;
            case Utilities.StellarClass.B:
                return rand.Next(100, 300) * 100;
            default: //O-class
                return rand.Next(300, 350) * 1000;


        }
    }
}

public struct Star {
    public float size;
    public GameObject obj;
    public Utilities.StellarClass sClass;
    public Material material;
    public int temperature;

}

public struct Planet {
    public float diameter;
    public Utilities.PlanetType type;
    public bool hasRings;
}

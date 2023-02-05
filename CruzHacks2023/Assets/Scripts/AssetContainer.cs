using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AssetContainer : MonoBehaviour {

    public Material[] StarMats;

    public PlanetText[] texts;

    public GameObject[] planetPrefabs;
    public GameObject lightPrefab;

}

[System.Serializable]
public struct PlanetText {
    public GameObject parent;
    public TMP_Text typeText;
    public TMP_Text sizeText;
}

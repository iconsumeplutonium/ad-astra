using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities  {

    public enum StellarClass {
        M, K, G, F, A, B, O
    }

    public enum PlanetType {
        Terrestrial, GasGiant, IceGiant
    }

    //https://stackoverflow.com/questions/37128451/random-number-generator-with-x-y-coordinates-as-seed
    public static int cash(int x, int y, int seed) {
        int h = seed + x * 374761393 + y * 668265263;
        h = (h ^ (h >> 13)) * 1274126177;
        return h ^ (h >> 16);
    }

    //from https://youtu.be/lctXaT9pxA0?t=447
    public static float AdjustFreq(float x) {
        float k = Mathf.Pow(1f - 0.2f, 3);
        return (x * k) / (x * k - x + 1);
    }

    //46179000: 0.7 solar radii (about the size of most M-class stars)
    //435402000: 6.6 solar radii (about the size of most O-class stars), but with one zero removed
    public static long ConvertUnitsToMiles(float size, float minStarSize, float maxStarSize) {
        float percent = size / (maxStarSize - minStarSize);
        return Mathf.RoundToInt((percent * (435402000 - 46179000)) + 46179000);
    }

}

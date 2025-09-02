using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility classes to make life easier
/// </summary>
/// 
/// * Author: Francisca Riffo (fran.riffo.astete@gmail.com)
/// * Last time modified: 2020-11-17

/// <summary>
/// Utility class to get Random Values and Points
/// </summary>
public static class RandomUtil {
    /// <summary>
    /// Get a random number between the components of a <c>Vector2</c>
    /// </summary>
    /// <param name="limits">Vector with the min and max values</param>
    /// <returns>A random float between min (inclusive) and max (inclusive)</returns>
    public static float Range(this Vector2 limits) => Random.Range(limits.x, limits.y);
    /// <summary>
    /// Get a random number between the components of a <c>Vector2</c>
    /// </summary>
    /// <param name="limits">Vector with the min and max values</param>
    /// <returns>A random int between min (inclusive) and max (exclusive)</returns>
    public static int Range(this Vector2Int limits) => Random.Range(limits.x, limits.y);
    /// <summary>
    /// Get a Random element of an array
    /// </summary>
    /// <typeparam name="T">Element Type</typeparam>
    /// <param name="array">Array with elements</param>
    /// <returns>Random element of the array</returns>
    public static T RandomPick<T>(this T[] array) => array[Random.Range(0, array.Length)];
    public static T RandomPick<T>(this List<T> array) => array[Random.Range(0, array.Count)];
    /// <summary>
    /// Get a Random element of an array without repetition
    /// </summary>
    /// <typeparam name="T">Element Type</typeparam>
    /// <param name="array">Array with elements</param>
    /// <param name="last">Index of the last element picked</param>
    /// <returns>A tuple with a random element of the array and its index</returns>
    public static (T, int) RandomPick<T>(this T[] array, int last) {
        if (array.Length < 2) {
            Debug.LogWarning("Array is size 1, can't pick a different element");
            return (array[0], 0);
        }

        int idx = last;
        while (idx == last) {
            idx = Random.Range(0, array.Length);
        }
        return (array[idx], idx);
    }
    public static (T, int) RandomPick<T>(this List<T> array, int last) {
        if (array.Count < 2) {
            Debug.LogWarning("List is size 1, can't pick a different element");
            return (array[0], 0);
        }

        int idx = last;
        while (idx == last) {
            idx = Random.Range(0, array.Count);
        }
        return (array[idx], idx);
    }
    /// <summary>
    /// Gets a Random element of an array without repetition
    /// </summary>
    /// <typeparam name="T">Element Type</typeparam>
    /// <param name="array">Array with elements</param>
    /// <param name="picked">Array with already picked elements</param>
    /// <returns>Random element not on picked array</returns>
    public static T RandomPick<T>(this T[] array, T[] picked) {
        if (array.Length < 2) {
            Debug.LogWarning("Array is size 1, can't pick a different element");
            return array[0];
        }

        if (array.Length <= picked.Length) {
            Debug.LogWarning("Array isn't bigger than picked array, all elements must already been seen. Aborting...");
            return array[0];
        }

        T element;
        do {
            element = array.RandomPick();
        } while (System.Array.IndexOf(picked, element) > -1);
        return element;
    }
    /// <summary>
    /// Get a Random 2D Point of a determined bound
    /// </summary>
    /// <param name="zone">Bounds of the area</param>
    /// <returns>Vector2 with a random point inside the area</returns>
    public static Vector2 RandomPoint(this Rect zone) {
        var x = Random.Range(zone.xMin, zone.xMax);
        var y = Random.Range(zone.yMin, zone.yMax);
        return new Vector2(x, y);
    }
}

/// <summary>
/// Utility methods for the Transform class
/// </summary>
public static class TransformExtensions {
    /// <summary>
    /// Randomize the order of the childs of this transform.
    /// </summary>
    /// <param name="transform">Parents which childs will be shuffled</param>
    public static void RandomizeChildOrder(this Transform transform) {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).SetSiblingIndex(Random.Range(0, transform.childCount));
        }
    }
    /// <summary>
    /// Destroy all the children of this transform
    /// </summary>
    /// <param name="transform">Parent of the childs to destroy</param>
    public static void DestroyChildren(this Transform transform) {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Object.Destroy(transform.GetChild(i).gameObject);
        }
    }
}

/// <summary>
/// Utility methods for the Array/List class
/// </summary>
public static class ArrayExtensions {
    /// <summary>
    /// Shuffle the elements of the array using the Fisher-Yates algorithm
    /// </summary>
    /// <typeparam name="T">Element type of the array</typeparam>
    /// <param name="arr">Array to be shuffled</param>
    public static void Shuffle<T>(this T[] arr) {
        int n = arr.Length;
        while (n > 1) {
            n--;
            var k = Random.Range(0, n + 1);
            var val = arr[k];
            arr[k] = arr[n];
            arr[n] = val;
        }
    }

    /// <summary>
    /// Shuffle the elements of the list using the Fisher-Yates algorithm
    /// </summary>
    /// <typeparam name="T">Element type of the list</typeparam>
    /// <param name="arr">List to be shuffled</param>
    public static void Shuffle<T>(this List<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            var k = Random.Range(0, n + 1);
            var val = list[k];
            list[k] = list[n];
            list[n] = val;
        }
    }
}

/// <summary>
/// Utility class to smooth transitions/animations with mathematic curves and interpolations
/// </summary>
public static class Curves {
    /// <summary>
    /// Type of the curve to be applied
    /// </summary>
    public enum CurveType { Linear, EaseIn, EaseOut, ExpEasIn, SmoothStep }

    /// <summary>
    /// Apply the Ease In Curve to interpolate
    /// </summary>
    /// <param name="t">Value between 0 and 1</param>
    /// <returns>The result of the interpolation</returns>
    public static float EaseIn(float t) => 1f - Mathf.Cos(t * Mathf.PI * .5f);
    /// <summary>
    /// Apply the Ease Out Curve to interpolate
    /// </summary>
    /// <param name="t">Value between 0 and 1</param>
    /// <returns>The result of the interpolation</returns>
    public static float EaseOut(float t) => Mathf.Sin(t * Mathf.PI * .5f);
    /// <summary>
    /// Apply the Exponential Ease In Curve to interpolate
    /// </summary>
    /// <param name="t">Value between 0 and 1</param>
    /// <returns>The result of the interpolation</returns>
    public static float ExpEaseIn(float t) => t * t;
    /// <summary>
    /// Apply a Smooth Step to interpolate
    /// </summary>
    /// <param name="t">Value between 0 and 1</param>
    /// <returns>The result of the interpolation</returns>
    public static float SmoothStep(float t) => t * t * (3f - 2f * t);
    /// <summary>
    /// Specify the curve to apply to the interpolation
    /// </summary>
    /// <param name="t">Value between 0 and 1</param>
    /// <param name="curveType">Curve to be applied</param>
    /// <returns>The result of the interpolation</returns>
    public static float ApplyCurve(float t, CurveType curveType) {
        switch (curveType) {
            case CurveType.EaseIn: return EaseIn(t);
            case CurveType.EaseOut: return EaseOut(t);
            case CurveType.ExpEasIn: return ExpEaseIn(t);
            case CurveType.SmoothStep: return SmoothStep(t);

            default: return t;
        }
    }
}

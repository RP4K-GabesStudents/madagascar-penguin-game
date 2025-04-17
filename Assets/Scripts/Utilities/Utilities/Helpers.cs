using System;
using UnityEngine;

public class Helpers : MonoBehaviour
{
    public static bool IsValidIndex(int index, Array[] array)
    {
        return index >= 0 && index < array.Length;
    }
}

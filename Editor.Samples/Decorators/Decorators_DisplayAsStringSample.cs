using TriInspector;
using UnityEngine;

public class Decorators_DisplayAsStringSample : ScriptableObject
{
    [DisplayAsString]
    public string hello = "world";

    [DisplayAsString, HideLabel]
    public Texture2D texture;
}
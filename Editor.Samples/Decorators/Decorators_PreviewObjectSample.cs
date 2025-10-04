using System;
using TriInspector;
using UnityEngine;

public class Decorators_PreviewObjectSample : ScriptableObject
{
    [PreviewObject]
    public GameObject gameObj;

    [PreviewObject(Height = 100)]
    public Sprite sprite;

    public TextureInfo texInfo;

    [Serializable]
    [InlineProperty(LabelWidth = 90)]
    [DeclareHorizontalGroup("Row", Sizes = new float[] {0, 100})]
    [DeclareVerticalGroup("Row/Fields")]
    public struct TextureInfo
    {
        [Group("Row/Fields")] public Texture tex;
        [Group("Row/Fields")] public Vector2 offset;
        [Group("Row/Fields")] public Vector2 scale;

        [Group("Row"), ShowInInspector, PreviewObject(Height = 100, DrawDefaultField = false)]
        public Texture Preview
        {
            get => tex;
            set => tex = value;
        }
    }
}
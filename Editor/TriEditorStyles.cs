using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    public static class TriEditorStyles
    {
        private static GUIStyle _contentBox;
        private static GUIStyle _box;
        
        public static GUIStyle TabOnlyOne { get; } = "Tab onlyOne";
        public static GUIStyle TabFirst { get; } = "Tab first";
        public static GUIStyle TabMiddle { get; } = "Tab middle";
        public static GUIStyle TabLast { get; } = "Tab last";
        
        private static GUIStyle FallbackBox { get; } = "HelpBox";
        private static GUIStyle FallbackContentBox { get; } = "HelpBox";
        
        public static GUIStyle ContentBox
        {
            get
            {
                if (_contentBox == null)
                {
                    var backgroundTexture = LoadTexture("TriInspector_Content_Bg");

                    if (backgroundTexture == null)
                    {
                        _contentBox = new GUIStyle(FallbackContentBox)
                        {
                            border = new RectOffset(2, 2, 2, 2),
                        };
                    }
                    else
                    {
                        _contentBox = new GUIStyle
                        {
                            border = new RectOffset(2, 2, 2, 2),
                            normal =
                            {
                                background = backgroundTexture,
                            },
                        };
                    }
                }

                return _contentBox;
            }
        }

        public static GUIStyle Box
        {
            get
            {
                if (_box == null)
                {
                    var backgroundTexture = LoadTexture("TriInspector_Box_Bg");

                    if (backgroundTexture == null)
                    {
                        _box = new GUIStyle(FallbackBox)
                        {
                            border = new RectOffset(2, 2, 2, 2),
                        };
                    }
                    else
                    {
                        _box = new GUIStyle
                        {
                            border = new RectOffset(2, 2, 2, 2),
                            normal =
                            {
                                background = backgroundTexture,
                            },
                        };
                    }
                }

                return _box;
            }
        }

        private static Texture2D LoadTexture(string name)
        {
            name = EditorGUIUtility.isProSkin ? $"{name}_Dark" : name;
            
            var result = AssetDatabase.FindAssets($"{name} t:texture2D")[0];
            var path = AssetDatabase.GUIDToAssetPath(result);
            
            return (Texture2D) EditorGUIUtility.Load(path);
        }
    }
}
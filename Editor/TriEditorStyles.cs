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
        
        private static GUIStyle FallbackContentBox { get; } = "HelpBox";
        private static GUIStyle FallbackBox { get; } = "HelpBox";
        
        public static GUIStyle ContentBox
        {
            get
            {
                if (_contentBox == null)
                {
                    var backgroundTexture = LoadTexture("TriInspector_Content_Bg");

                    if (backgroundTexture == null)
                    {
                        _contentBox = new GUIStyle(FallbackContentBox);
                    }
                    else
                    {
                        _contentBox = new GUIStyle
                        {
                            normal =
                            {
                                background = backgroundTexture,
                            },
                        };
                    }

                    _contentBox.border = new RectOffset(2, 2, 2, 2);
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
                        _box = new GUIStyle(FallbackBox);
                    }
                    else
                    {
                        _box = new GUIStyle
                        {
                            normal =
                            {
                                background = backgroundTexture,
                            },
                        };
                    }
                    
                    _box.border = new RectOffset(2, 2, 2, 2);
                }

                return _box;
            }
        }

        private static Texture2D LoadTexture(string name)
        {
            name = EditorGUIUtility.isProSkin ? $"{name}_Dark" : name;
            
            var results = AssetDatabase.FindAssets($"{name} t:texture2D");
            
            if (results.Length == 0) return null;
            
            var path = AssetDatabase.GUIDToAssetPath(results[0]);
            
            return (Texture2D) EditorGUIUtility.Load(path);
        }
    }
}
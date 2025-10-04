using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(PreviewObjectDrawer), TriDrawerOrder.Decorator,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class PreviewObjectDrawer : TriAttributeDrawer<PreviewObjectAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!typeof(Object).IsAssignableFrom(propertyDefinition.FieldType))
            {
                return "[PreviewObject] valid only on Objects";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            var previewSize = GetPreviewSize();

            if (!Attribute.DrawDefaultField)
            {
                return previewSize;
            }

            var contentWidth = width - previewSize;
            var contentHeight = base.GetHeight(contentWidth, property, next);

            return Mathf.Max(contentHeight, previewSize);
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var previewSize = GetPreviewSize();

            if (Attribute.DrawDefaultField)
            {
                var contentRect = new Rect(position)
                {
                    xMax = position.xMax - previewSize,
                    height = base.GetHeight(position.width - previewSize, property, next),
                };
                var previewRect = new Rect(position)
                {
                    xMin = contentRect.xMax,
                    height = previewSize,
                };

                base.OnGUI(contentRect, property, next);
                DrawPreview(previewRect, property);
            }
            else
            {
                var previewRect = new Rect(position)
                {
                    width = previewSize,
                    height = previewSize,
                };

                DrawPreview(previewRect, property);
            }
        }

        private void DrawPreview(Rect previewRect, TriProperty property)
        {
            var assetToPreview = (Object) property.Value;

            if (Event.current.type == EventType.Repaint)
            {
                EditorStyles.objectField.Draw(previewRect, false, false, false, false);
            }

            if (assetToPreview == null)
            {
                return;
            }

            const int previewContentPadding = 2;
            var previewContentRect = new Rect(previewRect)
            {
                xMin = previewRect.xMin + previewContentPadding,
                xMax = previewRect.xMax - previewContentPadding,
                yMin = previewRect.yMin + previewContentPadding,
                yMax = previewRect.yMax - previewContentPadding,
            };

            var previewTexture = AssetPreview.GetAssetPreview(assetToPreview);

            EditorGUI.DrawPreviewTexture(previewContentRect, previewTexture, null, ScaleMode.ScaleToFit);

            if (AssetPreview.IsLoadingAssetPreview(assetToPreview.GetInstanceID()))
            {
                property.PropertyTree.RequestRepaint();
            }
        }

        private float GetPreviewSize()
        {
            return Attribute.Height > 0 ? Attribute.Height : EditorGUIUtility.singleLineHeight * 4;
        }
    }
}
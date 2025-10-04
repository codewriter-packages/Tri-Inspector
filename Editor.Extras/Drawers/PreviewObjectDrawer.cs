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

            const int previewContentPadding = 2;
            var previewContentRect = new Rect(previewRect)
            {
                xMin = previewRect.xMin + previewContentPadding,
                xMax = previewRect.xMax - previewContentPadding,
                yMin = previewRect.yMin + previewContentPadding,
                yMax = previewRect.yMax - previewContentPadding,
            };

            if (assetToPreview == null)
            {
                GUI.Label(previewContentRect, "None", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (assetToPreview is Sprite sprite)
            {
                DrawSpritePreview(previewContentRect, sprite);
                return;
            }

            DrawAssetPreview(previewContentRect, assetToPreview, property);
        }

        private void DrawAssetPreview(Rect position, Object assetToPreview, TriProperty property)
        {
            var previewTexture = AssetPreview.GetAssetPreview(assetToPreview);

            if (previewTexture != null)
            {
                EditorGUI.DrawPreviewTexture(position, previewTexture, null, ScaleMode.ScaleToFit);
            }

            if (AssetPreview.IsLoadingAssetPreview(assetToPreview.GetInstanceID()))
            {
                property.PropertyTree.RequestRepaint();
            }
        }

        private static void DrawSpritePreview(Rect position, Sprite sprite)
        {
            var texRect = sprite.textureRect;
            var fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
            var invFullSize = new Vector2(1f / fullSize.x, 1f / fullSize.y);
            var size = new Vector2(texRect.width, texRect.height);
            var invSize = new Vector2(1f / size.x, 1f / size.y);

            var coords = new Rect(
                Vector2.Scale(texRect.position, invFullSize),
                Vector2.Scale(texRect.size, invFullSize));
            var ratio = Vector2.Scale(position.size, invSize);
            var minRatio = Mathf.Min(ratio.x, ratio.y);

            var center = position.center;
            position.size = size * minRatio;
            position.center = center;

            GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
        }

        private float GetPreviewSize()
        {
            return Attribute.Height > 0 ? Attribute.Height : EditorGUIUtility.singleLineHeight * 4;
        }
    }
}
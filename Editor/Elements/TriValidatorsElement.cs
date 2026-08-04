using System;
using System.Collections.Generic;
using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    internal sealed class TriValidatorsElement : TriElement
    {
        private readonly TriElement _next;
        private readonly TriPropertyValidationResultElement _results;

        public TriValidatorsElement(TriProperty property, TriElement next)
        {
            _next = next;
            _results = new TriPropertyValidationResultElement(property);

            AddChild(_results);
            AddChild(next);
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            var container = new VisualElement();
            container.Add(_results.CreateVisualElement(property));
            container.Add(TriNativeProjection.ProjectField(property, _next));
            return container;
        }
    }

    internal sealed class TriPropertyValidationResultElement : TriElement
    {
        private readonly TriProperty _property;
        private IReadOnlyList<TriValidationResult> _validationResults;

        public TriPropertyValidationResultElement(TriProperty property)
        {
            _property = property;
        }

        public override float GetHeight(float width)
        {
            if (ChildrenCount == 0)
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            return base.GetHeight(width);
        }

        public override bool Update()
        {
            var dirty = base.Update();

            dirty |= GenerateValidationResults();

            return dirty;
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            var container = new VisualElement();
            IReadOnlyList<TriValidationResult> cachedResults = null;

            void Rebuild()
            {
                if (ReferenceEquals(_property.ValidationResults, cachedResults))
                {
                    return;
                }

                cachedResults = _property.ValidationResults;
                container.Clear();

                var hasResults = cachedResults.Count != 0;
                container.style.marginTop = hasResults ? EditorGUIUtility.standardVerticalSpacing + 5 : 0;
                container.style.marginBottom = hasResults ? EditorGUIUtility.standardVerticalSpacing : 0;

                foreach (var result in cachedResults)
                {
                    container.Add(CreateResultElement(result));
                }
            }

            Rebuild();
            container.schedule.Execute(Rebuild).Every(100);

            return container;
        }

        private VisualElement CreateResultElement(TriValidationResult result)
        {
            return new TriInfoBoxVisualElement(
                result.Message,
                result.MessageType,
                result.FixAction != null ? () => ExecuteFix(result.FixAction) : null,
                result.FixActionContent?.text);
        }

        private bool GenerateValidationResults()
        {
            if (ReferenceEquals(_property.ValidationResults, _validationResults))
            {
                return false;
            }

            _validationResults = _property.ValidationResults;

            RemoveAllChildren();

            foreach (var result in _validationResults)
            {
                var infoBox = result.FixAction != null
                    ? new TriInfoBoxElement(result.Message, result.MessageType,
                        inlineAction: () => ExecuteFix(result.FixAction),
                        inlineActionContent: result.FixActionContent)
                    : new TriInfoBoxElement(result.Message, result.MessageType);

                AddChild(infoBox);
            }

            return true;
        }

        private void ExecuteFix(Action fixAction)
        {
            _property.ModifyAndRecordForUndo(targetIndex => fixAction?.Invoke());
        }
    }

    internal sealed class TriInfoBoxVisualElement : VisualElement
    {
        private const float ActionWidth = 100f;

        public TriInfoBoxVisualElement(string message, TriMessageType type, Action fixAction = null,
            string fixActionText = null)
        {
            var isPro = EditorGUIUtility.isProSkin;
            var tint = GetColor(type);
            var baseGray = isPro ? 0.3f : 0.9f;
            var baseAlpha = isPro ? 0.65f : 0.5f;
            var borderGray = isPro ? 0.12f : 0.6f;
            var borderColor = new Color(borderGray * tint.r, borderGray * tint.g, borderGray * tint.b, baseAlpha);

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.paddingLeft = style.paddingRight = style.paddingTop = style.paddingBottom = 2;
            style.backgroundColor = new Color(baseGray * tint.r, baseGray * tint.g, baseGray * tint.b, baseAlpha);
            style.borderTopWidth = style.borderLeftWidth = style.borderRightWidth = 1;
            style.borderTopColor = style.borderLeftColor = style.borderRightColor = borderColor;

            var icon = EditorGUIUtilityProxy.GetHelpIcon(GetMessageType(type));
            if (icon != null)
            {
                Add(new Image
                {
                    image = icon,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        width = 18,
                        height = 18,
                        flexShrink = 0,
                        marginRight = 2,
                    },
                });
            }

            Add(new Label(message)
            {
                style =
                {
                    fontSize = 11,
                    flexGrow = 1,
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
            });

            if (fixAction != null)
            {
                Add(new Button(fixAction)
                {
                    text = fixActionText,
                    style =
                    {
                        width = ActionWidth,
                        flexShrink = 0,
                        marginLeft = 5,
                        whiteSpace = WhiteSpace.Normal,
                    },
                });
            }
        }

        private static Color GetColor(TriMessageType type)
        {
            switch (type)
            {
                case TriMessageType.Error: return new Color(1f, 0.4f, 0.4f);
                case TriMessageType.Warning: return new Color(1f, 0.8f, 0.2f);
                default: return Color.white;
            }
        }

        private static MessageType GetMessageType(TriMessageType type)
        {
            switch (type)
            {
                case TriMessageType.Info: return MessageType.Info;
                case TriMessageType.Warning: return MessageType.Warning;
                case TriMessageType.Error: return MessageType.Error;
                default: return MessageType.None;
            }
        }
    }
}
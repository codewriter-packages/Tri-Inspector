using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[assembly: RegisterTriAttributeDrawer(typeof(PreviewMeshDrawer), TriDrawerOrder.Drawer,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class PreviewMeshDrawer : TriAttributeDrawer<PreviewMeshAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new PreviewMeshVisualElement(property, Attribute);
        }

        private class PreviewMeshVisualElement : TriHeaderBoxedVisualElement
        {
            private readonly MeshPreviewVisualElement _preview;

            public PreviewMeshVisualElement(TriProperty property, PreviewMeshAttribute attribute)
                : base(property, attribute.UseFoldout, BuildObjectField(property, attribute.UseFoldout))
            {
                _preview = new MeshPreviewVisualElement(property, attribute.Height, attribute.Width,
                    attribute.UseFoldout, attribute.RotationMethod);

                Content.Add(_preview);
            }

            protected override void OnExpandedChanged(bool expanded)
            {
                _preview.RefreshVisibility();
            }

            private static ObjectField BuildObjectField(TriProperty property, bool useFoldout)
            {
                var field = new ObjectField
                {
                    objectType = property.FieldType,
                    allowSceneObjects = property.PropertyTree.TargetIsPersistent == false,
                };

                field.BindTri(property, hideLabel: useFoldout);
                return field;
            }
        }

        private class MeshPreviewVisualElement : VisualElement
        {
            private readonly int _height;
            private readonly int _width;
            private readonly TriProperty _property;
            private readonly bool _useFoldout;
            private readonly PreviewMeshRotationMethod _rotationMethod;
            private readonly IMGUIContainer _imgui;

            private PreviewRenderUtility _previewUtility;
            private static Material _mat;

            private Material GetMat
            {
                get
                {
                    if (_mat == null)
                    {
                        var shaderNames = new[]
                        {
                            "Universal Render Pipeline/Lit",
                            "Standard",
                            "Legacy Shaders/Diffuse",
                        };

                        var shader = shaderNames.Select(name => Shader.Find(name)).First(shader => shader != null);

                        _mat = new Material(shader)
                        {
                            hideFlags = HideFlags.HideAndDontSave,
                            color = new Color(0.4f, 0.7f, 0.4f),
                        };
                    }

                    return _mat;
                }
            }

            private readonly float _c_ROTATION_SENSITIVITY = -0.5f;
            private readonly float _c_ZOOM_SENSITIVITY = 0.1f;
            private readonly float _c_ZOOM_SENSITIVITY_MIN = 2f;
            private readonly float _c_ZOOM_SENSITIVITY_MAX = 10f;
            private readonly float _c_MIN_WIDTH = 50f;
            private readonly float _c_DEFAULT_CAMERA_DISTANCE = 4f;

            private Quaternion _previewQuaternion;
            private Mesh _sharedMesh;
            private float _distance;

            private Vector2 _previewDir = new(-20f, 0f);

            #region Initialization

            public MeshPreviewVisualElement(TriProperty property, int height, int width, bool useFoldout,
                PreviewMeshRotationMethod rotationMethod)
            {
                _property = property;
                _height = height;
                _width = width;
                _useFoldout = useFoldout;
                _rotationMethod = rotationMethod;

                _imgui = new IMGUIContainer(OnPreviewGUI);
                _imgui.style.height = _height;
                Add(_imgui);

                style.display = DisplayStyle.None;
                style.width = width;
                style.height = height;

                RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
                RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            }

            private void OnAttachToPanel(AttachToPanelEvent evt)
            {
                _previewUtility = new();
                _property.ValueChanged += OnValueChanged;

                // Setup lights
                _previewUtility.lights[0].intensity = 1.3f;
                _previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
                _previewUtility.lights[1].intensity = 1.3f;

                // Setup camera
                _previewUtility.cameraFieldOfView = 30f;
                _previewUtility.camera.nearClipPlane = 0.1f;
                _previewUtility.camera.farClipPlane = 100f;
                _previewUtility.camera.backgroundColor = Color.black;
                _previewUtility.camera.clearFlags = CameraClearFlags.Color;

                GetMeshObject();
                RefreshVisibility();
            }

            private void OnDetachFromPanel(DetachFromPanelEvent evt)
            {
                _previewUtility?.Cleanup();
                _previewUtility = null;

                _property.ValueChanged -= OnValueChanged;
            }

            private void OnValueChanged(TriProperty property)
            {
                GetMeshObject();
                RefreshVisibility();
            }

            public void RefreshVisibility()
            {
                var shouldShow = _sharedMesh != null && (!_useFoldout || _property.IsExpanded);
                style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            private void OnPreviewGUI()
            {
                if (_sharedMesh == null || _previewUtility == null)
                {
                    return;
                }

                var containerWidth = _imgui.contentRect.width;
                var currentWidth = _width == -1 ? containerWidth : _width;
                currentWidth = Mathf.Max(currentWidth, _c_MIN_WIDTH);

                if (containerWidth <= 0f)
                {
                    return;
                }

                var position = new Rect(0f, 0f, currentWidth, _height);
                _previewUtility.BeginPreview(position, GUIStyle.none);
                _previewUtility.DrawMesh(_sharedMesh, Matrix4x4.TRS(Vector3.zero, _previewQuaternion, Vector3.one),
                    GetMat, 0);
                _previewUtility.camera.Render();

                Texture result = _previewUtility.EndPreview();

                if (result)
                {
                    GUI.DrawTexture(position, result, ScaleMode.ScaleToFit, false);
                }

                if (position.Contains(Event.current.mousePosition))
                {
                    HandleMouseEvent(Event.current);
                }
            }

            #endregion


            #region Helper Function

            private void GetMeshObject()
            {
                var obj = _property.Value as Object;

                if (obj == null)
                {
                    _sharedMesh = null;
                    return;
                }

                _previewQuaternion = Quaternion.Euler(_previewDir);
                _distance = _c_DEFAULT_CAMERA_DISTANCE;

                // Draw supported types
                if (obj is Mesh mesh)
                {
                    _sharedMesh = mesh;
                }
                else if (obj is GameObject go)
                {
                    var mf = go.GetComponentInChildren<MeshFilter>();
                    if (mf != null)
                    {
                        _sharedMesh = mf.sharedMesh;
                    }
                    else
                    {
                        Debug.Log("No MeshFilter found on GameObject.");
                    }
                }

                if (_sharedMesh != null)
                {
                    UpdatePreviewCamera();
                }
            }

            private void UpdatePreviewCamera()
            {
                var bounds = _sharedMesh.bounds;

                // fallback bounds
                if (bounds.size == Vector3.zero)
                {
                    bounds = new Bounds(Vector3.zero, Vector3.one);
                }

                var magnitude = bounds.extents.magnitude;
                _previewUtility.camera.transform.position = bounds.center + Vector3.back * (magnitude * _distance);
                _previewUtility.camera.transform.LookAt(bounds.center);
            }

            private void HandleMouseEvent(Event mouseEvent)
            {
                var shift = mouseEvent.shift;

                switch (mouseEvent.type)
                {
                    case EventType.MouseDrag:
                        var cameraMovement = mouseEvent.delta * _c_ROTATION_SENSITIVITY;
                        HandlePreviewCameraRotation(cameraMovement);
                        mouseEvent.Use();
                        _imgui.MarkDirtyRepaint();
                        break;

                    case EventType.ScrollWheel:
                        _distance = Mathf.Clamp(_distance + mouseEvent.delta.x * _c_ZOOM_SENSITIVITY,
                            _c_ZOOM_SENSITIVITY_MIN, _c_ZOOM_SENSITIVITY_MAX);
                        if (shift)
                        {
                            UpdatePreviewCamera();
                            mouseEvent.Use();
                            _imgui.MarkDirtyRepaint();
                        }

                        break;

                    default:
                        return;
                }
            }

            private void HandlePreviewCameraRotation(Vector2 movement)
            {
                float pitch = movement.y;
                float yaw = movement.x;

                switch (_rotationMethod)
                {
                    case PreviewMeshRotationMethod.Clamped:
                        _previewDir.x = Mathf.Clamp(pitch + _previewDir.x, -90f, 90);
                        _previewDir.y += yaw;
                        _previewQuaternion = Quaternion.Euler(_previewDir.x, 0, 0) *
                                             Quaternion.Euler(0, _previewDir.y, 0);
                        break;
                    case PreviewMeshRotationMethod.Freeform:
                        _previewQuaternion = Quaternion.Euler(pitch, yaw, 0) * _previewQuaternion;
                        break;
                }

                _previewQuaternion = Quaternion.Normalize(_previewQuaternion);
            }

            #endregion
        }
    }
}
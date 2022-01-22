# Tri Inspector [![Github license](https://img.shields.io/github/license/codewriter-packages/Tri-Inspector.svg?style=flat-square)](#) [![Unity 2020.3](https://img.shields.io/badge/Unity-2019.3+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/Tri-Inspector?style=flat-square)
_Advanced inspector attributes for Unity_

### Usage

```csharp
using System;
using TriInspector;
using UnityEngine;

public class BasicSample : MonoBehaviour
{
    [PropertyOrder(1)]
    [HideLabel, LabelText("My Label"), LabelWidth(100)]
    [GUIColor(0, 1, 0), Space, Indent, ReadOnly]
    [Title("My Title"), Header("My Header")]
    [PropertySpace(SpaceBefore = 10, SpaceAfter = 20)]
    [PropertyTooltip("My Tooltip")]
    public float unityField;
    
    [Required]
    public Material mat;

    [InlineEditor]
    public SampleScriptableObject objectReference;

    [HideInPlayMode, ShowInPlayMode]
    [DisableInPlayMode, EnableInPlayMode]
    [HideInEditMode, ShowInEditMode]
    [DisableInEditMode, EnableInEditMode]
    public float conditional;

    [PropertyOrder(3)]
    [EnableInPlayMode]
    [Button("Click Me!")]
    private void CustomButton()
    {
        Debug.Log("Button clicked!");
    }
    
    [ShowInInspector]
    public float ReadonlyProperty => 123f;

    [ShowInInspector]
    public float EditableProperty
    {
        get => unityField;
        set => unityField = value;
    }

    [InlineProperty(LabelWidth = 60)]
    public Config config = new Config();

    [Serializable]
    public class Config
    {
        public Vector3 position;
        public float rotation;
    }
}

[DeclareBoxGroup("body")]
[DeclareHorizontalGroup("header")]
[DeclareBoxGroup("header/left", Title = "My Left Box")]
[DeclareBoxGroup("header/right", Title = "My Right Box")]
[DeclareTabGroup("tabs")]
public class GroupDemo : MonoBehaviour
{
    [Group("header/left")] public string h1;
    [Group("header/left")] public string h2;

    [Group("header/right")] public string h3;
    [Group("header/right")] public string h4;

    [Group("body")] public string b1;
    [Group("body")] public string b2;

    [Group("tabs"), Tab("One")] public float tabOne;
    [Group("tabs"), Tab("Two")] public float tabTwo;
}
```

### Customization

#### Custom Drawers

<details>
  <summary>Custom Value Drawer</summary>

```csharp
using TriInspector;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriValueDrawer(typeof(BoolDrawer), TriDrawerOrder.Fallback)]

public class BoolDrawer : TriValueDrawer<bool>
{
    public override float GetHeight(float width, TriValue<bool> propertyValue, TriElement next)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, TriValue<bool> propertyValue, TriElement next)
    {
        var value = propertyValue.Value;

        EditorGUI.BeginChangeCheck();

        value = EditorGUI.Toggle(position, propertyValue.Property.DisplayNameContent, value);

        if (EditorGUI.EndChangeCheck())
        {
            propertyValue.Value = value;
        }
    }
}
```
</details>

<details>
  <summary>Custom Attribute Drawer</summary>

```csharp
using TriInspector;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(LabelWidthDrawer), TriDrawerOrder.Decorator)]

public class LabelWidthDrawer : TriAttributeDrawer<LabelWidthAttribute>
{
    public override void OnGUI(Rect position, TriProperty property, TriElement next)
    {
        var oldLabelWidth = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = Attribute.Width;
        next.OnGUI(position);
        EditorGUIUtility.labelWidth = oldLabelWidth;
    }
}
```
</details>

<details>
  <summary>Custom Group Drawer</summary>

```csharp
using TriInspector;
using TriInspector.Elements;

[assembly: RegisterTriGroupDrawer(typeof(TriBoxGroupDrawer))]

public class TriBoxGroupDrawer : TriGroupDrawer<DeclareBoxGroupAttribute>
{
    public override TriPropertyCollectionBaseElement CreateElement(DeclareBoxGroupAttribute attribute)
    {
        // ...
    }
}
```
</details>

#### Validators

<details>
  <summary>Custom Value Validator</summary>

```csharp
using TriInspector;

[assembly: RegisterTriValueValidator(typeof(MissingReferenceValidator<>))]

public class MissingReferenceValidator<T> : TriValueValidator<T>
    where T : UnityEngine.Object
{
    public override TriValidationResult Validate(TriValue<T> propertyValue)
    {
        // ...
    }
}
```
</details>

<details>
  <summary>Custom Attribute Validators</summary>

```csharp
using TriInspector;

[assembly: RegisterTriAttributeValidator(typeof(RequiredValidator), ApplyOnArrayElement = true)]

public class RequiredValidator : TriAttributeValidator<RequiredAttribute>
{
    public override TriValidationResult Validate(TriProperty property)
    {
        // ...
    }
}
```
</details>

#### Property Processors

<details>
  <summary>Custom Property Hide Processor</summary>

```csharp
using TriInspector;
using UnityEngine;

[assembly: RegisterTriPropertyHideProcessor(typeof(HideInPlayModeProcessor))]

public class HideInPlayModeProcessor : TriPropertyHideProcessor<HideInPlayModeAttribute>
{
    public override bool IsHidden(TriProperty property)
    {
        return Application.isPlaying;
    }
}
```
</details>

<details>
  <summary>Custom Property Disable Processor</summary>

```csharp
using TriInspector;
using UnityEngine;

[assembly: RegisterTriPropertyDisableProcessor(typeof(DisableInPlayModeProcessor))]

public class DisableInPlayModeProcessor : TriPropertyDisableProcessor<DisableInPlayModeAttribute>
{
    public override bool IsDisabled(TriProperty property)
    {
        return Application.isPlaying;
    }
}
```
</details>

## How to Install
Minimal Unity Version is 2020.3.

Library distributed as git package ([How to install package from git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html))
<br>Git URL: `https://github.com/codewriter-packages/Tri-Inspector.git`

## License

Tri-Inspector is [MIT licensed](./LICENSE.md).
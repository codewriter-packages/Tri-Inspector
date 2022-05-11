# Tri Inspector [![Github license](https://img.shields.io/github/license/codewriter-packages/Tri-Inspector.svg?style=flat-square)](#) [![Unity 2020.3](https://img.shields.io/badge/Unity-2020.3+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/Tri-Inspector?style=flat-square)
_Advanced inspector attributes for Unity_

- [Attributes](#Attributes)
  - [Misc](#Misc)
  - [Validation](#Validation)
  - [Styling](#Styling)
  - [Collections](#Collections)
  - [Conditionals](#Conditionals)
  - [Buttons](#Buttons)
  - [Debug](#Debug)
  - [Groups](#Groups)
- [Customization](#Customization)
  - [Custom Drawers](#Custom-Drawers)
  - [Validators](#Validators)
  - [Property Processors](#Property-Processors)
- [How to Install](#How-to-Install)
- [License](#License)

## Attributes

### Misc
#### ShowInInspector

Shows non-serialized property in the inspector.

```csharp
private float field;

[ShowInInspector]
public float ReadOnlyProperty => field;

[ShowInInspector]
public float EditableProperty
{
    get => field;
    set => field = value;
}
```

#### PropertyOrder

Changes property order in the inspector.

```csharp
[PropertyOrder(1)]
```

#### ReadOnly

Makes property non-editable.

```csharp
[ReadOnly]
```

#### OnValueChanged

Invokes callback on property modification.

```csharp
[OnValueChanged(nameof(OnMaterialChanged))]
public Material mat; 

private void OnMaterialChanged()
{
    Debug.Log("Material changed!");
}
```

### Validation

Tri Inspector has some builtin validators such as `missing reference` and `type mismatch` error. 
Additionally you can mark out your code with validation attributes 
or even write own validators.

![Builtin](https://user-images.githubusercontent.com/26966368/167894126-ac5b4722-c930-4304-b183-4b8cc461f083.png)
#### Required
```csharp
[Required]
public Material mat;
```

![Required](https://user-images.githubusercontent.com/26966368/167895375-a1c31812-081f-4033-b7e4-a0c3c43963f0.png)

#### ValidateInput
```csharp
[ValidateInput(nameof(ValidateTexture))]
public Textute tex;

private TriValidationResult ValidateTexture()
{
    if (tex == null) return TriValidationResult.Error("Tex is null");
    return TriValidationResult.Valid;
}

```

![ValidateInput](https://user-images.githubusercontent.com/26966368/167895864-cb181383-6f23-4f7f-8c3b-b683760e1d8a.png)

### Styling

#### HideLabel
```csharp
[HideLabel]
```
![HideLabel](https://user-images.githubusercontent.com/26966368/167896272-577cbc8f-95be-4b75-97b6-b67d58eba4d1.png)

#### LabelText
```csharp
[LabelText("My Label")]
```

#### LabelWidth
```csharp
[LabelWidth(100)]
```

#### GUIColor
```csharp
[GUIColor(0, 1, 0)]
```

#### Space
```csharp
[Space]
```

#### Indent
```csharp
[Indent]
```

#### Title
```csharp
[Title("My Title")]
public int val;
```

![Title](https://user-images.githubusercontent.com/26966368/167898501-24a8c472-08b1-4010-b00e-ef7dcc33dfae.png)

#### Header
```csharp
[Header("My Header")]
```

#### PropertySpace
```csharp
[PropertySpace(SpaceBefore = 10, 
               SpaceAfter = 20)]
```

#### PropertyTooltip
```csharp
[PropertyTooltip("My Tooltip")]
```

#### InlineEditor
```csharp
[InlineEditor]
public Material mat;
```

![InlineEditor](https://user-images.githubusercontent.com/26966368/167896721-79724d1c-570f-4e01-b3e1-8c83aacca661.png)

#### InlineProperty
```csharp
public MinMax rangeFoldout;

[InlineProperty(LabelWidth = 40)]
public MinMax rangeInline;

[Serializable]
public class MinMax
{
    public int min;
    public int max;
}
```

![InlineProperty](https://user-images.githubusercontent.com/26966368/167899261-6a3ceeda-609e-47d0-b8a0-38f4331cc9f9.png)

### Collections

#### ListDrawerSettings
```csharp
[ListDrawerSettings(Draggable = true,
                    HideAddButton = false,
                    HideRemoveButton = false,
                    AlwaysExpanded = false)]
```

![ListDrawerSettings](https://user-images.githubusercontent.com/26966368/167897095-cde06fdb-8b4c-422c-92dc-8ed781006c6e.png)

### Conditionals

#### HideInPlayMode / ShowInPlayMode
```csharp
[HideInPlayMode] [ShowInPlayMode]
```

#### DisableInPlayMode / EnableInPlayMode
```csharp
[DisableInPlayMode] [EnableInPlayMode]
```

#### HideInEditMode / ShowInEditMode
```csharp
[HideInEditMode] [ShowInEditMode]
```

#### DisableInEditMode / EnableInEditMode
```csharp
[DisableInEditMode] [EnableInEditMode]
```

### Buttons

#### Button
```csharp
[Button("My Button")]
private void DoButton()
{
    Debug.Log("Button clicked!");
}
```

![Button](https://user-images.githubusercontent.com/26966368/167897368-79fdb050-a2f3-4c37-be3f-54f10f46880e.png)

### Debug

#### ShowDrawerChain
```csharp
[ShowDrawerChain]
```

### Groups

```csharp
[DeclareHorizontalGroup("header")]
[DeclareBoxGroup("header/left", Title = "My Left Box")]
[DeclareVerticalGroup("header/right")]
[DeclareBoxGroup("header/right/top", Title = "My Right Box")]
[DeclareTabGroup("header/right/tabs")]
[DeclareBoxGroup("body")]
public class GroupDemo : MonoBehaviour
{
    [Group("header/left")] public bool prop1;
    [Group("header/left")] public int prop2;
    [Group("header/left")] public string prop3;
    [Group("header/left")] public Vector3 prop4;

    [Group("header/right/top")] public string rightProp;

    [Group("body")] public string body1;
    [Group("body")] public string body2;

    [Group("header/right/tabs"), Tab("One")] public float tabOne;
    [Group("header/right/tabs"), Tab("Two")] public float tabTwo;
    [Group("header/right/tabs"), Tab("Three")] public float tabThree;

    [Group("header/right"), Button]
    public void MyButton()
    {
    }
}
```

![GroupDemo Preview](https://user-images.githubusercontent.com/26966368/151707658-2e0c2e33-17d5-4cbb-8f83-d7d394ced6b6.png)

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

After installing the package, you need to unpack the `Installer.unitypackage` that comes with the package

## License

Tri-Inspector is [MIT licensed](./LICENSE.md).
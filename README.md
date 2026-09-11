# Tri Inspector 2.0 [![Github license](https://img.shields.io/github/license/codewriter-packages/Tri-Inspector.svg?style=flat-square)](#) [![Unity 6000.0](https://img.shields.io/badge/Unity-6000.0+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/Tri-Inspector?style=flat-square) [![openupm](https://img.shields.io/npm/v/com.codewriter.triinspector?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.codewriter.triinspector/)

_Advanced inspector attributes for Unity_

- [What's New in 2.0](#whats-new-in-20)
- [Samples](#Samples)
- [Attributes](#Attributes)
- [Integrations](#Integrations) ([Odin Inspector](#Odin-Inspector), [Odin Validator](#Odin-Validator))
- [How to Install](#How-to-Install)
- [License](#License)

## What's New in 2.0-preview

> [!WARNING]
> **2.0 is currently a preview release** and may contain bugs. If you run into any issues, please [report them](https://github.com/codewriter-packages/Tri-Inspector/issues). If you need a stable experience, install the latest **1.x.x** release from the [`1.x.x` branch](https://github.com/codewriter-packages/Tri-Inspector/tree/1.x.x).

> Tri Inspector 2.0 is a significant milestone — rebuilt from the ground up on Unity's **UI Toolkit**, bringing a modern foundation, polished visuals, and long-requested features.

- 🎯 **Unity 6 and above** — minimum supported version is now Unity 6, allowing the project to adopt modern editor APIs without legacy baggage.
- 🖼️ **UI Toolkit based** — the entire rendering pipeline has been rewritten using UI Toolkit, replacing the old IMGUI backend.
- 📦 **Prefab workflow support** — Tri Inspector fully respects Unity's prefab override system, so modified properties are highlighted, revertable, and apply correctly across prefab instances and variants.
- ✨ **New styles for UI elements** — lists, groups, info boxes now have polished, consistent visual styles that not only integrate with Unity 6's editor theme, but redefine what an editor inspector can look like.
- 🛠️ **Overall improvements and bug fixes** — a wide range of edge-case bugs have been resolved and internal systems have been hardened, resulting in a more stable and predictable inspector experience.
- ↕️ **TableList is now reorderable** — rows in `[TableList]` can be dragged and reordered directly in the inspector, making list management significantly faster.
- 📖 **Dictionary support** — dictionaries can now be displayed and edited in the inspector across all supported Unity versions. Starting from Unity 6.6, dictionaries are also fully serializable by Unity itself.

<img width="1000" height="934" alt="Tri-Inspector-Demo" src="https://github.com/user-attachments/assets/381f65f4-2e0c-4419-9739-ad16fdd4cbfc" />

## Samples

TriInspector ships with a built-in **Samples Window** — your interactive playground for every attribute in the package. Open it from `Tools/Tri Inspector/Samples` and browse all attributes organized by category. Select any sample to instantly see a live inspector with real fields, so you can explore exactly how each attribute looks and behaves without writing a single line of code.

Each sample also displays its full source code right in the window, so you can see exactly how the attribute is used and copy it straight into your project. It's the fastest way to discover what TriInspector can do and understand how to apply it.

![Samples](https://user-images.githubusercontent.com/26966368/177045336-a3fcf438-3e70-45d0-b753-299e577b2010.png)

### Popular attributes

TriInspector comes packed with a huge collection of attributes — far too many to cover here in full. Below are some of the most frequently used ones to get you started. For the complete list, open the Samples Window.

#### ShowInInspector

Shows non-serialized property in the inspector.

![ShowInInspector](https://user-images.githubusercontent.com/26966368/168230693-a1a389a6-1a3b-4b94-b4b5-0764e88591f4.png)

#### ReadOnly

Makes property non-editable in the inspector.

![ReadOnly](https://user-images.githubusercontent.com/26966368/168231817-948ef153-eb98-42fb-88ad-3e8d17925b43.png)

#### Required

Marks a field as required and shows an error in the inspector when it is not assigned. Supports an optional fix action — a custom method that can perform any logic to resolve the issue, such as finding and assigning the missing reference automatically.

![Required](https://github.com/codewriter-packages/Tri-Inspector/assets/26966368/56a8d0ef-c88b-4b4b-8121-388b94d47841)

#### ValidateInput

Runs a custom validation method whenever the field value changes and displays the result as an error or warning directly in the inspector.

![ValidateInput](https://user-images.githubusercontent.com/26966368/168233592-b4dcd4d4-88ec-4213-a2e5-667719feb0b8.png)

#### InfoBox

Displays a static or dynamic message box above a property with configurable severity — info, warning, or error.

![InfoBox](https://user-images.githubusercontent.com/26966368/169318171-d1a02212-48f1-41d1-b0aa-e2e1b25df262.png)

#### Dropdown

Replaces a field's default input with a dropdown list of predefined values. Values can be static constants or generated dynamically at runtime.

![Dropdown](https://user-images.githubusercontent.com/26966368/230157088-1fec3c38-7046-4fc8-8da1-aca63744ac37.png)

#### Scene

Renders a string field as a scene picker dropdown populated from the project's build settings, eliminating typos and making scene references refactor-safe. TriInspector also includes other specialized field drawers of this kind, such as `[Layer]` for layer selection, `[AnimatorParameter]` for Animator parameters, `[MaterialProperty]` for shader properties, and more.

![Scene](https://user-images.githubusercontent.com/26966368/179394466-a9397212-e3bc-40f1-b721-8f7c43aa3048.png)

#### Slider

Renders a numeric field as a slider with configurable fixed or dynamic min and max bounds, giving designers an intuitive range control instead of a raw number input.

![Slider](https://github.com/user-attachments/assets/4f56d7e6-0032-4037-b890-740a3f93cebe)

#### MinMaxSlider

Renders a Vector2 field as a min-max range slider, letting you define both the lower and upper bound of a range with a single intuitive control.

![MinMaxSlider](https://github.com/user-attachments/assets/44deaa4e-5e26-49f0-bc8a-b84b9f48f08a)

#### InlineEditor

Embeds the full inspector of a referenced asset inline within the parent inspector, so you can view and edit nested assets without leaving the current selection.

![InlineEditor](https://user-images.githubusercontent.com/26966368/168234617-86a7f500-e635-46f8-90f2-5696e5ae7e63.png)

#### Preview Mesh

Shows an interactive 3D mesh preview below a GameObject field directly in the inspector, making it easy to visually verify the right mesh is assigned.

![Preview Mesh](https://github.com/user-attachments/assets/329bb723-d2fc-4e18-97e6-f47706b0eb46)

#### Title

Draws a bold header line above a property or button in the inspector, making it easy to visually separate and label sections of a large component.

![Title](https://user-images.githubusercontent.com/26966368/168528842-10ba070e-74ab-4377-8f33-7a55609494f4.png)

#### TableList

Renders a list or array as a compact multi-column table in the inspector, with support for drag reordering and custom column sizes — perfect for large structured datasets.

![TableList](https://user-images.githubusercontent.com/26966368/171125460-679fe467-cf01-47e0-8674-b565ee3d4d7e.png)

### Groups

Properties can be grouped in the inspector using the `[Group]` attribute. TriInspector supports six group types, each declared on the class with a corresponding `[Declare*Group]` attribute:

- **BoxGroup** — wraps properties in a titled or untitled box
- **FoldoutGroup** — collapses properties behind a foldout toggle
- **ToggleGroup** — a foldout with a built-in enable/disable toggle
- **TabGroup** — organises properties into named tabs, showing one at a time
- **HorizontalGroup** — lays properties out side by side in a row
- **VerticalGroup** — stacks properties vertically, typically nested inside a horizontal group

Groups can be freely nested and combined to build complex, multi-region inspector layouts.

![BoxGroup](https://user-images.githubusercontent.com/26966368/177552426-8124b445-e235-43a2-9143-dd5d954dd9f8.png)

![FoldoutGroup](https://user-images.githubusercontent.com/26966368/201517886-4138ee55-33c2-4a1a-93bc-a3cda7745a4c.png)

![ToggleGroup](https://user-images.githubusercontent.com/26966368/230786234-33e9aa51-c9da-4b50-93ca-05e72b54aa07.png)

![TabGroup](https://user-images.githubusercontent.com/26966368/177552003-528a4e52-e340-460b-93e6-f56c07ac063b.png)

![HorizontalGroup](https://user-images.githubusercontent.com/26966368/177551227-9df32c44-9482-4580-8144-5745af806f24.png)

![VerticalGroup](https://user-images.githubusercontent.com/26966368/177550644-9d0dc2b7-ed18-4d8f-997d-c4fff2c6d6cb.png)

## Integrations

### Odin Inspector

Tri Inspector is able to work in compatibility mode with Odin Inspector.
In this mode, the primary interface will be drawn by the Odin Inspector. However,
parts of the interface can be rendered by the Tri Inspector.

In order for the interface to be rendered by Tri instead of Odin,
it is necessary to mark classes with `[DrawWithTriInspector]` attribute.

Alternatively, you can mark the entire assembly with an attribute `[assembly:DrawWithTriInspector]`
to draw all types in the assembly using the Tri Inspector.

### Odin Validator

Tri Inspector is integrated with the Odin Validator
so all validation results from Tri attributes will be shown
in the Odin Validator window.

![Odin-Validator-Integration](https://user-images.githubusercontent.com/26966368/169645537-d8f0b50f-46af-4804-95e8-337ff3b5ae83.png)

## How to Install

Library distributed as git package ([How to install package from git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html))
<br>Git URL: `https://github.com/codewriter-packages/Tri-Inspector.git`

> **Localization package dependency**<br/>
> Tri Inspector automatically installs [Localization package](https://docs.unity3d.com/Packages/com.unity.localization@1.0/manual/index.html) as dependency.<br/>
> If you are not using localization package and do not want to install it, you can install a stub package instead.<br/>
> Git URL: https://github.com/codewriter-packages/Unity-Localization-Stub-for-Tri-Inspector.git

## License

Tri-Inspector is [MIT licensed](./LICENSE.md).

# Tri Inspector [![Github license](https://img.shields.io/github/license/codewriter-packages/Tri-Inspector.svg?style=flat-square)](#) [![Unity 2019.3](https://img.shields.io/badge/Unity-2019.3+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/Tri-Inspector?style=flat-square)
_Advanced inspector attributes for Unity_

```csharp
using System;
using TriInspector;
using UnityEngine;

public class BasicSample : TriMonoBehaviour
{
    [PropertyOrder(1)]
    [HideLabel, LabelText("My Label"), LabelWidth(100)]
    [GUIColor(0, 1, 0), Space, Indent, ReadOnly]
    [Title("My Title"), Header("My Header")]
    [PropertySpace(SpaceBefore = 10, SpaceAfter = 20)]
    [PropertyTooltip("My Tooltip")]
    public float unityField;

    [HideInPlayMode, ShowInPlayMode]
    [DisableInPlayMode, EnableInPlayMode]
    public float conditional;

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
public class GroupDemo : TriMonoBehaviour
{
    [Group("header/left")] public string h1;
    [Group("header/left")] public string h2;

    [Group("header/right")] public string h3;
    [Group("header/right")] public string h4;

    [Group("body")] public string b1;
    [Group("body")] public string b2;
}
```

## How to Install
Minimal Unity Version is 2019.3.

Library distributed as git package ([How to install package from git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html))
<br>Git URL: `https://github.com/codewriter-packages/Tri-Inspector.git`

## License

Tri-Inspector is [MIT licensed](./LICENSE.md).
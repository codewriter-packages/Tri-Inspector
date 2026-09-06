#if UNITY_6000_6_OR_NEWER

using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public class Collections_DictionaryDrawerSettingsSample : ScriptableObject
{
    [SerializeField]
    public Dictionary<string, int> defaultLayout;

    [SerializeField, DictionaryDrawerSettings(
         AlwaysExpanded = true, KeyColumnSize = 0.25f)]
    public Dictionary<string, CharacterStats> labeledColumns = new()
    {
        {"archer", new CharacterStats {health = 80, damage = 120}},
        {"warrior", new CharacterStats {health = 200, damage = 60}},
        {"sniper", new CharacterStats {health = 60, damage = 180}},
    };

    [SerializeField, DictionaryDrawerSettings(
         Layout = DictionaryLayout.OneColumnWithValueFoldout,
         KeyLabel = "ID", ValueLabel = "Stats")]
    public Dictionary<string, CharacterStats> foldoutLayout;

    [SerializeField, DictionaryDrawerSettings(
         Layout = DictionaryLayout.OneColumnWithValueVisible,
         KeyLabel = "")]
    public Dictionary<string, CharacterStats> inlineLayout;

    [SerializeField, DictionaryDrawerSettings(
         Draggable = false,
         HideArraySize = true,
         HideAddButton = true,
         HideRemoveButton = true,
         AlwaysExpanded = true)]
    public Dictionary<string, Material> previewLayout = new()
    {
        ["first"] = null,
        ["second"] = null,
    };

    [Serializable]
    public struct CharacterStats
    {
        public int health;
        public int damage;
    }
}

#endif
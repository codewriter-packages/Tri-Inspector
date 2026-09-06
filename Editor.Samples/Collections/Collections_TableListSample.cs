using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class Collections_TableListSample : ScriptableObject
{
    [TableList(Draggable = true,
        HideAddButton = false,
        HideRemoveButton = false,
        AlwaysExpanded = false)]
    public List<TableItem> table;

    [FormerlySerializedAs("characterStats")] [TableList(Labels = new[] {""}, Sizes = new[] {22, 100, 0.4f, 0.6f})]
    public List<CharacterInfo> characters = new List<CharacterInfo>
    {
        new CharacterInfo {id = "archer", enabled = true, range = new Vector2(50, 100),},
        new CharacterInfo {id = "sniper", enabled = false, range = new Vector2(100, 200),},
        new CharacterInfo {id = "warrior", enabled = false, range = new Vector2(0, 2),},
    };

    [Serializable]
    public class TableItem
    {
        [Required]
        public Texture icon;

        public string description;

        [Group("Combined"), LabelWidth(16)]
        public string A, B, C;

        [Button, Group("Actions")]
        public void Test1()
        {
        }

        [Button, Group("Actions")]
        public void Test2()
        {
        }
    }

    [Serializable]
    public struct CharacterInfo
    {
        public bool enabled;

        public string id;

        [InlineButton("Search", nameof(SearchPrefabById))]
        public GameObject prefab;

        [MinMaxSlider(0, 250)]
        public Vector2 range;

        private void SearchPrefabById()
        {
        }
    }
}
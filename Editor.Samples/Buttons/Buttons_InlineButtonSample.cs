using UnityEngine;

namespace TriInspector.Editor.Samples.Buttons
{
    /// <summary>
    /// Adds a clickable button inline next to a property field in the inspector.
    /// </summary>
    public class Buttons_InlineButtonSample : ScriptableObject
    {
       [InlineButton("click add age",nameof(Add))]
        public int age;

        void Add()
        {
            age++;
            Debug.Log(age);
        }
    }
}
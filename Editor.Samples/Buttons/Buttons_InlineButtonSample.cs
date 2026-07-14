using UnityEngine;

namespace TriInspector.Editor.Samples.Buttons
{
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
using TriInspector;
using UnityEngine;

public class Validators_RegexSample : ScriptableObject
{
    [PropertySpace(SpaceAfter = 10)]
    public string expression = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";

    [Regex("$" + nameof(expression), DynamicExpression = true)]
    public string validEmail = "sample_mail123@email.com";

    [Regex(nameof(expression), DynamicExpression = true, 
        PreviewExpression = true, Example = "example_88@email.com")]
    public string invalidEmail = "sample_mail123email.com";
}
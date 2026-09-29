using System;

namespace TriInspector
{
    public static class TriValidationProfilerHook
    {
        // (validatorTypeName, property, elapsedTicks)
        public static Action<string, TriProperty, long> onValidatorRun;
    }
}

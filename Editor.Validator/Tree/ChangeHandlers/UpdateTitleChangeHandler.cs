using System.Linq;
using TriInspector.Utilities;
using UnityEngine;

namespace TriInspector.Validator.ChangeHandlers
{
    internal class UpdateTitleChangeHandler : TriValidationControllerChangeHandler
    {
        private readonly TriValidationTreeModel _model;
        private readonly TriValidationWindow _window;

        public int ErrorCount => _model.Roots.Sum(root => root.Data.errorCount);
        public int WarningCount => _model.Roots.Sum(root => root.Data.warningCount);

        public UpdateTitleChangeHandler(TriValidationTreeModel model, TriValidationWindow window)
        {
            _model = model;
            _window = window;
        }

        public override void HandleChanged()
        {
            UpdateTitle();
        }

        public void UpdateTitle()
        {
            var errors = ErrorCount;
            var warnings = WarningCount;

            _window.titleContent = CreateTitleContent(errors, warnings);
            _window.Repaint();
        }

        private static GUIContent CreateTitleContent(int errors, int warnings)
        {
            return errors > 0 ? new GUIContent($"Tri [{errors}]", TriMessageType.Error.GetIcon()) :
                warnings > 0 ? new GUIContent($"Tri [{warnings}]", TriMessageType.Warning.GetIcon()) :
                new GUIContent("Tri Validator");
        }
    }
}
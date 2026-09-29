using System.Collections.Generic;

namespace TriInspector.Validator.ChangeHandlers
{
    internal class PopulateHierarchyCountsChangeHandler : TriValidationControllerChangeHandler
    {
        private readonly TriValidationTreeModel _model;

        // Reused buffer for the Hierarchy-column snapshot so we don't allocate a fresh dictionary on every change.
        private readonly Dictionary<TriEntityID, (int error, int warning)> _hierarchyCounts = new();

        public PopulateHierarchyCountsChangeHandler(TriValidationTreeModel model)
        {
            _model = model;
        }

        public override void HandleChanged()
        {
            PopulateGameObjectCounts();
            TriValidationHierarchyCounts.Publish(_hierarchyCounts);
        }

        private void PopulateGameObjectCounts()
        {
            _hierarchyCounts.Clear();

            foreach (var root in _model.Roots)
            {
                AccumulateCounts(root, _hierarchyCounts);
            }
        }

        private static void AccumulateCounts(
            TriValidationNode node, Dictionary<TriEntityID, (int error, int warning)> counts)
        {
            if (node.Data.kind == TriValidationNodeKind.GameObject && node.Data.target != null)
            {
                // The Hierarchy columns show only this GameObject's own issues — the sum of its direct
                // Component children — not the rolled-up subtree total the validator window displays.
                var selfError = 0;
                var selfWarning = 0;
                foreach (var child in node.Children)
                {
                    if (child.Data.kind == TriValidationNodeKind.Component)
                    {
                        selfError += child.Data.errorCount;
                        selfWarning += child.Data.warningCount;
                    }
                }

                if (selfError > 0 || selfWarning > 0)
                {
                    counts[node.Data.target.GetTriEntityID()] = (selfError, selfWarning);
                }
            }

            foreach (var child in node.Children)
            {
                AccumulateCounts(child, counts);
            }
        }
    }
}
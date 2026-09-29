namespace TriInspector.Validator
{
    /// <summary>
    /// The single place that rolls per-severity issue counts up the tree and derives a branch node's
    /// severity from them. Leaf (Issue) counts are authoritative and never touched here; every branch node
    /// is just the sum of its children.
    /// </summary>
    internal static class TriValidationTreeCounts
    {
        /// <summary>
        /// Post-order recompute of an entire subtree. Use after a bulk change where several nodes at
        /// different depths were edited at once and a single upward walk wouldn't see the deeper edits.
        /// </summary>
        public static void Recompute(TriValidationNode node)
        {
            // Issue leaves carry authoritative counts; SceneAsset and Prefab leaves carry their rolled-up
            // count-only totals. None has children to sum, so leave their preset counts untouched.
            if (node.Data.kind == TriValidationNodeKind.Issue ||
                node.Data.kind == TriValidationNodeKind.SceneAsset ||
                node.Data.kind == TriValidationNodeKind.PrefabAsset)
            {
                return;
            }

            var errors = 0;
            var warnings = 0;
            var infos = 0;

            foreach (var child in node.Children)
            {
                Recompute(child);
                errors += child.Data.errorCount;
                warnings += child.Data.warningCount;
                infos += child.Data.infoCount;
            }

            Apply(node.Data, errors, warnings, infos);
        }

        /// <summary>
        /// Re-sums the node and each of its ancestors from their direct children's (already-correct) counts.
        /// Use for a localized change, e.g. after adding or removing a single issue leaf.
        /// </summary>
        public static void RecomputeUpward(TriValidationNode node)
        {
            for (var n = node; n != null; n = n.Parent)
            {
                var errors = 0;
                var warnings = 0;
                var infos = 0;

                foreach (var child in n.Children)
                {
                    errors += child.Data.errorCount;
                    warnings += child.Data.warningCount;
                    infos += child.Data.infoCount;
                }

                Apply(n.Data, errors, warnings, infos);
            }
        }

        /// <summary>
        /// Rebuild the subtree's totals with <see cref="Recompute" />,
        /// then propagate the new total to the ancestors with <see cref="RecomputeUpward" />.
        /// </summary>
        public static void RecomputeFrom(TriValidationNode node)
        {
            Recompute(node);
            RecomputeUpward(node.Parent);
        }

        private static void Apply(TriValidationNodeData data, int errors, int warnings, int infos)
        {
            data.errorCount = errors;
            data.warningCount = warnings;
            data.infoCount = infos;
            data.severity = errors > 0 ? TriMessageType.Error
                : warnings > 0 ? TriMessageType.Warning
                : infos > 0 ? TriMessageType.Info
                : TriMessageType.None;
        }
    }
}

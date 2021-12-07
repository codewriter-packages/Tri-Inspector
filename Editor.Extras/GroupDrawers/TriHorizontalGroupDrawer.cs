using TriInspector;
using TriInspector.Elements;
using TriInspector.GroupDrawers;
using UnityEngine;

[assembly: RegisterTriGroupDrawer(typeof(TriHorizontalGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriHorizontalGroupDrawer : TriGroupDrawer<DeclareHorizontalGroupAttribute>
    {
        public override TriPropertyCollectionBaseElement CreateElement(DeclareHorizontalGroupAttribute attribute)
        {
            return new TriBoxGroupElement();
        }

        private class TriBoxGroupElement : TriPropertyCollectionBaseElement
        {
            public override float GetHeight(float width)
            {
                if (ChildrenCount == 0)
                {
                    return 0f;
                }

                var height = 0f;

                for (var i = 0; i < ChildrenCount; i++)
                {
                    var child = GetChild(i);
                    var childWidth = width / ChildrenCount;
                    var childHeight = child.GetHeight(childWidth);

                    height = Mathf.Max(height, childHeight);
                }

                return height;
            }

            public override void OnGUI(Rect position)
            {
                if (ChildrenCount == 0)
                {
                    return;
                }

                var childWidth = position.width / ChildrenCount;

                for (var i = 0; i < ChildrenCount; i++)
                {
                    var child = GetChild(i);
                    var childRect = new Rect(position)
                    {
                        width = childWidth,
                        x = position.x + i * childWidth,
                    };

                    child.OnGUI(childRect);
                }
            }
        }
    }
}
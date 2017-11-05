using System.Collections.Generic;

namespace BraneCloud.Evolution.EC.GP.GE
{
    public static class GrammarNodeExtensions
    {
        /// <summary>
        /// Returns the nodes in the list and all of their descendants.
        /// </summary>
        public static IEnumerable<GrammarNode> Descendants(this IList<GrammarNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node != null)
                {
                    yield return node;
                    if (node.Children != null)
                    {
                        foreach (var child in node.Children.Descendants())
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return this node and its descendants.
        /// </summary>
        public static IEnumerable<GrammarNode> Descendants(this GrammarNode node)
        {
            if (node != null)
            {
                yield return node;
                if (node.Children != null)
                {
                    foreach (GrammarNode child in node.Children.Descendants())
                    {
                        yield return child;
                    }
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary>
    /// These are handy extension methods for GPNode types.
    /// </summary>
    public static class GPNodeExtensions
    {
        /// <summary>
        /// Enumerates all of the GPNodes in the tree starting with the root (tree.Child).
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this GPTree tree)
        {
            return Descendants(tree.Child);
        }

        /// <summary>
        /// Enumerates filtered GPNodes in the tree starting with the root (tree.Child).
        /// Results are filtered by the nodesearch argument (0=All, 1=Terminals, 2=Nonterminals).
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this GPTree tree, int nodesearch)
        {
            return Descendants(tree.Child, nodesearch);
        }

        /// <summary>
        /// Enumerates filtered GPNodes in the tree starting with the root (tree.Child).
        /// Results are filtered by the GPNodeGatherer which tests each node to decide
        /// if it qualifies.
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this GPTree tree, GPNodeGatherer gatherer)
        {
            return Descendants(tree.Child, gatherer);
        }

        /// <summary>
        /// Returns the nodes in the list and all of their descendants.
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this IList<GPNode> nodes)
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
        /// Returns the nodes in the list and all of their descendants filtered by nodesearch.
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this IList<GPNode> nodes, int nodesearch)
        {
            foreach (var node in nodes)
            {
                if (node != null)
                {
                    if (Test(node, nodesearch))
                    {
                        yield return node;
                    }
                    if (node.Children != null)
                    {
                        foreach (var child in node.Children.Descendants())
                        {
                            if (Test(child, nodesearch))
                                yield return child;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the nodes in the list and all of their descendants filtered by the GPNodeGatherer.
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this IList<GPNode> nodes, GPNodeGatherer gatherer)
        {
            foreach (var node in nodes)
            {
                if (node != null)
                {
                    if (Test(node, gatherer))
                    {
                        yield return node;
                    }
                    if (node.Children != null)
                    {
                        foreach (var child in node.Children.Descendants())
                        {
                            if (Test(child, gatherer))
                                yield return child;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return this node and its descendants.
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this GPNode node)
        {
            if (node != null)
            {
                yield return node;
                if (node.Children != null)
                {
                    foreach (GPNode child in node.Children.Descendants())
                    {
                        yield return child;
                    }
                }
            }
        }

        /// <summary>
        /// Return this node and its descendants filtered by nodesearch.
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this GPNode node, int nodesearch)
        {
            if (node != null)
            {
                if (Test(node, nodesearch))
                {
                    yield return node;
                }
                if (node.Children != null)
                    {
                        foreach (GPNode child in node.Children.Descendants())
                        {
                            if (Test(child, nodesearch))                        
                                yield return child;
                        }
                    }
            }
        }

        /// <summary>
        /// Return this node and its descendants filtered by the GPNodeGatherer.
        /// </summary>
        public static IEnumerable<GPNode> Descendants(this GPNode node, GPNodeGatherer gatherer)
        {
            if (node != null)
            {
                if (Test(node, gatherer))
                {
                    yield return node;
                }
                if (node.Children != null)
                {
                    foreach (GPNode child in node.Children.Descendants())
                    {
                        if (Test(child, gatherer))
                            yield return child;
                    }
                }
            }
        }

        private static bool Test(GPNode node, int nodesearch)
        {
            if (node == null)
                return false;

            // use the specified nodesearch
                return nodesearch == GPNode.NODESEARCH_ALL ||
                       nodesearch == GPNode.NODESEARCH_TERMINALS && (node.Children == null || node.Children.Length == 0) ||
                       nodesearch == GPNode.NODESEARCH_NONTERMINALS && (node.Children != null && node.Children.Length > 0);
        }
        private static bool Test(GPNode node, GPNodeGatherer filter)
        {
            if (node == null || filter == null)
                return false;

            // use the GPNodeGatherer if available
            return filter.Test(node);
        }
    }
}

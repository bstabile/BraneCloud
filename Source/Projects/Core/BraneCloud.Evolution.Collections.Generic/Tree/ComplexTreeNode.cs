/*
 * BraneCloud
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */
/*
 * This collection of non-binary tree data structures created by Dan Vanderboom.
 * Critical Development blog: http://dvanderboom.wordpress.com
 * Original Tree<T> blog article: http://dvanderboom.wordpress.com/2008/03/15/treet-implementing-a-non-binary-tree-in-c/
 */

using System;
using System.Text;

namespace BraneCloud.Evolution.Collections.Generic.Tree
{
    /// <summary>
    /// Represents a node in a Tree structure, with a parent node and zero or more child nodes.
    /// </summary>
    public class ComplexTreeNode<T> : IDisposable where T : ComplexTreeNode<T>
    {
        private T _parent;
        public T Parent
        {
            get { return _parent; }
            set
            {
                if (value == _parent)
                {
                    return;
                }

                if (_parent != null)
                {
                    _parent.Children.Remove(this);
                }

                if (value != null && !value.Children.Contains(this))
                {
                    value.Children.Add(this);
                }

                _parent = value;
            }
        }

        public T Root
        {
            get
            {
                //return (Parent == null) ? this : Parent.Root;

                ComplexTreeNode<T> node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                }
                return (T)node;
            }
        }

        private ComplexTreeNodeList<T> _children;
        public virtual ComplexTreeNodeList<T> Children
        {
            get { return _children; }
            private set { _children = value; }
        }

        private TreeTraversalDirection _DisposeTraversal = TreeTraversalDirection.BottomUp;
        /// <summary>
        /// Specifies the pattern for traversing the Tree for disposing of resources. Default is BottomUp.
        /// </summary>
        public TreeTraversalDirection DisposeTraversal
        {
            get { return _DisposeTraversal; }
            set { _DisposeTraversal = value; }
        }

        public ComplexTreeNode()
        {
            Parent = null;
            Children = new ComplexTreeNodeList<T>(this);
        }

        public ComplexTreeNode(T parent)
        {
            Parent = parent;
            Children = new ComplexTreeNodeList<T>(this);
        }

        public ComplexTreeNode(ComplexTreeNodeList<T> children)
        {
            Parent = null;
            Children = children;
            Children.Parent = (T)this;
        }

        public ComplexTreeNode(T parent, ComplexTreeNodeList<T> children)
        {
            Parent = parent;
            this.Children = children;
            Children.Parent = (T)this;
        }

        /// <summary>
        /// Reports a depth of nesting in the tree, starting at 0 for the root.
        /// </summary>
        public int Depth
        {
            get
            {
                var depth = 0;
                var node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                    depth++;
                }
                return depth;
            }
        }

        public override string ToString()
        {
            string Description = "Depth=" + Depth + ", Children=" + Children.Count;
            if (this == Root)
            {
                Description += " (Root)";
            }
            return Description;
        }

        #region IDisposable

        private bool _IsDisposed;
        public bool IsDisposed
        {
            get { return _IsDisposed; }
        }

        public virtual void Dispose()
        {
            CheckDisposed();

            // clean up contained objects (in Value property)
            if (DisposeTraversal == TreeTraversalDirection.BottomUp)
            {
                foreach (ComplexTreeNode<T> node in Children)
                {
                    node.Dispose();
                }
            }

            OnDisposing();

            if (DisposeTraversal == TreeTraversalDirection.TopDown)
            {
                foreach (ComplexTreeNode<T> node in Children)
                {
                    node.Dispose();
                }
            }

            // TODO: clean up the tree itself

            _IsDisposed = true;
        }

        public event EventHandler Disposing;

        protected void OnDisposing()
        {
            if (Disposing != null)
            {
                Disposing(this, EventArgs.Empty);
            }
        }

        protected void CheckDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #endregion
    }
}
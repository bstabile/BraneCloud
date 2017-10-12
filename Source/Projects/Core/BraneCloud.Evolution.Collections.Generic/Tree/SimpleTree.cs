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

namespace BraneCloud.Evolution.Collections.Generic.Tree
{
    /// <summary>
    /// Represents a hierarchy of objects or data.  SimpleTree is a root-level alias for SimpleTree and SimpleSubtreeNode.
    /// </summary>
    public class SimpleTree<T> : SimpleTreeNode<T>
    {
        public SimpleTree() { }
    }
}
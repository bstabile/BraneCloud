/*
 * BraneCloud.Evolution.EC (Evolutionary Computation)
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 *
 * This is an independent conversion from Java to .NET of ...
 *
 * Sean Luke's ECJ project at GMU 
 * (Academic Free License v3.0): 
 * http://www.cs.gmu.edu/~eclab/projects/ecj
 *
 * Radical alteration was required throughout (including structural).
 * The author of ECJ cannot and MUST not be expected to support this fork.
 *
 * If you wish to create yet another fork, please use a different root namespace.
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using System;
using System.Collections;
using System.Windows.Forms;

namespace BraneCloud.Evolution.EC.Configuration
{
	
	/// <author>  spaus
	/// </author>
	[Serializable]
	class ParameterDatabaseTreeNode : TreeNode, IComparable
	{
		
		/// <summary> </summary>
		public ParameterDatabaseTreeNode()
		{
		}
		
		/// <param name="userObject">
		/// </param>
		public ParameterDatabaseTreeNode(object userObject) : base(userObject.ToString())
		{
		    Tag = userObject;
		}
		
		/// <param name="userObject">
		/// </param>
		/// <param name="allowsChildren">
		/// </param>
		public ParameterDatabaseTreeNode(object userObject, bool allowsChildren) : base(userObject.ToString())
		{
            Tag = userObject;
            // this allowsChildren argument as it is directly linked to the Java Treeview
            // i think we should avoid using it ( to stick to .net ) .
            // but to decide I have to check if this is useful in the code
		}

        public TreeNodeCollection Children => Nodes;

		/// <param name="index">
		/// </param>
		/// <param name="visibleLeaves">
		/// </param>
		/// <returns>
		/// </returns>
		public virtual object GetChildAt(int index, bool visibleLeaves)
		{
			if (Nodes == null)
			{
				throw new IndexOutOfRangeException("node has no Children");
			}
			
			if (!visibleLeaves)
			{
				int nonLeafIndex = - 1;
				IEnumerator e = Children.GetEnumerator();
				while (e.MoveNext())
				{
                    TreeNode n = (TreeNode)e.Current;
					if (!(n.GetNodeCount(false) == 0))
					{
						if (++nonLeafIndex == index)
							return n;
					}
				}
				throw new IndexOutOfRangeException("index = " + index + ", Children = " + GetChildCount(visibleLeaves));
			}
			
			return Nodes[index];
		}
		
		/// <param name="visibleLeaves">
		/// </param>
		/// <returns>
		/// </returns>
		public virtual int GetChildCount(bool visibleLeaves)
		{
			if (!visibleLeaves)
			{
				int nonLeafCount = 0;
				IEnumerator e = Children.GetEnumerator();
				while (e.MoveNext())
				{
					TreeNode n = (TreeNode) e.Current;
					if (!(n.GetNodeCount(false) == 0))
						++nonLeafCount;
				}
				
				return nonLeafCount;
			}
			
			return base.Nodes.Count;
		}
		
		/* (non-Javadoc)
		* @see java.lang.Comparable#compareTo(java.lang.Object)
		*/
		public virtual int CompareTo(object o)
		{
            // CompareTo throws an exception in case the original Tag is null
            return ((IComparable<object>)Tag).CompareTo(((ParameterDatabaseTreeNode)o).Tag);
		}

	    /// <param name="comp">
		/// </param>
		public virtual void  Sort(IComparer comp)
		{
            if (this.Children == null)
                return ;
            IEnumerator e = Children.GetEnumerator();
            while (e.MoveNext())
            {
                ParameterDatabaseTreeNode n = (ParameterDatabaseTreeNode) e.Current;
                n.Sort(comp);
            }
		}
	}
}
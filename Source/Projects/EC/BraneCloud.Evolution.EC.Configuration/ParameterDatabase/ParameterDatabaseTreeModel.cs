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
	[Serializable]
	public class ParameterDatabaseTreeModel : TreeNode
	{
		virtual public bool VisibleLeaves
		{
			get
			{
				return visibleLeaves;
			}
			
			set
			{
				this.visibleLeaves = value;
			}
			
		}
		
		private bool visibleLeaves;
		
		public ParameterDatabaseTreeModel(TreeNode root) // BRS : TODO : base(root)
		{
			visibleLeaves = true;
		}
		
		public ParameterDatabaseTreeModel(TreeNode root, bool asksAllowsChildren) // BRS : TODO : base(root, asksAllowsChildren)
		{
			visibleLeaves = true;
		}
		
		public object GetChild(object parent, int index)
		{
			if (!visibleLeaves)
			{
				if (parent is ParameterDatabaseTreeNode)
				{
					return ((ParameterDatabaseTreeNode) parent).GetChildAt(index, visibleLeaves);
				}
			}
			
			return ((TreeNode) parent).Nodes[index];
		}
		
		public int GetChildCount(object parent)
		{
			if (!visibleLeaves)
			{
				if (parent is ParameterDatabaseTreeNode)
				{
					return ((ParameterDatabaseTreeNode) parent).GetChildCount(visibleLeaves);
				}
			}
			
			return ((TreeNode) parent).Nodes.Count;
		}
		
		public virtual void  Sort(object parent, IComparer comp)
		{
			((ParameterDatabaseTreeNode) parent).Sort(comp);
		}
	}
}
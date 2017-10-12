
using System;
using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.App.Regression.Func;

namespace BraneCloud.Evolution.EC.App.Regression.Func
{
    /// <summary>
    /// Copyright 2012 by Sean Luke
    /// Licensed under the Academic Free License version 3.0
    /// See the file "LICENSE" for more information
    /// 
    /// KornsERC.java
    /// 
    /// Created: Wed Nov  3 18:26:37 1999
    /// By: Sean Luke
    ///
    /// <p/>This ERC appears in the Korns function set.  It is defined as a random finite 65-bit IEEE double.  We achieve this by drawing a random long, then converting it to a double, then rejecting results which are either NaN or infinite.
    /// 
    /// <p/>M. F. Korns. Accuracy in Symbolic Regression. In <i>Proc. GPTP.</i> 2011.
    /// </summary>
    public class KornsERC : RegERC
    {
        public override string Name { get { return "KornsERC"; } }

        public override void ResetNode(IEvolutionState state, int thread)
        {
            do
            {
                value = Convert.ToDouble(state.Random[thread].NextLong());
            }
            while (Double.IsNaN(value) || Double.IsInfinity(value));
        }
    }
}



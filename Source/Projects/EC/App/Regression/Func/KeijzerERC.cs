
namespace BraneCloud.Evolution.EC.App.Regression.Func
{
    /// <summary>
    ///  Copyright 2012 by Sean Luke
    ///  Licensed under the Academic Free License version 3.0
    ///  See the file "LICENSE" for more information
    /// 
    /// KeijzerERC.java
    /// 
    /// Created: Wed Nov  3 18:26:37 1999
    /// By: Sean Luke
    ///
    /// <p/>This ERC appears in the Keijzer function set.  It is defined as a random value drawn from a Gaussian distriution with a mean of 0.0 and a standard deviation of 5.0.
    ///
    /// <p/>M. Keijzer. Improving Symbolic Regression with Interval Arithmetic and Linear Scaling. In <i>Proc. EuroGP.</i> 2003.
    /// </summary>
    public class KeijzerERC : RegERC
    {
        public static double MEAN = 0.0;
        public static double STANDARD_DEVIATION = 5.0;

        public override string Name { get { return "KeijzerERC"; } }

        public override void ResetNode(IEvolutionState state, int thread)
        {
            value = MEAN + state.Random[thread].NextGaussian() * STANDARD_DEVIATION;
        }
    }
}

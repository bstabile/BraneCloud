using System;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.Regression.Func
{
    /// <summary>
    /// Copyright 2012 by Sean Luke
    /// Licensed under the Academic Free License version 3.0
    /// See the file "LICENSE" for more information
    /// 
    /// Sqrt.java
    /// 
    /// Created: Mon Apr 23 17:15:35 EDT 2012
    /// By: Sean Luke
    /// 
    /// <p/>This function appears in the Korns and Keijzer function sets, and is sqrt(x)
    /// <p/>M. F. Korns. Accuracy in Symbolic Regression. In <i>Proc. GPTP.</i> 2011.
    /// <p/>M. Keijzer. Improving Symbolic Regression with Interval Arithmetic and Linear Scaling. In <i>Proc. EuroGP.</i> 2003.
    /// </summary>
    public class Sqrt : GPNode
    {
        public override string ToString() { return "sqrt"; }

        public override int ExpectedChildren => 1;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var rd = ((RegressionData)(input));

            Children[0].Eval(state, thread, input, stack, individual, problem);
            rd.x = Math.Sqrt(rd.x);
        }
    }
}



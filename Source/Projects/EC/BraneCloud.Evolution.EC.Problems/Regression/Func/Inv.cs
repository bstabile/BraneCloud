using System;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.Regression.Func
{
    /// <summary>
    /// Copyright 2012 by Sean Luke
    /// Licensed under the Academic Free License version 3.0
    /// See the file "LICENSE" for more information
    /// 
    /// Inv.java
    /// 
    /// Created: Mon Apr 23 17:15:35 EDT 2012
    /// By: Sean Luke
    /// 
    /// 
    /// <p/>This function appears in the Keijzer function set, and is 1/x.  Note that
    /// the division is <b>not protected</b>, so 1/0.0 is infinity.
    /// <p/>M. Keijzer. Improving Symbolic Regression with Interval Arithmetic and Linear Scaling. In <i>Proc. EuroGP.</i> 2003.
    /// </summary>
    public class Inv : GPNode
    {
        public override String ToString() { return "1/"; }

        public override int ExpectedChildren { get { return 1; } }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var rd = ((RegressionData)(input));

            Children[0].Eval(state, thread, input, stack, individual, problem);
            rd.x = 1.0 / rd.x;
        }
    }
}


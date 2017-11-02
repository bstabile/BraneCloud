using System;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.Regression.Func
{
    /// <summary>
    /// Copyright 2012 by Sean Luke
    /// Licensed under the Academic Free License version 3.0
    /// See the file "LICENSE" for more information
    /// 
    /// Tan.java
    ///  
    /// Created: Mon Apr 23 17:15:35 EDT 2012
    /// By: Sean Luke
    ///  
    /// <p/>This function appears in the Korns function set, and is just tanh(x)
    /// <p/>M. F. Korns. Accuracy in Symbolic Regression. In <i>Proc. GPTP.</i> 2011.
    /// </summary>
    public class Tan : GPNode
    {
        public override string ToString() { return "tan"; }

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
            rd.x = Math.Tan(rd.x);
        }
    }
}



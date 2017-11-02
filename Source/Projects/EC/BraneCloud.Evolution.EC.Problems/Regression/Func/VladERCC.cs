using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.Regression.Func
{
    /// <summary>
    /// Copyright 2012 by Sean Luke
    /// Licensed under the Academic Free License version 3.0
    /// See the file "LICENSE" for more information
    /// 
    /// VladERCC.java
    ///   
    /// Created: Wed Nov  3 18:26:37 1999
    /// By: Sean Luke
    /// 
    /// <p/>This ERC appears all three the Vladislavleva function sets.  It is not a constant but rather a function of one parameter (n) with an internal constant (c) and returns n * c.  Note that the value of c is drawn from the fully-closed range [-5.0, 5.0]. 
    /// </summary>
    public class VladERCC : VladERCA
    {
        public override string Name => "VladERCC";

        public override string ToStringForHumans()
        { return "n*" + (float)value; }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var rd = (RegressionData)input;

            Children[0].Eval(state, thread, input, stack, individual, problem);
            rd.x = rd.x * value;
        }
    }
}



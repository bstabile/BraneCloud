using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.Regression.Func
{
    /// <summary>
    /// Copyright 2006 by Sean Luke
    /// Licensed under the Academic Free License version 3.0
    /// See the file "LICENSE" for more information
    /// 
    /// X.java
    ///       
    /// Created: Wed Nov  3 18:26:37 1999
    /// By: Sean Luke
    /// </summary>
    public class X5 : GPNode
    {
        public override string ToString() { return "x5"; }

        /*
          public void checkConstraints(final EvolutionState state,
          final int tree,
          final GPIndividual typicalIndividual,
          final Parameter individualBase)
          {
          super.checkConstraints(state,tree,typicalIndividual,individualBase);
          if (children.length!=0)
          state.output.error("Incorrect number of children for node " + 
          toStringForError() + " at " +
          individualBase);
          }
        */
        public override int ExpectedChildren => 0;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            var rd = ((RegressionData)(input));
            double[] c = ((Benchmarks)problem).currentValue;
            if (c.Length >= 5)
                rd.x = ((Benchmarks)problem).currentValue[4];
            else rd.x = 0;
        }
    }
}



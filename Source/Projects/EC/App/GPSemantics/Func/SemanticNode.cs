
using System;
using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{
    public abstract class SemanticNode : GPNode
    {
        public override string ToString()
        {
            return "" + Value + Index;
        }

        public virtual char Value { get; set; }
        public virtual int Index { get; set; }

        public override int ExpectedChildren => 0;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            // No need to evaluate or look at children.
        }
    }
}
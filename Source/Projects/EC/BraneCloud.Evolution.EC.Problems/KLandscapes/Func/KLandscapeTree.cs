using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.KLandscapes.Func
{
    public abstract class KLandscapeTree : GPNode
    {
        public virtual char Value { get; set; }

        public override string ToString()
        {
            return "" + Value;
        }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
        }
    }
}
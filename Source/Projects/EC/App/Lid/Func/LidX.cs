
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.Lid.Func
{
    [ECConfiguration("ec.app.lid.func.LidX")]
    public class LidX : GPNode
    {
        public override string ToString()
        {
            return "X";
        }

        public override int ExpectedChildren => 0;

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            // No need to evaluate or look at children. Lid is only
            // about tree shape/size
        }
    }
}
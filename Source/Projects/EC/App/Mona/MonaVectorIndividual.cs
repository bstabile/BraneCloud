using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Vector;

namespace BraneCloud.Evolution.EC.App.Mona
{
    [ECConfiguration("ec.app.mona.MonaVectorIndividual")]
    public class MonaVectorIndividual : DoubleVectorIndividual
    {
        public override void Reset(IEvolutionState state, int thread)
        {
            base.Reset(state, thread);

            int numVertices = ((Mona) state.Evaluator.p_problem).NumVertices;
            int vertexSkip = numVertices * 2 + 4; // for four colors

            for (int x = 3; x < genome.Length; x += vertexSkip)
                // Alsing originally just set all his colors to 0 alpha.
                // Here I divide the alpha by 10 so they're initially very
                // transparent
                genome[x] /= 10;
        }
    }
}
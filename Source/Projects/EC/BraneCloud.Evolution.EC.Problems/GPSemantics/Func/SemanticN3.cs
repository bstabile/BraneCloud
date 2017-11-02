using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticN3")]
    public class SemanticN3 : SemanticNode
    {
        public override char Value => 'N';

        public override int Index => 3;

    }
}
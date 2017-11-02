using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX1")]
    public class SemanticX1 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 1;
    }
}
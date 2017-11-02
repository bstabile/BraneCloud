using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX5")]
    public class SemanticX5 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 5;
    }
}
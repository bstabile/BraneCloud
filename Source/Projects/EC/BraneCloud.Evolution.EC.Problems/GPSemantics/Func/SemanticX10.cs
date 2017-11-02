using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX10")]
    public class SemanticX10 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 10;
    }
}
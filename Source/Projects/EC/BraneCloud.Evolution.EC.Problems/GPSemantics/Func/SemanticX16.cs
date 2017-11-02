using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX16")]
    public class SemanticX16 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 16;
    }
}
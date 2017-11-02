using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX9")]
    public class SemanticX9 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 9;
    }
}
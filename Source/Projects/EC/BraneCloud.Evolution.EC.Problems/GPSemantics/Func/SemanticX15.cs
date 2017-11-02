using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX15")]
    public class SemanticX15 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 15;
    }
}
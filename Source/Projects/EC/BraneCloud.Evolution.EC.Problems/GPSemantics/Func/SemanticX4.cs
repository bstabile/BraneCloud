using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX4")]
    public class SemanticX4 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 4;
    }
}
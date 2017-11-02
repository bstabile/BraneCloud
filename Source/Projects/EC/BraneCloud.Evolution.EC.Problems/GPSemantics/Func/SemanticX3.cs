using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX3")]
    public class SemanticX3 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 3;
    }
}
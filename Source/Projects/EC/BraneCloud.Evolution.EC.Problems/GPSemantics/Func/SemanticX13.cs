using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX13")]
    public class SemanticX13 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 13;
    }
}
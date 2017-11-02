using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX2")]
    public class SemanticX2 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 2;
    }
}
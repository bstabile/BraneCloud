using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{


    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX8")]
    public class SemanticX8 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 8;
    }
}
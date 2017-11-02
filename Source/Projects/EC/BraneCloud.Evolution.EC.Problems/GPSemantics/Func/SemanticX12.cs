using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticX12")]
    public class SemanticX12 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 12;
    }
}
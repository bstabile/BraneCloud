using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX12")]
    public class SemanticX12 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 12;
    }
}
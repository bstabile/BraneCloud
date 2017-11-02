using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX9")]
    public class SemanticX9 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 9;
    }
}
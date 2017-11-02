using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX16")]
    public class SemanticX16 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 16;
    }
}
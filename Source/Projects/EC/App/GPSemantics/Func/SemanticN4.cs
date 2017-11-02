using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticN4")]
    public class SemanticN4 : SemanticNode
    {
        public override char Value => 'N';

        public override int Index => 4;

    }
}
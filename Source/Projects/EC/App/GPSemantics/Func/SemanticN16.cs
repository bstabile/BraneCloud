using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticN16")]
    public class SemanticN16 : SemanticNode
    {
        public override char Value => 'N';

        public override int Index => 16;

    }
}
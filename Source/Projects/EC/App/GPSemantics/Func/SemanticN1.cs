using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticN1")]
    public class SemanticN1 : SemanticNode
    {
        public override char Value => 'N';

        public override int Index => 1;
    }
}

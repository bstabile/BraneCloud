using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticN5")]
    public class SemanticN5 : SemanticNode
    {
        public override char Value => 'N';

        public override int Index => 5;

    }
}
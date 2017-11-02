using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX7")]
    public class SemanticX7 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 7;
    }
}
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX13")]
    public class SemanticX13 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 13;
    }
}
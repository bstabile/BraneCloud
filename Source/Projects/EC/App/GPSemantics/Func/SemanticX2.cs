using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX2")]
    public class SemanticX2 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 2;
    }
}
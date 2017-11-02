using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX6")]
    public class SemanticX6 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 6;
    }
}
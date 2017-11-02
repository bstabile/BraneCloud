using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX10")]
    public class SemanticX10 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 10;
    }
}
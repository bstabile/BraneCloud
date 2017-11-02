using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.GPSemantics.Func
{

    [ECConfiguration("ec.problems.gpsemantics.func.SemanticJ")]
    public class SemanticJ : SemanticNode
    {
        public override string ToString()
        {
            return "J";
        }

        public override int ExpectedChildren => 2;

        public override int Index => -1;

        public override char Value => 'J';
    }
}

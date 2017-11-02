using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.KLandscapes.Func
{
    [ECConfiguration("ec.problems.klandscapes.func.KLandscapeTreeW")]
    public class KLandscapeTreeW : KLandscapeTree
    {
        public override int ExpectedChildren => 0;

        public override char Value => 'W';
    }
}

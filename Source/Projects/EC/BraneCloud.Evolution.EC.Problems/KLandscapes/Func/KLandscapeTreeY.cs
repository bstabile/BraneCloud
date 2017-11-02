using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.KLandscapes.Func
{
    [ECConfiguration("ec.problems.klandscapes.func.KLandscapeTreeY")]
    public class KLandscapeTreeY : KLandscapeTree
    {
        public override int ExpectedChildren => 0;

        public override char Value => 'Y';
    }
}


using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.KLandscapes.Func
{
    [ECConfiguration("ec.problems.klandscapes.func.KLandscapeTreeB")]
    public class KLandscapeTreeB : KLandscapeTree
    {
        public override int ExpectedChildren => 2;

        public override char Value => 'B';
    }
}

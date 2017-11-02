using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Problems.KLandscapes.Func
{
    [ECConfiguration("ec.problems.klandscapes.func.KLandscapeTreeA")]
    public class KLandscapeTreeA : KLandscapeTree
    {
        public override int ExpectedChildren => 2;

        public override char Value => 'A';
    }
}
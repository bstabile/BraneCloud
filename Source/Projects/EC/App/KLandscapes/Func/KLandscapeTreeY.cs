using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.KLandscapes.Func
{
    [ECConfiguration("ec.app.klandscapes.func.KLandscapeTreeY")]
    public class KLandscapeTreeY : KLandscapeTree
    {
        public override int ExpectedChildren => 0;

        public override char Value => 'Y';
    }
}

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.KLandscapes.Func
{
    [ECConfiguration("ec.app.klandscapes.func.KLandscapeTreeA")]
    public class KLandscapeTreeA : KLandscapeTree
    {
        public override int ExpectedChildren => 2;

        public override char Value => 'A';
    }
}
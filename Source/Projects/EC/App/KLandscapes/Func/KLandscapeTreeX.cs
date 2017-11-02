using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.KLandscapes.Func
{
    [ECConfiguration("ec.app.klandscapes.func.KLandscapeTreeX")]
    public class KLandscapeTreeX : KLandscapeTree
    {
        public override int ExpectedChildren => 0;

        public override char Value => 'X';
    }
}

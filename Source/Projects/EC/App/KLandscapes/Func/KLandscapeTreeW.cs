using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.App.KLandscapes.Func
{
    [ECConfiguration("ec.app.klandscapes.func.KLandscapeTreeW")]
    public class KLandscapeTreeW : KLandscapeTree
    {
        public override int ExpectedChildren => 0;

        public override char Value => 'W';
    }
}

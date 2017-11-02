
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.Majority
{
    [ECConfiguration("ec.app.majority.MajorityData")]
    public class MajorityData : GPData
    {
        public long Data0;
        public long Data1;

        public override void CopyTo(GPData gpd)
        {
            var md = (MajorityData) gpd;
            md.Data0 = Data0;
            md.Data1 = Data1;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.GP.GE
{
    public interface IGPProblemParent
    {
        IGPProblem Problem { get; set; }
    }
}

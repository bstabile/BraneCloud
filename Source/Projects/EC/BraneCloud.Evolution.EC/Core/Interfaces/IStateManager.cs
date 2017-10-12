using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Core.Interfaces
{
    public interface IStateManager
    {
        void SetCheckpoint(IEvolutionState state);
        IEvolutionState GetCheckpoint(string fileName);
    }
}

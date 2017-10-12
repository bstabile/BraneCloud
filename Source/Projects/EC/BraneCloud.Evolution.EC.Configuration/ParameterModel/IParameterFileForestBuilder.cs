using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration.ForestModel;

namespace BraneCloud.Evolution.EC.Configuration
{
    public interface IParameterFileForestBuilder
    {
        IParameterFileForest Build(IEnumerable<string> targets);

        List<string> Targets { get; }
        List<BuilderError> Errors { get; }
        IParameterFileForest Result { get; }
    }
}

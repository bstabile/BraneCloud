using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BraneCloud.Evolution.EC.Configuration.ForestModel;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class ParameterFileForestBuilder : IParameterFileForestBuilder
    {
        public IParameterFileForest Build(IEnumerable<string> targets)
        {
            Targets = targets.ToList();
            Errors = new List<BuilderError>(Targets.Count);
            var forest = new ParameterFileForest();
            foreach (var t in targets)
            {
                if (string.IsNullOrEmpty(t))
                {
                    Errors.Add(new BuilderError(t, "Target specification was null or empty."));
                    continue;
                }
                if (!Directory.Exists(t))
                {
                    Errors.Add(new BuilderError(t, "The specified target directory does not exist."));
                }
                var tree = new ParameterFileTree(t);
                forest.Trees.Add(t, tree);
            }
            return Result = forest; // cache it in case the builder lifetime will be extended for some reason
        }
        public List<string> Targets { get; protected set; }
        public List<BuilderError> Errors { get; protected set; }
        public IParameterFileForest Result { get; protected set; }
    }
}

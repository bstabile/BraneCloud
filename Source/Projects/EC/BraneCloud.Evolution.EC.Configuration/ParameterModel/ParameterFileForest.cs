using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class ParameterFileForest : IParameterFileForest
    {
        public ParameterFileForest()
        {
            Errors = new List<BuilderError>();
            Trees = new Dictionary<string, ParameterFileTree>();
        }

        public ParameterFileForest(IEnumerable<string> roots) : this()
        {
            Build(roots);
        }

        public void Build(IEnumerable<string> roots)
        {
            foreach (var t in roots)
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
                Trees.Add(t, tree);
            }
            
        }

        public IDictionary<string, ParameterFileTree> Trees { get; protected set; }
        public List<BuilderError> Errors { get; protected set; }
    }
}

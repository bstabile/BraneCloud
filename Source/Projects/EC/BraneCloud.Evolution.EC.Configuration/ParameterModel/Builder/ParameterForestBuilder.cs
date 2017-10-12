/*
 * BraneCloud.Evolution.EC (Evolutionary Computation)
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 *
 * This is an independent conversion from Java to .NET of ...
 *
 * Sean Luke's ECJ project at GMU 
 * (Academic Free License v3.0): 
 * http://www.cs.gmu.edu/~eclab/projects/ecj
 *
 * Radical alteration was required throughout (including structural).
 * The author of ECJ cannot and MUST not be expected to support this fork.
 *
 * If you wish to create yet another fork, please use a different root namespace.
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class ParameterForestBuilder : IParameterForestBuilder
    {
        public IParameterForest Build(ParameterSourceLocator sourceLocator)
        {
            return Build(new[] {sourceLocator});
        }

        public IParameterForest Build(IEnumerable<ParameterSourceLocator> sourceLocators)
        {
            SourceLocators = sourceLocators.ToList();
            Errors = new List<BuilderError>();
            Result = null;

            var forest = new ParameterForest();
            foreach (var loc in SourceLocators)
            {
                if (string.IsNullOrEmpty(loc.Path))
                {
                    Errors.Add(new BuilderError(this, loc, "The root specification was null or empty."));
                    continue;
                }
                if (!Directory.Exists(loc.Path))
                {
                    Errors.Add(new BuilderError(this, loc, "The root does not exist."));
                }
                try
                {
                    var tree = new FileTree(loc.Path);
                    forest.Sources.Add(loc.Path, new FileTree(loc.Path));
                    foreach (var node in tree.NodeRegistry)
                    {
                        forest.Nodes.Add(node.Key, node.Value);
                        forest.Trees.Add(node.Key, new FileDictionaryTree(node.Value.Info));
                    }
                }
                catch (Exception ex)
                {
                    Errors.Add(new BuilderError(forest, loc,
                                   String.Format("An exception of type {0} was thrown while attempting to build the source.", 
                                   ex.GetType().Name),
                                   ex));
                }
            }
            return Result = forest; // return the cached result
        }

        public List<ParameterSourceLocator> SourceLocators { get; protected set; }
        public List<BuilderError> Errors { get; protected set; }
        public IParameterForest Result { get; protected set; }
    }
}
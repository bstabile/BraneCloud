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
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class BuilderError
    {
        public BuilderError()
        {
            Messages = new List<string>();
            Exceptions = new List<Exception>();
        }

        public BuilderError(object target, ParameterSourceLocator sourceLocator, IEnumerable<string> messages, IEnumerable<Exception> exceptions)
            : this()
        {
            Target = target;
            SourceLocator = sourceLocator;
            if (messages != null)
            {
                foreach (var msg in messages.Where(msg => !String.IsNullOrEmpty(msg)))
                {
                    Messages.Add(msg);
                }
            }
            if (exceptions == null) return;
            foreach (var ex in exceptions.Where(ex => ex != null))
            {
                Exceptions.Add(ex);
            }
        }

        public BuilderError(ParameterSourceLocator sourceLocator, IEnumerable<string> messages)
            : this(null, sourceLocator, messages, null)
        {
        }

        public BuilderError(ParameterSourceLocator sourceLocator, string message)
            : this(null, sourceLocator, new[]{message}, null)
        {
        }

        public BuilderError(object target, ParameterSourceLocator sourceLocator, string message)
            : this(target, sourceLocator, new[]{message}, null)
        {
        }

        public BuilderError(object target, ParameterSourceLocator sourceLocator, string message, Exception exception)
            : this(target, sourceLocator, new[]{message}, new[]{exception})
        {
        }

        public BuilderError(object target, ParameterSourceLocator sourceLocator, string message, IEnumerable<Exception> exceptions)
            : this(target, sourceLocator, new[]{message}, exceptions)
        {
        }

        public object Target { get; protected set; }
        public ParameterSourceLocator SourceLocator { get; protected set; }
        public List<string> Messages { get; private set; }
        public List<Exception> Exceptions { get; private set; }
    }
}
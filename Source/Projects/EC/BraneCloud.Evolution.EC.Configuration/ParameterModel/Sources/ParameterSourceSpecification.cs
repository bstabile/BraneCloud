using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class ParameterSourceSpecification
    {
        public ParameterSourceSpecification(string root)
        {
            Root = root;
            Type = ParameterSourceType.Default;
            Description = "";
        }

        public ParameterSourceSpecification(string root, ParameterSourceType type)
        {
            Root = root;
            Type = type;
            Description = "";
        }

        public ParameterSourceSpecification(string root, ParameterSourceType type, string description)
        {
            Root = root;
            Type = type;
            Description = description;
        }

        public string Root { get; protected set; }
        public ParameterSourceType Type { get; protected set; }
        public string Description { get; protected set; }
    }
}

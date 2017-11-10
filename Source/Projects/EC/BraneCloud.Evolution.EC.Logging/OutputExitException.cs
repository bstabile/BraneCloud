using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.Logging
{
    public class OutputExitException : Exception
    {
        public OutputExitException(String message)
            : base(message)
        {
        }
    }
}

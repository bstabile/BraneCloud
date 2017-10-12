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

using System.Text.RegularExpressions;

namespace BraneCloud.Evolution.Util.RegularExpressions
{
    public class RegexUtilities
    {
        // BRS : NOTE : The following members are just placeholders. You can safely ignore this file for now.

        public static bool IsValidEmailAddress(string address)
        {
            return Regex.IsMatch(address, RegexPattern.ValidEmailAddress);
        }

        public static string ConvertDateFromMonthFirstToYearFirst(string input, char separatorBefore, char separatorAfter)
        {
            var patternFrom = "\\b(?<month>\\d{1,2})" + separatorBefore
                            + "(?<day>\\d{1,2})" + separatorBefore
                            + "(?<year>\\d{2,4})\\b";

            var patternTo = "${year}" + separatorAfter
                          + "${month}" + separatorAfter
                          + "${day}";

            return Regex.Replace(input, patternFrom, patternTo);
        }
    }
}
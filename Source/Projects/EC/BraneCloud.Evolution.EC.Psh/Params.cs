/*
* Copyright 2009-2010 Jon Klein
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.IO;
using SharpenMinimal;

namespace BraneCloud.Evolution.EC.Psh {
/// <summary>A utility class for reading PushGP params.</summary>
public class Params {
  /// <exception cref="System.Exception"/>
  public static Dictionary<string, string> ReadFromFile(string inFilepath) {
    Dictionary<string, string> map = new Dictionary<string, string>();
    return Read(ReadFileString(inFilepath), map, inFilepath);
  }

  /// <exception cref="System.Exception"/>
  public static Dictionary<string, string> Read(string inParams) {
    Dictionary<string, string> map = new Dictionary<string, string>();
    return Read(inParams, map, null);
  }

  /// <exception cref="System.Exception"/>
  public static Dictionary<string, string> Read(string inParams, Dictionary<string, string> inMap, string inFilepath) {
    int linenumber = 0;
    string filename = "<string>";
    try {
      var reader = new System.IO.StringReader(inParams);
      string line;
      string parent;
      if (inFilepath == null) {
        parent = null;
        filename = "<string>";
      } else {
        // parent = inFile.GetParent();
        parent = Path.GetDirectoryName(inFilepath);
        filename = Path.GetFileName(inFilepath);
      }
      while ((line = reader.ReadLine()) != null) {
        linenumber += 1;
        int comment = line.IndexOf('#', 0);
        if (comment != -1) {
          line = SharpenMinimal.Runtime.Substring(line, 0, comment);
        }
        if (line.StartsWith("include")) {
          int startIndex = "include".Length;
          string includefile = SharpenMinimal.Extensions.Trim(SharpenMinimal.Runtime.Substring(line, startIndex, line.Length));
          try {
            var f = Path.Combine(parent, includefile);
            Read(ReadFileString(f), inMap, f);
          } catch (IncludeException) {
            // A previous include exception should bubble up to the
            // top
            throw;
          } catch (Exception) {
            // Any other exception should generate an error message
            throw new IncludeException("Error including file \"" + includefile + "\" at line " + linenumber + " of file \"" + filename + "\"");
          }
        } else {
          int split = line.IndexOf('=', 0);
          if (split != -1) {
            string name = SharpenMinimal.Extensions.Trim(SharpenMinimal.Runtime.Substring(line, 0, split));
            string value = SharpenMinimal.Extensions.Trim(SharpenMinimal.Runtime.Substring(line, split + 1, line.Length));
            while (value.EndsWith("\\")) {
              value = SharpenMinimal.Runtime.Substring(value, 0, value.Length - 1);
              line = reader.ReadLine();
              if (line == null) {
                break;
              }
              linenumber++;
              value += SharpenMinimal.Extensions.Trim(line);
            }
            inMap[name] = value;
          }
        }
      }
    } catch (IncludeException) {
      // A previous include exception should bubble up to the top
      throw;
    } catch (Exception) {
      // Any other exception should generate an error message
      throw new IncludeException("Error at line " + linenumber + " of parameter file \"" + filename + "\"");
    }
    return inMap;
  }

  /// <summary>Utility function to read a file in its entirety to a string.</summary>
  /// <param name="inPath">The file path to be read.</param>
  /// <returns>The contents of a file represented as a string.</returns>
  /// <exception cref="System.Exception"/>
  internal static string ReadFileString(string inPath) {
    return File.ReadAllText(inPath);
  }

}
}

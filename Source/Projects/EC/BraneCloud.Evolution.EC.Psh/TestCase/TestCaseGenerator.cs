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
using BraneCloud.Evolution.EC.Psh;

namespace BraneCloud.Evolution.EC.Psh.TestCase {
/// <summary>A class allowing for the runtime creation of custom test cases.</summary>
/// <remarks>
/// A class allowing for the runtime creation of custom test cases.
/// Each test case is a dictionary of HashMap&lt; String, Object &gt;. Each entry in
/// the dictionary corresponds to a problem input, except for the special token
/// "output", which is reserved for the problem output.
/// </remarks>
public abstract class TestCaseGenerator {
  /// <returns>The number of cases the generator will create.</returns>
  public abstract int TestCaseCount();

  /// <returns>
  /// Test case at index n as an ObjectPair, where _first is the input
  /// and _second is the output. The types of the objects may depend
  /// on the problem type.
  /// </returns>
  public abstract ObjectPair TestCase(int inIndex);
}
}

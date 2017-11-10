using System;
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

namespace BraneCloud.Evolution.EC.Psh {
/// <summary>Abstract class for implementing stacks.</summary>
public interface Stack {

  void Dup();

  void Rot();

  void Shove(int inIndex);

  void Swap();

  void Yank(int inIndex);

  void YankDup(int inIndex);

  void Clear();

  int Size();

  void Popdiscard();
}

}

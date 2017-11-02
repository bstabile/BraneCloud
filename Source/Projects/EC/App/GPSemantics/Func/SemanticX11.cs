using System;
using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.GPSemantics.Func
{

    [ECConfiguration("ec.app.gpsemantics.func.SemanticX11")]
    public class SemanticX11 : SemanticNode
    {
        public override char Value => 'X';

        public override int Index => 11;
    }
}
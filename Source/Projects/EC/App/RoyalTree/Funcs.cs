using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.App.RoyalTree.Func
{

    public abstract class RoyalTreeNode : GPNode
    {
        public abstract char Value { get; set; }

        public override string ToString()
        {
            return "" + Value;
        }

        public override void Eval(IEvolutionState state,
            int thread,
            GPData input,
            ADFStack stack,
            GPIndividual individual,
            IProblem problem)
        {
            // no need to do anything here
        }
    }

    [ECConfiguration("ec.app.royaltree.func.RoyalTreeA")]
    public class RoyalTreeA : RoyalTreeNode
    {
        public override int ExpectedChildren => 1;

        public override char Value { get; set; } = 'A';
    }

    [ECConfiguration("ec.app.royaltree.func.RoyalTreeB")]
    public class RoyalTreeB : RoyalTreeNode
    {
        public override int ExpectedChildren => 2;

        public override char Value { get; set; } = 'B';
    }

    [ECConfiguration("ec.app.royaltree.func.RoyalTreeC")]
    public class RoyalTreeC : RoyalTreeNode
    {
        public override int ExpectedChildren => 3;

        public override char Value { get; set; } = 'C';
    }

    [ECConfiguration("ec.app.royaltree.func.RoyalTreeD")]
    public class RoyalTreeD : RoyalTreeNode
    {
        public override int ExpectedChildren => 4;

        public override char Value { get; set; } = 'D';
    }

    [ECConfiguration("ec.app.royaltree.func.RoyalTreeE")]
    public class RoyalTreeE : RoyalTreeNode
    {
        public override int ExpectedChildren => 5;
        public override char Value { get; set; } = 'E';
    }

    [ECConfiguration("ec.app.royaltree.func.RoyalTreeX")]
    public class RoyalTreeX : RoyalTreeNode
    {
        public override int ExpectedChildren => 0;
        public override char Value { get; set; } = 'X';
    }
}

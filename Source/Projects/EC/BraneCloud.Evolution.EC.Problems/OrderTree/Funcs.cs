using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;

namespace BraneCloud.Evolution.EC.Problems.OrderTree.Func
{
    public abstract class OrderTreeNode : GPNode
    {
        public abstract int Value { get; set; }

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

    /// <summary>
    /// OrderTreeFunctionNode has two children by default.
    /// </summary>
    public abstract class OrderTreeFunctionNode : OrderTreeNode
    {
        public override int ExpectedChildren => 2;
    }

    /// <summary>
    /// OrderTreeTerminalNode has zero children (sealed).
    /// </summary>
    public abstract class OrderTreeTerminalNode : OrderTreeNode
    {
        public sealed override int ExpectedChildren => 0;
    }


    #region Functions

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF0")]
    public class OrderTreeF0 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 0;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF1")]
    public class OrderTreeF1 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 1;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF2")]
    public class OrderTreeF2 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 2;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF3")]
    public class OrderTreeF3 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 3;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF4")]
    public class OrderTreeF4 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 4;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF5")]
    public class OrderTreeF5 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 5;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF6")]
    public class OrderTreeF6 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 6;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF7")]
    public class OrderTreeF7 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 7;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF8")]
    public class OrderTreeF8 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 8;
    }

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeF9")]
    public class OrderTreeF9 : OrderTreeFunctionNode
    {
        public override int Value { get; set; } = 9;
    }

    #endregion


    #region Terminals

    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT0")]
    public class OrderTreeT0 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 0;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT1")]
    public class OrderTreeT1 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 1;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT2")]
    public class OrderTreeT2 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 2;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT3")]
    public class OrderTreeT3 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 3;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT4")]
    public class OrderTreeT4 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 4;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT5")]
    public class OrderTreeT5 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 5;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT6")]
    public class OrderTreeT6 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 6;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT7")]
    public class OrderTreeT7 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 7;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT8")]
    public class OrderTreeT8 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 8;
    }
    [ECConfiguration("ec.problems.ordertree.func.OrderTreeT9")]
    public class OrderTreeT9 : OrderTreeTerminalNode
    {
        public override int Value { get; set; } = 9;
    }

    #endregion
}

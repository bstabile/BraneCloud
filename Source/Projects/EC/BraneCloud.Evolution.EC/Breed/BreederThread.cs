using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BraneCloud.Evolution.EC.Support;

namespace BraneCloud.Evolution.EC.Breed
{
    public class BreederThread : IThreadRunnable
    {
        internal Population NewPop;
        public int[] NumInds;
        public int[] From;
        public IBreeder Breeder;
        public IEvolutionState State;
        public int ThreadNum;
        public virtual void Run()
        {
            Breeder.BreedPopChunk(NewPop, State, NumInds, From, ThreadNum);
        }
    }
}

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BraneCloud.Evolution.EC.App.Hiff
{
/*
 * HIFF (Hierarchical IF-and-only-IF) is a structured problem of nested sections
 * developed by Richard Watson
 * 
 * Comes with shuffled option.
 * 
 * In it's simplest incarnation it requires a length 2^n.
 * 
 * Some scope for generalisation here...
 */

/*
 * 
 * We ought to GROW this structure!!! instead of moulded ready-made structures
 */

//public class HIFFProblem : Problem 
//{

//    static int d;

//    public HIFFProblem() {}

//    public void initialise() {
//        //super(c);
//    }

//    public double evaluate(Genome g) {
//        int fitness = evaluate((IntGenome) g, 0, g.length() - 1);
//        //log.debug("HIFF fitness = " + fitness);
//        return fitness;
//    }

//    private int evaluate(IntGenome g, int start, int end) {
//        if (start == end) {
//            //log.debug("terminates traversal @ " + start);
//            return 1;
//        }
//        //log.debug("start: " + start + " end:" + end);
//        int suit = g.get(start);
//        int credit = 1;
//        for (int i = start + 1; i <= end; i++) {
//            if (g.get(i) != suit) {
//                credit = 0;
//                break;
//            }
//        }
//        int halfLength = (end - start + 1) / 2; // inclusive
//        //log.debug("PRIOR : start=" + start + " end=" +end + " halflength=" +
//        // halfLength);
//        return credit + evaluate(g, start, start + halfLength - 1)
//                + evaluate(g, start + halfLength, end);
//    }

//    // number of modules in a single genome
//    public int numberOfModules(PartialIntGenome g, int length) {
//        int count = 0;
//        int suit = g.get(0);
//        boolean continuous = false;

//        for (int i = 0; i < g.length(); i++) {
//            if (i % length == 0) { // starting new section
//                if (continuous) {
//                    count++;
//                }
//                suit = g.get(i); // get the value needed to follow
//                continuous = true; // assume true until not followed
//            } else { // keep testing for continuity
//                continuous = (continuous && suit == g.get(i));
//            }
//            //System.out.println("i="+i+", count="+count+", suit="+suit+",
//            // continuous="+continuous);
			
//            // account for PartialIntGenomes
//            if(!g.specifies(i)) continuous = false;
			
//        }

//        // finally consider last section
//        if (continuous)
//            count++;

//        return count;
//    }

//    public static int numberOfModules(GeneSet gs, int length) {
//        /*
//         * works by building a set of all the bbs of specified length contained
//         * in geneset.
//         */

//        // initialise set of bbs
//        HashSet modules = new HashSet();
//        // for each individual in geneset
//        for (int indy = 0; indy < gs.size(); indy++) {
//            boolean continuous = false;
//            PartialIntGenome g = gs.get(indy).flatten();
//            int suit = g.get(0);
//            PartialIntGenome bb = new PartialIntGenome();
//            for (int i = 0; i < gs.get(indy).size(); i++) {
//                if (i % length == 0) { // starting new section
//                    if (continuous) {
//                        addBB(bb, modules);
//                    }
//                    suit = g.get(i); // get the value needed to follow
//                    continuous = true; // assume true until not followed
//                    bb = new PartialIntGenome();
//                } else { // keep testing for continuity
//                    continuous = (continuous && suit == g.get(i));
//                }
//                // account for PartialIntGenomes
//                if(!g.specifies(i)) continuous = false;
				
//                bb.set(i, g.get(i));
//                //System.out.println("i="+i+", count="+count+",
//                // suit="+suit+",
//                // continuous="+continuous);
//            }

//            // finally consider last section
//            if (continuous)
//                addBB(bb, modules);
//        }
//        // initialise cand bb
//        // construct condidate bb
//        // if bb not in set then add it (and output on display)
//        // display set of screen
//        // return size of set
//        return modules.size();
//    }

//    private static void addBB(PartialIntGenome bb, HashSet set) {
//        for (Iterator it = set.iterator(); it.hasNext();) {
//            if (bb.isPhenotypicClone((PartialIntGenome)it.next())) {
//                return;
//            }
//        }
//        //log.debug("adding " + bb + " to set " + set);
//        set.add(bb);
//    }

//    public String testModuleCount() {
//        StringBuffer sb = new StringBuffer();
//        GeneSetModel gsm = new GeneSetModel();
//        gsm.initialise();
//        sb.append("pop:\n"+gsm);
//        GeneSet evalSet = new GeneSet();
//        for(int i = 0; i < Config.getInt("PopulationSize"); i++){
//            evalSet.add(gsm.getIndividual());
//        }
//        sb.append("evalset:\n" + evalSet.popString());
		
//        // need to set size (i.e. specified) of new genomes to 0 for new genomes
//        // for numberOfModules to endure they are blank but cannot do earlier since
//        // pop initialisation sets size = 1
//        Config.set("GenesSpecified", 0);
		
//        sb.append("NOM(2): " + numberOfModules(evalSet, 2));
//        return sb.toString();
//    }
	
//    public static void main(String[] arg) {
//        IntGenome.MAX = 1;
//        Config.readFrom("seam.gac");
//        HIFFProblem p = new HIFFProblem();
//        IntGenome context = new IntGenome();
//        PartialIntGenome pig = new PartialIntGenome();
//        pig.setSize(8);
//        pig.initialise();
//        context.initialise(Config.getInt("GenomeLength"));
//        p.log.debug("Context: " + context);
//        p.log.debug("Genome : " + pig);
//        p.log.debug("Compose: " + PartialIntGenome.compose(pig, context));

//        p.log.debug("Evaluates: "
//                + p.evaluate(PartialIntGenome.compose(pig, context)));
//        p.log.debug("Building-blocks of length 4: "
//                + p.numberOfModules(PartialIntGenome.compose(pig, context), 4));
		
//        System.out.println(p.testModuleCount());
//    }
//}

}
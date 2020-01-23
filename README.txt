BrainCloud.Evolution.EC is a full port of ECJ to the .NET platform.
This is an INDEPENDENT FORK of ECJ v20, converted to C# .NET 4.0 
by Bennett R. Stabile 2011-05-14

************************************************************************************************

ECJ (Evolutionary Computation in Java) has been under development
by Sean Luke and colleagues at George Mason University and elsewhere
for the past thirteen years. Is is widely used in academic research circles
and other places due to its power, versatility, stability, and level of support.

The goal here is to achieve and maintain "functional equivalence", but
there can be no guarantees or support of any kind (although I'll do my best).

The licence for this derivative work is Apache 2.0. (read: Do what you want with no guarantees!)

Extensive documentation for the Java version can be found here:

http://cs.gmu.edu/~eclab/projects/ecj/

But please do NOT expect Sean or his associates to answer questions or provide
support of any kind for this conversion. They have plenty of ongoing work with ECJ!
If you need that kind of support, then you should use THE JAVA SOFTWARE THEY PROVIDE.

************************************************************************************************

NOTE: The Parameter System (covered extensively in the original ECJ docs)

The most difficult aspect of this software to learn is the "ParameterDatabase"
system that provides a remarkable degree of flexibility, at the expense of
simplicity. It is therefore suggested that you step through the samples to
see how parameters are consumed during an evolutionary run. Once you do
this a few times, it will all become much less intimidating. And you will
find it very easy to wander off to tackle your specific problem domain.

There is a significant difference in the way this fork handles parameters.
First, an attribute is used to mark classes with a "canonical name", i.e.
the name that is used in ECJ. Then, a class called ECActivator reflects on
whatever assemblies you feed it during setup, and it is able to match the
name to an actual .NET type. The attribute you use to tag types is called
ECConfigurationAttribute. Thus you can do this:

	[ECConfiguration("ec.MyUniqueType")]
	public class BrainCloud.Evolution.EC.MyUniqueType {}
 
This allows the .NET version to consume parameter files unchanged from the ECJ system. 
That was my solution to one aspect of the "functional equivalence" challenge (there
are a few others, but none quite as pervasive).

************************************************************************************************

NOTE: The source code directory structure

The structure of folders in the package is slightly different than what you might expect.
That is because EC exists as just one branch in a much larger tree that I maintain privately.
This structure will almost certainly change in the future. But for now, just be aware that...

	Projects
	Solutions

... are siblings for a good reason. That is a bit of a headache sometimes (for various reasons),
but it makes sense in a larger context that doesn't show up in the open source version.

************************************************************************************************

NOTE: Tests

There are over 500 unit and functional tests in the solution. But these were primarily to
designed in haste to validate the conversion. This software will almost certainly require 
several thousands of tests to achieve anything close to full coverage. 

Subsequent drops will feature many new tests, while some of the tests needed during the 
conversion may dissappear or be substantially reworked. This is still a work in progress.

************************************************************************************************

NOTE: Future Changes

There WILL be significant changes in subsequent drops of this software. So I suggest you take
note of the fact that this is still an ALPHA. That is not so much because things aren't working
correctly, because most of it is working correctly. However, adpating this to take FULL advantage
of .NET is going to involve sweeping changes. Java is simply a very different beast, and disconnects
are inevitable to achieve an elegant and *optimal* end result.

************************************************************************************************

Have Fun!!!

;-)

BRS 


Take a look at Sean Luke's comments in MersenneTwisterFast.
I haven't had the need or the guts to mess with it beyond
a straightforward conversion to C# from the original Java.

It is worth finding "usages" of RandomChoiceChooser to see
how that is employed in the GP subsystem. It was a little
confusing at first. But it DOES make sense when you walk
through it.

MASON, a related project at GMU, contains some useful statistical 
random distribution classes (with PDF and CDF RNG functionality)
that I will probably add in the near future.

BRS
2011-05-14
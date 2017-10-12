EC is the place where the important algorithms live.

Once I've had a chance to restructure this assembly
there will be little dependency on anything else.
For example, with some IoC we should be able to move
references to Configuration, Logging, etc. out and
simply inject what is needed at runtime.

BRS
2011-05-14
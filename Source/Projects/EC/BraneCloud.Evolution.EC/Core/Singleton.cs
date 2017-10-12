using System;

namespace BraneCloud.Evolution.EC.Model
{
	/// <summary> A Singleton is a class for which there will be only one instance
	/// in the entire course of a run, and which will exist for pretty
	/// much the entire run.  Singletons are set up using setup(...)
	/// when they are first created.
	/// 
	/// </summary>
	
	public interface Singleton : Setup
	{
	}
}
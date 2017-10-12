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
using System.IO;
using System.Collections;
using System.Linq;
using System.Reflection;

using BraneCloud.Evolution.EC.Support;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Logging
{
    /// <summary> 
    /// Outputs and logs system messages, errors, and other various
    /// items printed as a result of a run.
    /// 
    /// <p/> 
    /// Output maintains zero or more logs, which contain Writers which write out stuff.
    /// 
    /// <p/>
    /// When the system fails for some reason and must be started back
    /// up from a checkpoint, Output's log files may be overwritten.  Output
    /// offers three approaches here.  First, Output can clear the log file
    /// and overwrite it.  Second, Output can append to the existing log file;
    /// because checkpoints are only done occasionally, this may result in
    /// duplicate outputs to a file, so keep this in mind.  Third, Output can
    /// keep certain written text, typically <i>announcements</i>, in memory;
    /// this text gets written out into the checkpoint file, and so it is sound.
    /// 
    /// <p/>
    /// There are several kinds of announcements, in different levels
    /// of importance.
    /// 
    /// <ol>
    /// <li/> FATAL ERRORs.  These errors cause the system to exit(1) immediately.
    /// <li/> Simple ERRORs.  These errors set the "errors" flag to true; at 
    /// the end of a stream of simple errors, the system in general is expected
    /// to exit with a fatal error due to the flag being set.  That's the
    /// protocol anyway.  On restart from a checkpoint, if there were any
    /// simple errors, the system ends with a fatal error automatically.
    /// <li/> WARNINGs.  These errors do not cause the system to exit under any
    /// circumstances.
    /// <li/> MESSAGEs.  Useful facts printed out for the benefit of the user.
    /// <li/> SYSTEM MESSAGEs.  Useful system-level facts printed out for the 
    /// benefit of the user.
    /// </ol>
    /// 
    /// <p/>
    /// Output by default will automatically flush any log which prints an announcement
    /// (or anything printed with Output.PrintLn(...).  You can change this behavior with
    /// the setFlush() method.
    /// 
    /// <p/>
    /// Output will also store all announcements in memory by default so as to reproduce
    /// them if it's restarted from a checkpoint.  You can change this behavior also by
    /// 
    /// </summary>
    [Serializable]
    public class Output : IOutput
    {
        #region Constants

        public const int ALL_LOGS = -1;

        #endregion // Constants
        #region Private Fields

        private bool _errors;
        private readonly ArrayList _logs = ArrayList.Synchronized(new ArrayList(10));
        private ArrayList _announcements = ArrayList.Synchronized(new ArrayList(10));
        private readonly HashSetSupport _oneTimeWarnings = new HashSetSupport();

        #endregion // Private Fields
        #region Constructors and Restart

        /// <summary>
        /// Default constructor creates a new, verbose, empty Output object with StoreAnnouncements set to true. </summary>
        public Output()
        {
            _errors = false;
            _storeAnnouncements = true;
        }

        /// <summary>
        /// Creates a new, verbose, empty Output object.
        /// </summary>
        public Output(bool storeAnnouncementsInMemory)
        {
            _errors = false;
            _storeAnnouncements = storeAnnouncementsInMemory;
        }

        public virtual void Restart()
        {
            lock (this)
            {
                // restart logs, then repost announcements to them
                var ls = _logs.Count;
                for (var x = 0; x < ls; x++)
                {
                    var l = (Log)(_logs[x]);
                    _logs[x] = l = l.Restarter.Restart(l);
                    if (!l.RepostAnnouncementsOnRestart || !_storeAnnouncements) continue;
                    var ac = _announcements.Count;
                    for (var y = 0; y < ac; y++)
                    {
                        var a = (Announcement)(_announcements[y]);
                        PrintLn(a.Text, l, true, true);
                    }
                }

                // exit with a fatal error if the errors flag is set. 
                ExitIfErrors();
            }
        }

        #endregion // Constructors and Restart
        #region IDisposable and Exit and Close

        /// <summary>
        /// Closes the logs -- ONLY call this if you are preparing to quit 
        /// </summary>
        public virtual void Close()
        {
            lock (this)
            {
                // just in case
                Flush();

                foreach (var log in _logs.Cast<Log>().Where(log => log.Writer != null))
                {
                    log.Writer.Dispose();
                }
            }
        }

        /// <summary>
        /// Should this really be handling the exit?
        /// </summary>
        private void ExitWithError()
        {
            // flush logs first
            Close();

            // exit
            Environment.Exit(1);
        }

        /// <summary>
        /// Exits with a fatal error if the error flag has been raised. 
        /// </summary>
        public virtual void ExitIfErrors()
        {
            lock (this)
            {
                if (!_errors) return;
                PrintLn("SYSTEM EXITING FROM ERRORS\n", ALL_LOGS, true);
                ExitWithError();
            }
        }

        ~Output()
        {
            // flush the logs
            try
            {
                Close();
            }
            catch { /* can't do much of anything here */ }
        }

        #endregion // IDisposable
        #region Flush and Clear

        /// <summary>
        /// Flushes the logs 
        /// </summary>
        public virtual void Flush()
        {
            // BRS : TODO : None of this nonsense seems to help when running in MSTest (which may mean that multi-threading needs attention).
            // NOTE: Although the tests DO pass, there is an exception in the test result details.
            // All in all, the Flush(), Close(), Dispose(), Finalize semantics of all of this needs to be cleaned up considerably!
            lock (this)
            {
                foreach (var log in _logs.Cast<Log>().Where(log => log != null && log.Writer != null))
                {
                    try
                    {
                        log.Writer.Flush();
                    }
                    catch (IOException ex)
                    {
                        throw new ApplicationException(String.Format("Error while trying to flush log [{0}]", log), ex);
                    }
                }
                try
                {
                    // just in case...
                    Console.Out.Flush();
                    Console.Error.Flush();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error while trying to flush Console streams (see inner exception for details).", ex);
                }
            }
        }

        /// <summary>
        /// Clears the error flag. 
        /// </summary>
        public virtual void ClearErrors()
        {
            lock (this)
            {
                _errors = false;
            }
        }

        /// <summary>
        /// Clears out announcements.  Note that this will cause these
        /// announcements to be unavailable for reposting after a restart! 
        /// </summary>
        public virtual void ClearAnnouncements()
        {
            lock (this)
            {
                if (_announcements != null)
                    _announcements = ArrayList.Synchronized(new ArrayList(10));
            }
        }

        #endregion // Flush and Clear
        #region Compression

        /// <summary>
        /// Returns a compressing input stream using JZLib (http://www.jcraft.com/jzlib/).  
        /// If JZLib is not available on your system, this method will return null. 
        /// </summary>
        public static Stream MakeCompressingInputStream(Stream input)
        {
            // to do this, we're going to use reflection.  But here's the equivalent code:
            /* return new com.jcraft.jzlib.ZInputStream(in); */
            try
            {
                return (Stream)(Type.GetType("com.jcraft.jzlib.ZInputStream").GetConstructor(new Type[] { typeof(Stream) }).Invoke(new object[] { input }));
            }
            catch (Exception)
            {
                return null;
            } // failed, probably doesn't have JZLib on the system
        }

        /// <summary>
        /// Returns a compressing output stream using JZLib (http://www.jcraft.com/jzlib/).  
        /// If JZLib is not available on your system, this method will return null. 
        /// </summary>
        public static Stream MakeCompressingOutputStream(Stream output)
        {
            // to do this, we're going to use reflection.  But here's the equivalent code:
            /*
            com.jcraft.jzlib.ZOutputStream stream = new com.jcraft.jzlib.ZOutputStream(out, com.jcraft.jzlib.JZlib.Z_BEST_SPEED);
            stream.setFlushMode(com.jcraft.jzlib.JZlib.Z_SYNC_FLUSH);
            return stream;
            */
            try
            {
                var outz = Type.GetType("com.jcraft.jzlib.JZlib");
                var Z_BEST_SPEED = (int)outz.GetField("Z_BEST_SPEED", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).GetValue(null);
                var Z_SYNC_FLUSH = (int)outz.GetField("Z_SYNC_FLUSH", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).GetValue(null);

                var outc = Type.GetType("com.jcraft.jzlib.ZOutputStream");
                var outi = outc.GetConstructor(new[] { typeof(Stream), Type.GetType("int") }).Invoke(new object[] { output, (int)Z_BEST_SPEED });
                outc.GetMethod("setFlushMode", (new[] { Type.GetType("int") } == null) ? new Type[0] : new[] { Type.GetType("int") }).Invoke(outi, new object[] { (int)Z_SYNC_FLUSH });
                return (Stream)outi;
            }
            catch (Exception)
            {
                return null;
            } // failed, probably doesn't have JZLib on the system
        }

        #endregion // Compression
        #region Public Properties

        /// <summary>
        /// The <i>FilePrefix</i> property is guaranteed to always return a non-null value,
        /// either the string that has been previously set by a client, or an empty string.
        /// </summary>
        /// <remarks>
        /// This guarantee would not normally be required. But in <i>ECJ</i> it is at least
        /// implied, because the value is set at construction time to an empty string.
        /// Here we are formalizing that in case legacy code has come to rely on it
        /// for whatever reason.
        /// </remarks>
        public virtual string FilePrefix
        {
            get
            {
                return String.IsNullOrEmpty(_filePrefix) ? "" : _filePrefix;
            }
            set
            {
                _filePrefix = String.IsNullOrEmpty(value) ? "" : value;
            }
        }
        private string _filePrefix = "";

        /// <summary>
        /// Gets or Sets whether the Output flushes its announcements.
        /// </summary>
        public virtual bool AutoFlush
        {
            get
            {
                lock (this)
                {
                    return _autoFlush;
                }
            }            
            set
            {
                lock (this)
                {
                    _autoFlush = value;
                }
            }           
        }
        private bool _autoFlush = true;

        /// <summary>
        /// Sets whether the Output stores its announcements.
        /// </summary>
        public virtual bool StoreAnnouncements
        {
            // Returns the Output's storing behavior.
            get
            {
                lock (this) { return _storeAnnouncements; }
            }
            set
            {
                lock (this) { _storeAnnouncements = value; }
            }
        }
        private bool _storeAnnouncements;

        public virtual int NumAnnouncements
        {
            get
            {
                lock (this)
                {
                    return _announcements != null ? _announcements.Count : 0;
                }
            }
        }

        /// <summary>
        /// Returns the number of logs currently posted. 
        /// </summary>
        public virtual int NumLogs
        {
            get
            {
                lock (this)
                {
                    return _logs.Count;
                }
            }
        }

        public bool HasErrors
        {
            get
            {
                lock (this)
                {
                    return _errors;
                }
            }
        }

        #endregion // Public Properties
        #region AddLog and RemoveLog and Reopen

        /// <summary>
        /// Creates a new log and adds it to Output.  
        /// This log will write to the file <i>fileName</i>, and you may
        /// NOT post announcements to the log. If the log must be
        /// reset upon restarting from a checkpoint, 
        /// it will NOT append to the file (if one already exists with that name),
        /// but rather will create or overwrite the specified file.
        /// Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart.
        /// </summary>
        public int AddLog(string fileName)
        {
            var name = String.IsNullOrEmpty(_filePrefix) ? fileName : _filePrefix + fileName;
            return AddLog(new Log(new FileInfo(name), false, false, false));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.  
        /// This log will write to the file <i>fileName</i>, and you may
        /// NOT post announcements to the log. If the log must be
        /// reset upon restarting from a checkpoint, it will append to the file
        /// or erase the file and start over depending on <i>appendOnRestart</i>.
        /// Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart. 
        /// </summary>
        public int AddLog(string fileName, bool appendOnRestart)
        {
            var name = String.IsNullOrEmpty(_filePrefix) ? fileName : _filePrefix + fileName;
            return AddLog(new Log(new FileInfo(name), false, appendOnRestart, false));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.  
        /// This log will write to the file <i>fileName</i>, and you may
        /// NOT post announcements to the log. If the log must be
        /// reset upon restarting from a checkpoint, it will append to the file
        /// or erase the file and start over depending on <i>appendOnRestart</i>.
        /// Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart.
        /// The log can be compressed with gzip, but you cannot appendOnRestart
        /// and compress at the same time.
        /// </summary>
        public int AddLog(string fileName, bool appendOnRestart, bool gzip)
        {
            var name = String.IsNullOrEmpty(_filePrefix) ? fileName : _filePrefix + fileName;
            return AddLog(new Log(new FileInfo(name), false, appendOnRestart, gzip));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.  
        /// This log will write to the file <i>fileName</i>, and may
        /// or may not <i>post announcements</i> to the log. If the log must be
        /// reset upon restarting from a checkpoint, it will append to the file
        /// or erase the file and start over depending on <i>appendOnRestart</i>.
        /// If <i>appendOnRestart</i> is false and <i>postAnnouncements</i> is
        /// true, then this log will repost all the announcements on restarting
        /// from a checkpoint. Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart.
        /// The log can be compressed with gzip, but you cannot appendOnRestart
        /// and compress at the same time.
        /// </summary>
        public int AddLog(string fileName, bool postAnnouncements, bool appendOnRestart, bool gzip)
        {
            var name = String.IsNullOrEmpty(_filePrefix) ? fileName : _filePrefix + fileName;
            return AddLog(new Log(new FileInfo(name), postAnnouncements, appendOnRestart, gzip));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.  
        /// This log will write to the file <i>fileInfo</i>, and you may
        /// not post announcements to the log. If the log must be
        /// reset upon restarting from a checkpoint. It will create a new file,
        /// overwriting any file with the same name (if one exists).
        /// Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart. 
        /// Note that if there is a <i>FilePrefix</i> defined, the log will be
        /// created from a new <see cref="FileInfo"/> that includes the prefix.
        /// </summary>
        public int AddLog(FileInfo fileInfo)
        {
            var name = String.IsNullOrEmpty(_filePrefix) ? fileInfo.FullName : _filePrefix + fileInfo.FullName;
            return AddLog(new Log(new FileInfo(name), false, false, false));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.  
        /// This log will write to the file <i>fileInfo</i>, and you may
        /// not post announcements to the log. If the log must be
        /// reset upon restarting from a checkpoint, it will append to the file
        /// or erase the file and start over depending on <i>appendOnRestart</i>.
        /// Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart. 
        /// Note that if there is a <i>FilePrefix</i> defined, the log will be
        /// created from a new <see cref="FileInfo"/> that includes the prefix.
        /// </summary>
        public int AddLog(FileInfo fileInfo, bool appendOnRestart)
        {
            var name = String.IsNullOrEmpty(_filePrefix) ? fileInfo.FullName : _filePrefix + fileInfo.FullName;
            return AddLog(new Log(new FileInfo(name), false, appendOnRestart, false));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.  
        /// This log will write to the file <i>fileInfo</i>, and you may
        /// NOT post announcements to the log. If the log must be
        /// reset upon restarting from a checkpoint, it will append to the file
        /// or erase the file and start over depending on <i>appendOnRestart</i>.
        /// Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart.
        /// The log can be compressed with gzip, but you cannot appendOnRestart
        /// and compress at the same time.
        /// Note that if there is a <i>FilePrefix</i> defined, the log will be
        /// created from a new <see cref="FileInfo"/> that includes the prefix.
        /// </summary>
        public int AddLog(FileInfo fileInfo, bool appendOnRestart, bool gzip)
        {
            var name = String.IsNullOrEmpty(_filePrefix) ? fileInfo.FullName : _filePrefix + fileInfo.FullName;
            return AddLog(new Log(new FileInfo(name), false, appendOnRestart, gzip));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.  
        /// This log will write to the file <i>file</i>, and may
        /// or may not <i>post announcements</i> to the log. If the log must be
        /// reset upon restarting from a checkpoint, it will append to the file
        /// or erase the file and start over depending on <i>appendOnRestart</i>.
        /// If <i>appendOnRestart</i> is false and <i>postAnnouncements</i> is
        /// true, then this log will repost all the announcements on restarting
        /// from a checkpoint. Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart.
        /// The log can be compressed with gzip, but you cannot appendOnRestart
        /// and compress at the same time.
        /// Note that if there is a <i>FilePrefix</i> defined, the log will be
        /// created from a new <see cref="FileInfo"/> that includes the prefix.
        /// </summary>
        public int AddLog(FileInfo fileInfo, bool postAnnouncements, bool appendOnRestart, bool gzip)
        {
            var name = String.IsNullOrEmpty(_filePrefix) ? fileInfo.FullName : _filePrefix + fileInfo.FullName;
            return AddLog(new Log(new FileInfo(name), postAnnouncements, appendOnRestart, gzip));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.
        /// This log will write to stdout (descriptor == Log.D_STDOUT) 
        /// or stderr (descriptor == Log.D_STDERR), and may or may not
        /// <i>post announcements</i> to the log. Returns the position of the 
        /// log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart. 
        /// </summary>
        public int AddLog(int descriptor, bool postAnnouncements)
        {
            return AddLog(new Log(descriptor, postAnnouncements));
        }

        /// <summary>
        /// Creates a new log and adds it to Output.
        /// This log may or may not <i>post announcements</i> to
        /// the log, and if it does, it additionally may or may not <i>repost</i>
        /// all of its announcements to the log upon a restart.  The log
        /// writes to <i>writer</i>, which is reset upon system restart by
        /// <i>restarter</i>. Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart. 
        /// </summary>
        public int AddLog(StreamWriter writer, LogRestarter restarter, bool postAnnouncements, bool repostAnnouncements)
        {
            return AddLog(new Log(writer, restarter, postAnnouncements, repostAnnouncements));
        }

        /// <summary>
        /// Adds the given log to Output.  In general you shouldn't use this
        /// method unless you really <i>really</i> need something custom. 
        /// Returns the position of the log in Output's 
        /// collection of logs -- you should use this to access the log always;
        /// never store the log itself, which may go away upon a system restart. 
        /// </summary>
        public virtual int AddLog(Log l)
        {
            lock (this)
            {
                _logs.Add(l);
                return _logs.Count - 1;
            }
        }

        /// <summary>
        /// Removes the given log. 
        /// </summary>
        public virtual Log RemoveLog(int x)
        {
            lock (this)
            {
                var l = GetLog(x);
                _logs.RemoveAt(x);
                return l;
            }
        }

        #region Reopen

        /// <summary>
        /// Forces a file-based log to reopen, erasing its previous contents.
        /// Non-file logs ignore this. 
        /// </summary>		
        public virtual void Reopen(int log)
        {
            lock (this)
            {
                var oldlog = (Log)_logs[log];
                _logs[log] = oldlog.Reopen();
            }
        }

        /// <summary>
        /// Forces one or more file-based logs to reopen, erasing 
        /// their previous contents.  non-file logs ignore this. 
        /// </summary>	
        public virtual void Reopen(int[] logs)
        {
            lock (this)
            {
                foreach (var t in logs)
                {
                    var oldlog = (Log)_logs[t];
                    _logs[t] = oldlog.Reopen();
                }
            }
        }

        #endregion // Reopen

        /// <summary>
        /// Returns the given log. 
        /// </summary>
        public virtual Log GetLog(int x)
        {
            lock (this)
            {
                return (Log)_logs[x];
            }
        }

        #endregion // AddLog and RemoveLog

        // This is where all the various writing operations are defined.
        #region Messages

        #region Message

        /// <summary>
        /// Prints an initial message to System.err.  This is only to
        /// be used by ec.Evolve in starting up the system.  These messages are not logged. 
        /// </summary>
        public static void InitialMessage(string s)
        {
            Console.Error.WriteLine(s);
            Console.Error.Flush();
        }

        /// <summary>
        /// Posts a system message. 
        /// </summary>
        public virtual void SystemMessage(string s)
        {
            lock (this)
            {
                PrintLn(s, ALL_LOGS, true);
            }
        }

        /// <summary>
        /// Posts a message. 
        /// </summary>
        public virtual void Message(string s)
        {
            lock (this)
            {
                PrintLn(s, ALL_LOGS, true);
            }
        }

        #endregion
        #region Fatal

        /// <summary>
        /// Posts a fatal error.  This causes the system to exit. 
        /// </summary>
        public virtual void Fatal(string s)
        {
            lock (this)
            {
                PrintLn("FATAL ERROR:\n" + s, ALL_LOGS, true);
                ExitWithError();
            }
        }

        /// <summary>
        /// Posts a fatal error.
        /// Optionally terminate immediately as specified by the <i>exit</i> argument.
        /// </summary>
        public virtual void Fatal(string s, bool exit)
        {
            lock (this)
            {
                PrintLn("FATAL ERROR:\n" + s, ALL_LOGS, true);
                if (exit)
                    ExitWithError();
            }
        }

        /// <summary>
        /// Posts a fatal error.  This causes the system to exit. 
        /// </summary>
        public virtual void Fatal(string s, IParameter p1)
        {
            lock (this)
            {
                PrintLn("FATAL ERROR:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
                ExitWithError();
            }
        }

        /// <summary>
        /// Posts a fatal error.
        /// Optionally terminate immediately as specified by the <i>exit</i> argument.
        /// </summary>
        public virtual void Fatal(string s, IParameter p1, bool exit)
        {
            lock (this)
            {
                PrintLn("FATAL ERROR:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
                if (exit)
                    ExitWithError();
            }
        }

        /// <summary>
        /// Posts a fatal error.  This causes the system to exit. 
        /// </summary>
        public virtual void Fatal(string s, IParameter p1, IParameter p2)
        {
            lock (this)
            {
                PrintLn("FATAL ERROR:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
                if (p2 != null && p1 != null)
                {
                    PrintLn("     ALSO: " + p2, ALL_LOGS, true);
                }
                else
                {
                    PrintLn("PARAMETER: " + p2, ALL_LOGS, true);
                }
                ExitWithError();
            }
        }

        /// <summary>
        /// Posts a fatal error. 
        /// Optionally terminate immediately as specified by the <i>exit</i> argument.
        /// </summary>
        public virtual void Fatal(string s, IParameter p1, IParameter p2, bool exit)
        {
            lock (this)
            {
                PrintLn("FATAL ERROR:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
                if (p2 != null && p1 != null)
                {
                    PrintLn("     ALSO: " + p2, ALL_LOGS, true);
                }
                else
                {
                    PrintLn("PARAMETER: " + p2, ALL_LOGS, true);
                }
                if (exit)
                    ExitWithError();
            }
        }

        #endregion // Fatal
        #region Error

        #region InitialError

        /// <summary>
        /// Prints an initial error to System.err.  This is only to
        /// be used by ec.Evolve in starting up the system. 
        /// This causes the system to exit. 
        /// </summary>
        /// <remarks>
        /// This writes directly to STDERR.
        /// </remarks>
        public static void InitialError(string s)
        {
            Console.Error.WriteLine("STARTUP ERROR:\n" + s);

            // just in case...
            Console.Out.Flush();
            Console.Error.Flush();
            Environment.Exit(1);
        }

        /// <summary>
        /// Prints an initial error to System.err.  This is only to
        /// be used by ec.Evolve in starting up the system. 
        /// Optionally terminate immediately as specified by the <i>exit</i> argument.
        /// </summary>
        /// <remarks>
        /// This writes directly to STDERR.
        /// </remarks>
        public static void InitialError(string s, bool exit)
        {
            Console.Error.WriteLine("STARTUP ERROR:\n" + s);

            // just in case...
            Console.Out.Flush();
            Console.Error.Flush();
            if (exit)
                Environment.Exit(1);
        }

        /// <summary>
        /// Prints an initial error to System.err.  This is only to
        /// be used by ec.Evolve in starting up the system.
        /// A parameter value can be specified to indicate the cause of the problem.
        /// This causes the system to exit. 
        /// </summary>
        /// <remarks>
        /// This writes directly to STDERR.
        /// </remarks>
        public static void InitialError(string s, IParameter p1)
        {
            Console.Error.WriteLine("STARTUP ERROR:\n" + s);
            if (p1 != null)
            {
                Console.Error.WriteLine("PARAMETER: " + p1);
            }

            // just in case...
            Console.Out.Flush();
            Console.Error.Flush();
            Environment.Exit(1);
        }

        /// <summary>
        /// Prints an initial error to System.err.  This is only to
        /// be used by ec.Evolve in starting up the system.
        /// A parameter value can be specified to indicate the cause of the problem.
        /// Optionally terminate immediately as specified by the <i>exit</i> argument.
        /// </summary>
        /// <remarks>
        /// This writes directly to STDERR.
        /// </remarks>
        public static void InitialError(string s, IParameter p1, bool exit)
        {
            Console.Error.WriteLine("STARTUP ERROR:\n" + s);
            if (p1 != null)
            {
                Console.Error.WriteLine("PARAMETER: " + p1);
            }

            // just in case...
            Console.Out.Flush();
            Console.Error.Flush();
            if (exit)
                Environment.Exit(1);
        }

        /// <summary>
        /// Prints an initial error to System.err.  This is only to
        /// be used by ec.Evolve in starting up the system. 
        /// Two parameter arguments can be specified to indicate the cause of the problem.
        /// This causes the system to exit. 
        /// </summary>
        /// <remarks>
        /// This writes directly to STDERR.
        /// </remarks>
        public static void InitialError(string s, IParameter p1, IParameter p2)
        {
            Console.Error.WriteLine("STARTUP ERROR:\n" + s);
            if (p1 != null)
            {
                Console.Error.WriteLine("PARAMETER: " + p1);
            }
            if (p2 != null && p1 != null)
            {
                Console.Error.WriteLine("     ALSO: " + p2);
            }

            // just in case...
            Console.Out.Flush();
            Console.Error.Flush();
            Environment.Exit(1);
        }

        /// <summary>
        /// Prints an initial error to System.err.  This is only to
        /// be used by ec.Evolve in starting up the system. 
        /// Two parameter arguments can be specified to indicate the cause of the problem.
        /// Optionally terminate immediately as specified by the <i>exit</i> argument.
        /// </summary>
        /// <remarks>
        /// This writes directly to STDERR.
        /// </remarks>
        public static void InitialError(string s, IParameter p1, IParameter p2, bool exit)
        {
            Console.Error.WriteLine("STARTUP ERROR:\n" + s);
            if (p1 != null)
            {
                Console.Error.WriteLine("PARAMETER: " + p1);
            }
            if (p2 != null && p1 != null)
            {
                Console.Error.WriteLine("     ALSO: " + p2);
            }

            // just in case...
            Console.Out.Flush();
            Console.Error.Flush();
            if (exit)
                Environment.Exit(1);
        }

        #endregion // InitialError

        /// <summary>
        /// Posts a simple error. This causes the error flag to be raised as well. 
        /// </summary>
        public virtual void Error(string s)
        {
            lock (this)
            {
                PrintLn("ERROR:\n" + s, ALL_LOGS, true);
                _errors = true;
            }
        }

        /// <summary>
        /// Posts a simple error. This causes the error flag to be raised as well. 
        /// </summary>
        public virtual void Error(string s, IParameter p1)
        {
            lock (this)
            {
                PrintLn("ERROR:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
                _errors = true;
            }
        }

        /// <summary>
        /// Posts a simple error. This causes the error flag to be raised as well. 
        /// </summary>
        public virtual void Error(string s, IParameter p1, IParameter p2)
        {
            lock (this)
            {
                PrintLn("ERROR:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
                if (p2 != null && p1 != null)
                {
                    PrintLn("     ALSO: " + p2, ALL_LOGS, true);
                }
                else
                {
                    PrintLn("PARAMETER: " + p2, ALL_LOGS, true);
                }
                _errors = true;
            }
        }

        #endregion // Error
        #region Warning

        /// <summary>
        /// Posts a warning. 
        /// </summary>
        public virtual void Warning(string s)
        {
            lock (this)
            {
                PrintLn("WARNING:\n" + s, ALL_LOGS, true);
            }
        }

        /// <summary>
        /// Posts a warning. 
        /// </summary>
        public virtual void Warning(string s, IParameter p1)
        {
            lock (this)
            {
                PrintLn("WARNING:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
            }
        }

        /// <summary>
        /// Posts a warning. 
        /// </summary>
        public virtual void Warning(string s, IParameter p1, IParameter p2)
        {
            lock (this)
            {
                PrintLn("WARNING:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
                if (p2 != null && p1 != null)
                {
                    PrintLn("     ALSO: " + p2, ALL_LOGS, true);
                }
                else
                {
                    PrintLn("PARAMETER: " + p2, ALL_LOGS, true);
                }
            }
        }

        #endregion // Warning
        #region WarnOnce

        /// <summary>
        /// Posts a warning one time only. 
        /// </summary>
        public virtual void WarnOnce(string s)
        {
            lock (this)
            {
                if (_oneTimeWarnings.Contains(s)) return;
                _oneTimeWarnings.Add(s);
                PrintLn("ONCE-ONLY WARNING:\n" + s, ALL_LOGS, true);
            }
        }

        public virtual void WarnOnce(string s, IParameter p1)
        {
            lock (this)
            {
                if (_oneTimeWarnings.Contains(s)) return;
                _oneTimeWarnings.Add(s);
                PrintLn("ONCE-ONLY WARNING:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
            }
        }

        public virtual void WarnOnce(string s, IParameter p1, IParameter p2)
        {
            lock (this)
            {
                if (_oneTimeWarnings.Contains(s)) return;
                _oneTimeWarnings.Add(s);
                PrintLn("ONCE-ONLY WARNING:\n" + s, ALL_LOGS, true);
                if (p1 != null)
                {
                    PrintLn("PARAMETER: " + p1, ALL_LOGS, true);
                }
                if (p2 != null && p1 != null)
                {
                    PrintLn("     ALSO: " + p2, ALL_LOGS, true);
                }
                else
                {
                    PrintLn("PARAMETER: " + p2, ALL_LOGS, true);
                }
            }
        }

        #endregion // WarnOnce
        #region PrintLn

        /// <summary>
        /// Prints a message to a given log.  
        /// The <i>announcement</i> argument indicates that the message is an announcement. 
        /// </summary>		
        protected internal virtual void PrintLn(string s, Log log, bool announcement, bool reposting)
        {
            lock (this)
            {
                if (log.Writer == null)
                {
                    throw new OutputException("Log with a null writer: " + log);
                }
                if (!log.PostAnnouncements && announcement)
                    return; // don't write it
                if (log.Muzzle) return;  // don't write it

                // now write it
                log.Writer.WriteLine(s);
                if (_autoFlush)
                    log.Writer.Flush();
                //...and stash it in memory maybe
                if (_storeAnnouncements && announcement && !reposting)
                    _announcements.Add(new Announcement(s));
            }
        }

        /// <summary>
        /// Prints a message to a given log.
        /// If log==ALL_LOGS, posted to all logs which accept announcements. 
        /// </summary>
        public void PrintLn(string s, int log, bool announcement)
        {
            lock (this)
            {
                if (log == ALL_LOGS)
                    for (var x = 0; x < _logs.Count; x++)
                    {
                        var l = (Log) _logs[x];
                        if (l == null) throw new OutputException("Unknown log number" + l);
                        PrintLn(s, l, announcement, false);
                    }
                else
                {
                    var l = (Log) _logs[log];
                    if (l == null) throw new OutputException("Unknown log number" + l);
                    PrintLn(s, l, announcement, false);
                }
            }
        }

        /// <summary>
        /// Prints a non-announcement message to the given logs.
        /// </summary>
        public void PrintLn(String s, int log)
        {
            PrintLn(s, (Log)_logs[log], false, false);
        }

        #endregion // PrintLn
        #region Print

        /// <summary>
        /// Prints a non-announcement message to a given log.
        /// If log==ALL_LOGS, posted to all logs which accept announcements. No '\n' is printed.
        /// </summary>
        public void Print(String s, int log)
        {
            lock (this)
            {
                if (log == ALL_LOGS)
                    for (var x = 0; x < _logs.Count; x++)
                    {
                        var l = (Log) _logs[x];
                        if (l == null) throw new OutputException("Unknown log number" + l);
                        Print(s, l);
                    }
                else
                {
                    var l = (Log) _logs[log];
                    if (l == null) throw new OutputException("Unknown log number" + l);
                    Print(s, l);
                }
            }
        }

        /// <summary>
        /// Prints a non-announcement message to a given log. No '\n' is printed.  
        /// </summary>
        public void Print(string s, Log log)
        {
            lock (this)
            {
                if (log.Writer == null)
                {
                    throw new OutputException("Log with a null writer: " + log);
                }
                // now write it
                log.Writer.Write(s);
            }
        }

        #endregion // Print

        #endregion // Messages
    }
}
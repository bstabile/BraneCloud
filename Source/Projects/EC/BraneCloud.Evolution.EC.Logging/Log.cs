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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BraneCloud.Evolution.EC.Logging
{
    /// <summary> 
    /// Defines a log to which Output outputs.  A log consists of three items:
    /// <ol>
    /// <li/> A PrintWriter which prints out messages.
    /// <li/> A flag which indicates whether or not to filter out (refuse to print)
    /// announcements (usually error messages and warnings).
    /// </ol>
    /// 
    /// Logs can be <i>restarted</i> after a computer outage by using the 
    /// information stored in the LogRestarter
    /// <tt>Restarter</tt>.  
    /// 
    /// </summary>	
    [Serializable]
    public class Log
    {
        [Serializable]
        private class AnonymousClassLogRestarter : LogRestarter
        {

            public override Log Restart(Log l)
            {
                return Reopen(l);
            }
            public override Log Reopen(Log l)
            {
                if (l.Writer != null && !l.IsLoggingToSystemOut)
                {
                    l.Writer.Close();
                }
                // BRS : TODO : This was reduced to the simplest implementation. Need to add the appropriate expected behavior.
                l.Writer = new StreamWriter(l.FileInfo.FullName, l.AppendOnRestart, Encoding.Default);
                return l;
            }
        }
        [Serializable]
        private class AnonymousClassLogRestarter1 : LogRestarter
        {
            public AnonymousClassLogRestarter1(Log enclosingInstance)
            {
                InitBlock(enclosingInstance);
            }
            private void  InitBlock(Log enclosingInstance)
            {
                EnclosingInstance = enclosingInstance;
            }

            private Log EnclosingInstance { get; set; }

            public override Log Restart(Log l)
            {
                l.Writer = new StreamWriter(l.FileInfo.FullName, l.AppendOnRestart, Encoding.Default);
                return l;
            }
            public override Log Reopen(Log l)
            {
                if (l.Writer != null && !EnclosingInstance.IsLoggingToSystemOut)
                {
                    l.Writer.Close();
                }
                l.Writer = new StreamWriter(l.FileInfo.FullName, false, Encoding.Default);
                return l;
            }
        }
        [Serializable]
        private class AnonymousClassLogRestarter2 : LogRestarter
        {
            public override Log Restart(Log l)
            {
                l.Writer = new StreamWriter(Console.OpenStandardOutput(), Encoding.Default);
                return l;
            }
            public override Log Reopen(Log l)
            {
                return l; // makes no sense
            }
        }
        [Serializable]
        private class AnonymousClassLogRestarter3 : LogRestarter
        {
            public override Log Restart(Log l)
            {
                l.Writer = new StreamWriter(Console.OpenStandardError(), Encoding.Default);
                return l;
            }
            public override Log Reopen(Log l)
            {
                return l; // makes no sense
            }
        }

        // basic log features

        #region Constants

        private const long SerialVersionUID = 1;

        // values for specifying logs based on System.out or System.err

        /// <summary>
        /// Specifies that the log should write to stdout (System.out) 
        /// </summary>
        public const int D_STDOUT = 0;

        /// <summary>
        /// Specifies that the log should write to stderr (System.err) 
        /// </summary>
        public const int D_STDERR = 1;

        #endregion

        /// <summary>
        /// Should we write to this log at all?
        /// </summary>
        public bool Muzzle { get; set; }

        /// <summary>The log's Writer </summary>
        public TextWriter Writer
        {
            get => _writer;
            set => _writer = value;
        }
        [NonSerialized]
        private TextWriter _writer;

        /// <summary>A filename, if the writer writes to a file </summary>
        public FileInfo FileInfo
        {
            get => _fileInfo;
            set => _fileInfo = value;
        }
        private FileInfo _fileInfo;

        /// <summary>Should the log post announcements? </summary>
        public bool PostAnnouncements
        {
            get => _postAnnouncements;
            set => _postAnnouncements = value;
        }
        private bool _postAnnouncements = true;
        
        // stuff for restarting
        
        /// <summary>The log's Restarter </summary>
        public LogRestarter Restarter
        {
            get => _restarter;
            set => _restarter = value;
        }
        private LogRestarter _restarter;

        /// <summary>Should the log repost all announcements on restart </summary>
        public bool RepostAnnouncementsOnRestart
        {
            get => _repostAnnouncementsOnRestart;
            set => _repostAnnouncementsOnRestart = value;
        }
        private bool _repostAnnouncementsOnRestart;
        
        /// <summary>
        /// If the log writes to a file, should it append to the file on restart,
        /// or should it overwrite the file? 
        /// </summary>
        public bool AppendOnRestart
        {
            get => _appendOnRestart;
            set => _appendOnRestart = value;
        }
        private bool _appendOnRestart;

        public bool IsLoggingToSystemOut
        {
            get => _isLoggingToSystemOut;
            set => _isLoggingToSystemOut = value;
        }
        private bool _isLoggingToSystemOut;
        
       ~Log()
        {
            // BRS : For some reason this was throwing an exception when running in multi-threaded MSTest agent cases.
            // For now we're just swallowing it. But it would be nice to know why this is happening! :-/
            try
            {
                // rarely happens though... :-(
                if (Writer != null && !IsLoggingToSystemOut)
                {
                    Writer.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Creates a log to a given filename; this file may or may not
        /// be appended to on restart, depending on _AppendOnRestart.  If
        /// and only if the file is <i>not</i> appended to on restart, then
        /// announcements are reposted on restart.
        /// </summary>		
        public Log(string filename, bool postAnnouncements, bool appendOnRestart)
            : this(new FileInfo(filename), postAnnouncements, appendOnRestart, false)
        {
        }

        /// <summary>
        /// Creates a log to a given filename; this file may or may not
        /// be appended to on restart, depending on _AppendOnRestart.  If
        /// and only if the file is <i>not</i> appended to on restart, then
        /// announcements are reposted on restart.
        /// </summary>		
        public Log(FileInfo filename, bool postAnnouncements, bool appendOnRestart) 
            : this(filename, postAnnouncements, appendOnRestart, false)
        {
        }
        
        /// <summary>
        /// Creates a log to a given filename; this file may or may not
        /// be appended to on restart, depending on _AppendOnRestart.  If
        /// and only if the file is <i>not</i> appended to on restart, then
        /// announcements are reposted on restart. The file can be compressed
        /// with gzip, but you may not both gzip AND AppendOnRestart.  If gzipped,
        /// then .gz is automagically appended to the file name.
        /// </summary>		
        public Log(FileInfo fileInfo, bool postAnnouncements, bool appendOnRestart, bool gzip)
        {
            _postAnnouncements = postAnnouncements;
            _repostAnnouncementsOnRestart = !appendOnRestart;
            _appendOnRestart = appendOnRestart;
            _isLoggingToSystemOut = false;
            
            if (gzip)
            {
                _fileInfo = new FileInfo(fileInfo.FullName + ".gz");
                if (AppendOnRestart)
                    throw new IOException("Cannot gzip and AppendOnRestart at the same time: new Log(File,bool,bool,bool)");
                
                _writer = new StreamWriter(fileInfo.FullName);

                Restarter = new AnonymousClassLogRestarter();
            }
            else
            {
                _fileInfo = fileInfo;
                _writer = new StreamWriter(fileInfo.FullName, false, Encoding.Default);

                _restarter = new AnonymousClassLogRestarter1(this);
            }
        }
        
        /// <summary>
        /// Creates a log on stdout (descriptor == Log.D_STDOUT) 
        /// or stderr (descriptor == Log.D_STDERR). 
        /// </summary>
        public Log(int descriptor, bool postAnnouncements)
        {
            _fileInfo = null;
            _postAnnouncements = postAnnouncements;
            _repostAnnouncementsOnRestart = true;
            _appendOnRestart = true; // doesn't matter
            _isLoggingToSystemOut = true;

            if (descriptor == D_STDOUT)
            {
                _writer = Console.Out;
                //_writer = new StreamWriter(System.Console.OpenStandardOutput(), System.Text.Encoding.Default);
                _restarter = new AnonymousClassLogRestarter2();
            }
            // D_STDERR
            else
            {
                _writer = Console.Error;
                //_writer = new StreamWriter(System.Console.OpenStandardError(), System.Text.Encoding.Default);
                _restarter = new AnonymousClassLogRestarter3();
            }
        }

        /// <summary>
        /// Creates a log on a given Writer and custom LogRestarter.  
        /// In general, you should not use this to write to a file.  Use Log(filename...) instead.
        /// </summary>
        public Log(TextWriter writer, LogRestarter restarter, bool postAnnouncements, bool repostAnnouncementsOnRestart)
        {
            _fileInfo = null;
            PostAnnouncements = postAnnouncements;
            RepostAnnouncementsOnRestart = repostAnnouncementsOnRestart;
            AppendOnRestart = true;  // doesn't matter
            /*
             * Unfortunately, we can't know if the specified Writer wraps
             * System.out.  Err on the side of caution and assume we're not.
             */
            IsLoggingToSystemOut = false;
            Writer = writer;
            Restarter = restarter;
        }

        /// <summary>
        /// Restarts a log after a system restart from checkpoint.  Returns
        /// the restarted log -- note that it may not be the same log! 
        /// </summary>		
        public virtual Log Restart()
        {
            return _restarter.Restart(this);
        }
        
        /// <summary>
        /// Forces a file-based log to reopen, erasing its previous contents.
        /// non-file logs ignore this. 
        /// </summary>		
        public virtual Log Reopen()
        {
            return _restarter.Reopen(this);
        }
    }
}
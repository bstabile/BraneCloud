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

using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Logging
{
    public interface IOutput
    {
        string FilePrefix { get; set; }
        bool AutoFlush { get; set; }
        bool StoreAnnouncements { get; set; }
        int NumAnnouncements { get; }
        bool HasErrors { get; }

        void Close();
        void Flush();

        int AddLog(string fileName);
        int AddLog(string fileName, bool appendOnRestart);
        int AddLog(string fileName, bool appendOnRestart, bool gzip);
        int AddLog(string fileName, bool postAnnouncements, bool appendOnRestart, bool gzip);

        int AddLog(FileInfo fileInfo);
        int AddLog(FileInfo fileInfo, bool appendOnRestart);
        int AddLog(FileInfo fileInfo, bool appendOnRestart, bool gzip);
        int AddLog(FileInfo fileInfo, bool postAnnouncements, bool appendOnRestart, bool gzip);


        int AddLog(int descriptor, bool postAnnouncements);

        int AddLog(StreamWriter writer, LogRestarter restarter, bool postAnnouncements, bool repostAnnouncements);

        int AddLog(Log l);

        int NumLogs { get; }
        Log GetLog(int x);
        Log RemoveLog(int x);

        void SystemMessage(string s);

        void Fatal(string s);
        void Fatal(string s, bool exit);
        void Fatal(string s, IParameter p1);
        void Fatal(string s, IParameter p1, bool exit);
        void Fatal(string s, IParameter p1, IParameter p2);
        void Fatal(string s, IParameter p1, IParameter p2, bool exit);

        void Error(string s);
        void Error(string s, IParameter p1);
        void Error(string s, IParameter p1, IParameter p2);

        void Warning(string s);
        void Warning(string s, IParameter p1);
        void Warning(string s, IParameter p1, IParameter p2);

        void WarnOnce(string s);
        void WarnOnce(string s, IParameter p1);
        void WarnOnce(string s, IParameter p1, IParameter p2);

        void Message(string s);

        void Reopen(int log);
        void Reopen(int[] logs);

        void PrintLn(string s, int log, bool announcement);
        void PrintLn(string s, int log);

        void Print(string s, int log);
        void Print(string s, Log log);

        void ExitIfErrors();

        void ClearErrors();
        void ClearAnnouncements();

        void Restart();
    }
}
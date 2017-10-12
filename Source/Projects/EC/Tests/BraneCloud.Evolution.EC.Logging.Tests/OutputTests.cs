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
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BraneCloud.Evolution.EC.Configuration;
using File = System.IO.File;

namespace BraneCloud.Evolution.EC.Logging.Tests
{
    /// <summary>
    /// Summary description for OutputTests
    /// </summary>
    [TestClass]
    public class OutputTests
    {
        #region Housekeeping

        public OutputTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext context { get { return testContextInstance; } }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #endregion // Housekeeping

        #region Constants

        [TestMethod]
        public void AllLogsEqualsMinusOne()
        {
            Assert.AreEqual(Output.ALL_LOGS, -1);
        }

        #endregion // Constants

        #region Constructors

        [TestMethod]
        [Description("Default constructor sets StoreAnnouncements and AutoFlush to true. FilePrefix is empty string. NumLogs is zero.")]
        public void CreateOutputWithDefaultConstructor()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            Assert.IsTrue(output.StoreAnnouncements);
            Assert.IsTrue(output.AutoFlush);
            Assert.AreEqual(output.FilePrefix, "");
            Assert.AreEqual(output.NumLogs, 0);
        }

        [TestMethod]
        [Description("This constructor does nothing but set errors to false and StoreAnnouncements to passed argument.")]
        public void CreateOutputWithStoreAnnouncementsSetToFalse()
        {
            var output = new Output(false);
            Assert.IsNotNull(output);
            Assert.IsFalse(output.StoreAnnouncements);
            Assert.IsTrue(output.AutoFlush);
            Assert.AreEqual(output.FilePrefix, "");
        }

        #endregion // Constructors
        #region Properties

        #region Trivial Property Checks

        [TestMethod]
        [Description("This trivially tests the StoreAnnouncements property.")]
        public void SetStoreAnnouncements()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.StoreAnnouncements = false;
            Assert.IsFalse(output.StoreAnnouncements);
            output.StoreAnnouncements = true;
            Assert.IsTrue(output.StoreAnnouncements);
        }

        [TestMethod]
        [Description("This trivially tests the AutoFlush property.")]
        public void SetAutoFlush()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AutoFlush = false;
            Assert.IsFalse(output.AutoFlush);
            output.AutoFlush = true;
            Assert.IsTrue(output.AutoFlush);
        }

        [TestMethod]
        [Description("This tests the FilePrefix property, with the guarantee that it will always be non-null.")]
        public void SetFilePrefix()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            Assert.AreEqual(output.FilePrefix, "");
            output.FilePrefix = "Hello World!";
            Assert.AreEqual(output.FilePrefix, "Hello World!");
            output.FilePrefix = null;
            Assert.IsNotNull(output.FilePrefix);
        }

        [TestMethod]
        public void GetNumLogs()
        {
            var output = new Output();
            Assert.AreEqual(output.NumLogs, 0);
            output.AddLog(0, true);
            Assert.AreEqual(output.NumLogs, 1);
            output.AddLog(1, false);
            Assert.AreEqual(output.NumLogs, 2);
            output.Close();
        }

        #endregion // Trivial Property Checks
        #region FilePrefix Checks

        [TestMethod]
        [Description("Prefix beginning with alpha character followed by underscore.")]
        public void FilePrefixAlphaUnderscore()
        {
            var prefix = "a_";
            var output = new Output {FilePrefix = prefix};
            output.AddLog("test.log");
            Assert.IsTrue(File.Exists(prefix + "test.log"));
            output.Close();
        }

        [TestMethod]
        [Description("Prefix beginning with alpha character followed by underscore.")]
        public void FilePrefixNumeric()
        {
            var prefix = "1";
            var output = new Output { FilePrefix = prefix };
            output.AddLog("test.log");
            Assert.IsTrue(File.Exists(prefix + "test.log"));
            output.Close();
        }

        [TestMethod]
        [Description("Prefix beginning with alpha character followed by underscore.")]
        public void FilePrefixNumericUnderscore()
        {
            var prefix = "1_";
            var output = new Output { FilePrefix = prefix };
            output.AddLog("test.log");
            Assert.IsTrue(File.Exists(prefix + "test.log"));
            output.Close();
        }

        [TestMethod]
        [Description("Prefix beginning with alpha character followed by underscore.")]
        public void FilePrefixNumericOnly()
        {
            var prefix = "1";
            var output = new Output { FilePrefix = prefix };
            output.AddLog("");
            Assert.IsTrue(File.Exists(prefix + ""));
            output.Close();
        }

        [TestMethod]
        [Description("Prefix beginning with alpha character followed by underscore.")]
        public void FilePrefixUnderscore()
        {
            var prefix = "_";
            var output = new Output { FilePrefix = prefix };
            output.AddLog("test.log");
            Assert.IsTrue(File.Exists(prefix + "test.log"));
            output.Close();
        }

        [TestMethod]
        [Description("Prefix beginning with alpha character followed by underscore.")]
        public void FilePrefixUnderscoreOnly()
        {
            var prefix = "_";
            var output = new Output { FilePrefix = prefix };
            output.AddLog("");
            Assert.IsTrue(File.Exists(prefix + ""));
            output.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        [Description("Prefix of dot with no suffix throws.")]
        public void FilePrefixDotOnlyThrows()
        {
            var prefix = ".";
            var output = new Output { FilePrefix = prefix };
            output.AddLog("");
            Assert.IsTrue(File.Exists(prefix + ""));
            output.Close();
        }

        [TestMethod]
        [Description("Prefix of dot is okay as long as something follows.")]
        public void FilePrefixDotOnlyExtension()
        {
            var prefix = ".";
            var output = new Output { FilePrefix = prefix };
            output.AddLog(".log");
            Assert.IsTrue(File.Exists(prefix + ".log"));
            output.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        [Description("Prefix of dot is okay as long as something follows.")]
        public void FilePrefixWithRepeatNameThrows()
        {
            var prefix = "Output_";
            var output = new Output { FilePrefix = prefix };
            output.AddLog("1.log");
            Assert.IsTrue(File.Exists(prefix + "1.log"));
            output.AddLog("1.log");
            Assert.IsTrue(File.Exists(prefix + "1.log"));
            output.Close();
        }

        [TestMethod]
        [Description("Prefix with multiple numbered logs.")]
        public void FilePrefixWithMultipleNumberedLogs()
        {
            var prefix = "Output_";
            var output = new Output { FilePrefix = prefix };
            output.AddLog("1.log");
            Assert.IsTrue(File.Exists(prefix + "1.log"));
            output.AddLog("2.log");
            Assert.IsTrue(File.Exists(prefix + "2.log"));
            output.Close();
        }

        #endregion
        #region StoreAnnouncement Checks

        [TestMethod]
        [Description("Store announcement. Disable storing does not clear collection. Then when cleared, count is zero even after new message.")]
        public void StoreAnnouncementsDisableAndClear()
        {
            var output = new Output();
            output.AddLog(Log.D_STDERR, true);
            output.Message("Hello World!");
            Assert.AreEqual(output.NumAnnouncements, 1);
            output.StoreAnnouncements = false;
            Assert.AreEqual(output.NumAnnouncements, 1);
            output.ClearAnnouncements();
            Assert.AreEqual(output.NumAnnouncements, 0);
            output.Message("Hi again!");
            Assert.AreEqual(output.NumAnnouncements, 0);
            output.Close();
        }

        #endregion
        #region HasErrors Check

        [TestMethod]
        [Description("Prefix beginning with alpha character followed by underscore.")]
        public void ErrorsNotedAndCleared()
        {
            var output = new Output();
            output.AddLog(Log.D_STDERR, true);
            output.Error("Oops!");
            Assert.IsTrue(output.HasErrors);
            output.ClearErrors();
            Assert.IsFalse(output.HasErrors);
            output.Close();
        }

        #endregion

        #endregion // Properties
        #region AddLog as Log instance

        [TestMethod]
        public void AddErrorLog()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(new Log("err.log", true, false));
            Assert.AreEqual(output.NumLogs, 1);
            Assert.IsTrue(File.Exists("err.log"));
        }

        [TestMethod]
        public void AddErrorLogAndWrite()
        {
            var fileNameErr = MethodInfo.GetCurrentMethod().Name + "_Err.log";
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(new Log(fileNameErr, true, false));
            Assert.AreEqual(output.NumLogs, 1);
            Assert.IsTrue(File.Exists(fileNameErr));
            output.GetLog(0).Writer.WriteLine("Hello World!");
            output.Close();
            var reader = new StreamReader(new FileStream(fileNameErr, FileMode.Open, FileAccess.Read, FileShare.Read));
            var line = reader.ReadLine();
            Assert.AreEqual(line, "Hello World!");
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void AddErrorLogAndWriteThenTryReadWithoutClosingOuputThrows()
        {
            var fileNameErr = MethodInfo.GetCurrentMethod().Name + "_Err.log";
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(new Log(fileNameErr, true, false));
            Assert.AreEqual(output.NumLogs, 1);
            Assert.IsTrue(File.Exists(fileNameErr));
            output.GetLog(0).Writer.WriteLine("Hello World!");
            var reader = new StreamReader(new FileStream(fileNameErr, FileMode.Open, FileAccess.Read, FileShare.Read));
            var line = reader.ReadLine();
            Assert.AreEqual(line, "Hello World!");
        }

        [TestMethod]
        public void AddErrorAndOutLogAndWriteToEach()
        {
            var fileNameErr = MethodInfo.GetCurrentMethod().Name + "_Err.log";
            var fileNameOut = MethodInfo.GetCurrentMethod().Name + "_Out.log";
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(new Log(fileNameErr, true, false));
            output.AddLog(new Log(fileNameOut, true, false));
            Assert.AreEqual(output.NumLogs, 2);
            Assert.IsTrue(File.Exists(fileNameErr));
            Assert.IsTrue(File.Exists(fileNameOut));
            output.GetLog(0).Writer.WriteLine("Hello Error!");
            output.GetLog(1).Writer.WriteLine("Hello Out!");
            output.Close();
            using (var reader = new StreamReader(new FileStream(fileNameErr, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                var line = reader.ReadLine();
                Assert.AreEqual(line, "Hello Error!");
            }
            using (var reader = new StreamReader(new FileStream(fileNameOut, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                var line = reader.ReadLine();
                Assert.AreEqual(line, "Hello Out!");
            }
        }

        [TestMethod]
        public void AddErrorAndOutLogAndWriteTwoLinesToEach()
        {
            var fileNameErr = MethodInfo.GetCurrentMethod().Name + "_Err.log";
            var fileNameOut = MethodInfo.GetCurrentMethod().Name + "_Out.log";
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(new Log(fileNameErr, true, false));
            output.AddLog(new Log(fileNameOut, true, false));
            Assert.AreEqual(output.NumLogs, 2);
            Assert.IsTrue(File.Exists(fileNameErr));
            Assert.IsTrue(File.Exists(fileNameOut));
            output.GetLog(0).Writer.WriteLine("Hello Error!");
            output.GetLog(0).Writer.WriteLine("Hello Error again!");
            output.GetLog(1).Writer.WriteLine("Hello Out!");
            output.GetLog(1).Writer.WriteLine("Hello Out again!");
            output.Close();
            using (var reader = new StreamReader(new FileStream(fileNameErr, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                var line = reader.ReadLine();
                Assert.AreEqual(line, "Hello Error!");
                line = reader.ReadLine();
                Assert.AreEqual(line, "Hello Error again!");
            }
            using (var reader = new StreamReader(new FileStream(fileNameOut, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                var line = reader.ReadLine();
                Assert.AreEqual(line, "Hello Out!");
                line = reader.ReadLine();
                Assert.AreEqual(line, "Hello Out again!");
            }
        }

        #endregion // AddLog as Log instance
        #region AddLog using Descriptors

        [TestMethod]
        public void AddConsoleErrorLogUsingDescriptor()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            Assert.AreEqual(output.NumLogs, 1);
            output.Close();
        }

        [TestMethod]
        public void AddConsoleOutLogUsingDescriptor()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDOUT, true);
            Assert.AreEqual(output.NumLogs, 1);
            output.Close();
        }

        #endregion // AddLog using Descriptors
        #region Print to System Logs

        [TestMethod]
        public void PrintToConsoleError()
        {
            var output = new Output();
            var logNum = output.AddLog(Log.D_STDERR, true);
            output.Print("Hello World!", logNum);
            output.Print("Hello Again World!", logNum); // Prints on same line
            output.PrintLn("", logNum); // Now we've moved to a new line.
            output.Print("Hello for a third time World!", logNum);
            output.Print("Hello for a fourth time World!", logNum);
            output.Close();
        }

        [TestMethod]
        public void PrintToConsoleOut()
        {
            var output = new Output();
            var logNum = output.AddLog(Log.D_STDOUT, true);
            output.Print("Hello World!", logNum);
            output.Print("Hello Again World!", logNum); // Prints on same line
            output.PrintLn("", logNum); // Now we've moved to a new line.
            output.Print("Hello for a third time World!", logNum);
            output.Print("Hello for a fourth time World!", logNum);
            output.Close();
        }

        #endregion // Print to System Logs
        #region PrintLn to System Logs

        [TestMethod]
        public void PrintLnToConsoleError()
        {
            var output = new Output();
            var logNum = output.AddLog(Log.D_STDERR, true);
            output.PrintLn("Hello World!", logNum);
            output.PrintLn("Hello Again World!", logNum); // Prints on same line
            output.PrintLn("", logNum); // Now we've moved to a new line.
            output.PrintLn("Hello for a third time World!", logNum);
            output.PrintLn("Hello for a fourth time World!", logNum);
            output.Close();
        }

        [TestMethod]
        public void PrintLnToConsoleOut()
        {
            var output = new Output();
            var logNum = output.AddLog(Log.D_STDOUT, true);
            output.PrintLn("Hello World!", logNum);
            output.PrintLn("Hello Again World!", logNum); // Prints on same line
            output.PrintLn("", logNum); // Now we've moved to a new line.
            output.PrintLn("Hello for a third time World!", logNum);
            output.PrintLn("Hello for a fourth time World!", logNum);
            output.Close();
        }

        #endregion // PrintLn to System Logs
        #region InitialError to STDERR

        // Note that we use the overloads that take a boolean "exit" argument. This allows us to avoid crashing the test agents!

        [TestMethod]
        public void InitialErrorToStandardErrorStream()
        {
            Output.InitialError("Ooops!", false); // Do NOT exit because this causes test agent to crash!
        }

        [TestMethod]
        public void InitialErrorToStandardErrorStreamWithOneParameter()
        {
            Output.InitialError("Ooops!", new Parameter("BadParameter"), false); // Do NOT exit because this causes test agent to crash!
        }

        [TestMethod]
        public void InitialErrorToStandardErrorStreamWithTwoParameters()
        {
            Output.InitialError("Ooops!", new Parameter("FirstBadParameter"), new Parameter("SecondBadParameter"), false); // Do NOT exit because this causes test agent to crash!
        }

        #endregion // InitialError to STDERR
        #region InitialMessage to STDERR

        [TestMethod]
        public void InitialMessageToStandardErrorStream()
        {
            Output.InitialMessage("Welcome to ECCS!");
        }


        #endregion // InitialMessage to STDERR
        #region SystemMessage

        [TestMethod]
        [Description("This prints a system message to all registered logs.")]
        public void SystemMessageToStandardErrorAndOutputStreams()
        {
            var output = new Output();
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.SystemMessage("Welcome to ECCS!");
            Assert.AreEqual(output.NumAnnouncements, 2); // one for each registered log
            context.WriteLine("NumAnnouncements = {0}", output.NumAnnouncements);
            output.Close();
        }

        #endregion // SystemMessage
        #region Message to Registered System Logs

        [TestMethod]
        [Description("This prints a message to all registered logs. Currently the same as SystemMessage (because there is no verbosity).")]
        public void MessageToStandardErrorAndOutputStreams()
        {
            var output = new Output();
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Message("Message!");
            Assert.AreEqual(output.NumAnnouncements, 2); // one for each registered log
            context.WriteLine("NumAnnouncements = {0}", output.NumAnnouncements);
            output.Close();
        }

        #endregion // Message to Registered System Logs
        #region Fatal to Registered System Logs

        // Note that we use the overloads that take a boolean "exit" argument. This allows us to avoid crashing the test agents!

        [TestMethod]
        [Description("This writes a Fatal message to all registered logs.")]
        public void FatalToStandardErrorAndOutputStreams()
        {
            var output = new Output();
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Fatal("Yikes!", false); // Do NOT exit because this causes test agent to crash!
            Assert.AreEqual(output.NumAnnouncements, 2); // one for each registered log
            context.WriteLine("NumAnnouncements = {0}", output.NumAnnouncements);
            output.Close();
        }

        [TestMethod]
        [Description("This writes a Fatal message to two registered logs with One Bad Parameter argument. Note that this creates 4 announcements.")]
        public void FatalToStandardErrorAndOutputStreamsWithOneParameterToBlame()
        {
            var output = new Output();
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Fatal("Yikes!", new Parameter("BadParameter"), false); // Do NOT exit because this causes test agent to crash!
            Assert.AreEqual(output.NumAnnouncements, 4); // Note how many Announcements we end up with here!
            context.WriteLine("NumAnnouncements = {0}", output.NumAnnouncements);
            output.Close();
        }

        [TestMethod]
        [Description("This writes a Fatal message to two registered logs with Two Bad Parameter arguments. Note that this creates 6 announcements.")]
        public void FatalToStandardErrorAndOutputStreamsWithTwoParametersToBlame()
        {
            var output = new Output();
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Fatal("Yikes!", new Parameter("FirstBadParameter"), new Parameter("SecondBadParameter"), false); // Do NOT exit because this causes test agent to crash!
            Assert.AreEqual(output.NumAnnouncements, 6); // Note how many Announcements we end up with here!
            context.WriteLine("NumAnnouncements = {0}", output.NumAnnouncements);
            output.Close();
        }


        #endregion // Fatal to Registered System Logs
        #region Error to Registered System Logs

        [TestMethod]
        public void ErrorToConsoleErrorAndOutputLog()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Error("Ooops!");
            Assert.AreEqual(output.NumLogs, 2);
            output.Close();
        }

        [TestMethod]
        public void ErrorToConsoleErrorAndOutputLogWithOneParameterToBlame()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Error("Ooops!", new Parameter("BadParameter"));
            Assert.AreEqual(output.NumLogs, 2);
            output.Close();
        }

        [TestMethod]
        public void ErrorToConsoleErrorAndOutputLogWithTwoParametersToBlame()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Error("Ooops!", new Parameter("FirstBadParameter"), new Parameter("SecondBadParameter"));
            Assert.AreEqual(output.NumLogs, 2);
            output.Close();
        }

        #endregion Error to Registered System Logs
        #region Warning to Registered System Logs

        [TestMethod]
        public void WarningToConsoleErrorAndOutputLog()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Warning("Ooops!");
            Assert.AreEqual(output.NumLogs, 2);
            output.Close();
        }

        [TestMethod]
        public void WarningToConsoleErrorAndOutputLogWithOneParameterToBlame()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Warning("Ooops!", new Parameter("BadParameter"));
            Assert.AreEqual(output.NumLogs, 2);
            output.Close();
        }

        [TestMethod]
        public void WarningToConsoleErrorAndOutputLogWithTwoParametersToBlame()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.Warning("Ooops!", new Parameter("FirstBadParameter"), new Parameter("SecondBadParameter"));
            Assert.AreEqual(output.NumLogs, 2);
            output.Close();
        }

        #endregion // Warning to Registered System Logs
        #region WarnOnce to Registered System Logs

        [TestMethod]
        public void WarnOnceToConsoleErrorAndOutputLog()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.WarnOnce("Ooops!");
            output.WarnOnce("Ooops!");
            output.WarnOnce("Ooops!");
            Assert.AreEqual(output.NumLogs, 2);
            Assert.AreEqual(output.NumAnnouncements, 2);
            output.Close();
        }

        [TestMethod]
        public void WarnOnceToConsoleErrorAndOutputLogWithOneParameterToBlame()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.WarnOnce("Ooops!", new Parameter("BadParameter"));
            output.WarnOnce("Ooops!", new Parameter("BadParameter"));
            output.WarnOnce("Ooops!", new Parameter("BadParameter"));
            Assert.AreEqual(output.NumLogs, 2);
            Assert.AreEqual(output.NumAnnouncements, 4);
            output.Close();
        }

        [TestMethod]
        public void WarnOnceToConsoleErrorAndOutputLogWithTwoParametersToBlame()
        {
            var output = new Output();
            Assert.IsNotNull(output);
            output.AddLog(Log.D_STDERR, true);
            output.AddLog(Log.D_STDOUT, true);
            output.WarnOnce("Ooops!", new Parameter("FirstBadParameter"), new Parameter("SecondBadParameter"));
            output.WarnOnce("Ooops!", new Parameter("FirstBadParameter"), new Parameter("SecondBadParameter"));
            output.WarnOnce("Ooops!", new Parameter("FirstBadParameter"), new Parameter("SecondBadParameter"));
            Assert.AreEqual(output.NumLogs, 2);
            Assert.AreEqual(output.NumAnnouncements, 6);
            output.Close();
        }


        #endregion // WarnOnce to Registered System Logs

    }
}
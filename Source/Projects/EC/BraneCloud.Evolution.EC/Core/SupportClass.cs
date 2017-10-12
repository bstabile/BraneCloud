//
// In order to convert some functionality to Visual C#, the Java Language Conversion Assistant
// creates "support classes" that duplicate the original functionality.  
//
// Support classes replicate the functionality of the original code, but in some cases they are 
// substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that 
// the primary goal of these support classes is to replicate functionality, and that at times 
// the architecture of the resulting solution may differ somewhat.
//

using System;
using System.IO;
using System.Text;
using System.Data;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Drawing;

namespace BraneCloud.Evolution.EC
{
    /// <summary>
    /// This interface should be implemented by any class whose instances are intended 
    /// to be executed by a thread.
    /// </summary>
    public interface IThreadRunnable
    {
        /// <summary>
        /// This method has to be implemented in order that starting of the thread causes the object's 
        /// run method to be called in that separately executing thread.
        /// </summary>
        void Run();
    }

    /// <summary>
    /// Contains conversion support elements such as classes, interfaces and static methods.
    /// </summary>
    public class SupportClass
    {
        /// <summary>
        /// The class performs token processing in strings
        /// </summary>
        public class Tokenizer : IEnumerator
        {
            /// Position over the string
            private long _currentPos = 0;

            /// Include demiliters in the results.
            private bool _includeDelims = false;

            /// Char representation of the String to tokenize.
            private char[] _chars = null;

            //The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character and the form-feed character
            private string _delimiters = " \t\n\r\f";

            /// <summary>
            /// Initializes a new class instance with a specified string to process
            /// </summary>
            /// <param name="source">String to tokenize</param>
            public Tokenizer(string source)
            {
                _chars = source.ToCharArray();
            }

            /// <summary>
            /// Initializes a new class instance with a specified string to process
            /// and the specified token delimiters to use
            /// </summary>
            /// <param name="source">String to tokenize</param>
            /// <param name="delimiters">String containing the delimiters</param>
            public Tokenizer(string source, string delimiters)
                : this(source)
            {
                _delimiters = delimiters;
            }


            /// <summary>
            /// Initializes a new class instance with a specified string to process, the specified token 
            /// delimiters to use, and whether the delimiters must be included in the results.
            /// </summary>
            /// <param name="source">String to tokenize</param>
            /// <param name="delimiters">String containing the delimiters</param>
            /// <param name="includeDelims">Determines if delimiters are included in the results.</param>
            public Tokenizer(string source, string delimiters, bool includeDelims)
                : this(source, delimiters)
            {
                _includeDelims = includeDelims;
            }


            /// <summary>
            /// Returns the next token from the token list
            /// </summary>
            /// <returns>The string value of the token</returns>
            public string NextToken()
            {
                return NextToken(_delimiters);
            }

            /// <summary>
            /// Returns the next token from the source string, using the provided
            /// token delimiters
            /// </summary>
            /// <param name="delimiters">String containing the delimiters to use</param>
            /// <returns>The string value of the token</returns>
            public string NextToken(string delimiters)
            {
                //According to documentation, the usage of the received delimiters should be temporary (only for this call).
                //However, it seems it is not true, so the following line is necessary.
                this._delimiters = delimiters;

                //at the end 
                if (_currentPos == _chars.Length)
                    throw new ArgumentOutOfRangeException();

                //if over a delimiter and delimiters must be returned
                if ((Array.IndexOf(_delimiters.ToCharArray(), _chars[_currentPos]) != -1) && _includeDelims)
                    return "" + _chars[_currentPos++];

                //need to get the token wo delimiters.
                    return nextToken(_delimiters.ToCharArray());
            }

            //Returns the nextToken wo delimiters
            private string nextToken(char[] delimiters)
            {
                var token = "";
                var pos = _currentPos;

                //skip possible delimiters
                while (Array.IndexOf(delimiters, _chars[_currentPos]) != -1)
                    //The last one is a delimiter (i.e there is no more tokens)
                    if (++_currentPos == _chars.Length)
                    {
                        _currentPos = pos;
                        throw new ArgumentOutOfRangeException();
                    }

                //getting the token
                while (Array.IndexOf(delimiters, _chars[_currentPos]) == -1)
                {
                    token += _chars[_currentPos];
                    //the last one is not a delimiter
                    if (++_currentPos == _chars.Length)
                        break;
                }
                return token;
            }


            /// <summary>
            /// Determines if there are more tokens to return from the source string
            /// </summary>
            /// <returns>True or false, depending if there are more tokens</returns>
            public bool HasMoreTokens()
            {
                //keeping the current pos
                var pos = _currentPos;

                try
                {
                    NextToken();
                }
                catch (ArgumentOutOfRangeException)
                {
                    return false;
                }
                finally
                {
                    _currentPos = pos;
                }
                return true;
            }

            /// <summary>
            /// Remaining tokens count
            /// </summary>
            public int Count
            {
                get
                {
                    //keeping the current pos
                    var pos = _currentPos;
                    var i = 0;

                    try
                    {
                        while (true)
                        {
                            NextToken();
                            i++;
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        _currentPos = pos;
                        return i;
                    }
                }
            }

            /// <summary>
            ///  Performs the same action as NextToken.
            /// </summary>
            public object Current
            {
                get
                {
                    return NextToken();
                }
            }

            /// <summary>
            //  Performs the same action as HasMoreTokens.
            /// </summary>
            /// <returns>True or false, depending if there are more tokens</returns>
            public bool MoveNext()
            {
                return HasMoreTokens();
            }

            /// <summary>
            /// Does nothing.
            /// </summary>
            public void Reset()
            {
                ;
            }
        }
        /*******************************/
        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static long Identity(long literal)
        {
            return literal;
        }

        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static ulong Identity(ulong literal)
        {
            return literal;
        }

        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static float Identity(float literal)
        {
            return literal;
        }

        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static double Identity(double literal)
        {
            return literal;
        }

        /*******************************/
        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2 << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, int bits)
        {
            if (number >= 0)
                return number >> bits;
             
            return (number >> bits) + (2L << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /*******************************/
        /// <summary>
        /// SupportClass for the Stack class.
        /// </summary>
        public class StackSupport
        {
            /// <summary>
            /// Removes the element at the top of the stack and returns it.
            /// </summary>
            /// <param name="stack">The stack where the element at the top will be returned and removed.</param>
            /// <returns>The element at the top of the stack.</returns>
            public static object Pop(ArrayList stack)
            {
                var obj = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);

                return obj;
            }
        }


        /*******************************/
        /// <summary>
        /// Support class used to handle threads
        /// </summary>
        public class ThreadClass : IThreadRunnable
        {
            /// <summary>
            /// The instance of Threading.Thread
            /// </summary>
            private Thread threadField;

            /// <summary>
            /// Initializes a new instance of the ThreadClass class
            /// </summary>
            public ThreadClass()
            {
                threadField = new Thread(Run);
            }

            /// <summary>
            /// Initializes a new instance of the Thread class.
            /// </summary>
            /// <param name="Name">The name of the thread</param>
            public ThreadClass(string Name)
            {
                threadField = new Thread(Run);
                this.Name = Name;
            }

            /// <summary>
            /// Initializes a new instance of the Thread class.
            /// </summary>
            /// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
            public ThreadClass(ThreadStart Start)
            {
                threadField = new Thread(Start);
            }

            /// <summary>
            /// Initializes a new instance of the Thread class.
            /// </summary>
            /// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
            /// <param name="Name">The name of the thread</param>
            public ThreadClass(ThreadStart Start, string Name)
            {
                threadField = new Thread(Start);
                this.Name = Name;
            }

            /// <summary>
            /// This method has no functionality unless the method is overridden
            /// </summary>
            public virtual void Run()
            {
            }

            /// <summary>
            /// Causes the operating system to change the state of the current thread instance to ThreadState.Running
            /// </summary>
            public virtual void Start()
            {
                threadField.Start();
            }

            /// <summary>
            /// Interrupts a thread that is in the WaitSleepJoin thread state
            /// </summary>
            public virtual void Interrupt()
            {
                threadField.Interrupt();
            }

            /// <summary>
            /// Gets the current thread instance
            /// </summary>
            public Thread Instance
            {
                get
                {
                    return threadField;
                }
                set
                {
                    threadField = value;
                }
            }

            /// <summary>
            /// Gets or sets the name of the thread
            /// </summary>
            public string Name
            {
                get
                {
                    return threadField.Name;
                }
                set
                {
                    if (threadField.Name == null)
                        threadField.Name = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating the scheduling priority of a thread
            /// </summary>
            public ThreadPriority Priority
            {
                get
                {
                    return threadField.Priority;
                }
                set
                {
                    threadField.Priority = value;
                }
            }

            /// <summary>
            /// Gets a value indicating the execution status of the current thread
            /// </summary>
            public bool IsAlive
            {
                get
                {
                    return threadField.IsAlive;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether or not a thread is a background thread.
            /// </summary>
            public bool IsBackground
            {
                get
                {
                    return threadField.IsBackground;
                }
                set
                {
                    threadField.IsBackground = value;
                }
            }

            /// <summary>
            /// Blocks the calling thread until a thread terminates
            /// </summary>
            public void Join()
            {
                threadField.Join();
            }

            /// <summary>
            /// Blocks the calling thread until a thread terminates or the specified time elapses
            /// </summary>
            /// <param name="MiliSeconds">Time of wait in milliseconds</param>
            public void Join(long MiliSeconds)
            {
                lock (this)
                {
                    threadField.Join(new TimeSpan(MiliSeconds * 10000));
                }
            }

            /// <summary>
            /// Blocks the calling thread until a thread terminates or the specified time elapses
            /// </summary>
            /// <param name="MiliSeconds">Time of wait in milliseconds</param>
            /// <param name="NanoSeconds">Time of wait in nanoseconds</param>
            public void Join(long MiliSeconds, int NanoSeconds)
            {
                lock (this)
                {
                    threadField.Join(new TimeSpan(MiliSeconds * 10000 + NanoSeconds * 100));
                }
            }

            /// <summary>
            /// Resumes a thread that has been suspended
            /// BRS : TODO : Change this, it's been deprecated.
            /// </summary>
            public void Resume()
            {
                threadField.Resume();
            }

            /// <summary>
            /// Raises a ThreadAbortException in the thread on which it is invoked, 
            /// to begin the process of terminating the thread. Calling this method 
            /// usually terminates the thread
            /// </summary>
            public void Abort()
            {
                threadField.Abort();
            }

            /// <summary>
            /// Raises a ThreadAbortException in the thread on which it is invoked, 
            /// to begin the process of terminating the thread while also providing
            /// exception information about the thread termination. 
            /// Calling this method usually terminates the thread.
            /// </summary>
            /// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted</param>
            public void Abort(object stateInfo)
            {
                lock (this)
                {
                    threadField.Abort(stateInfo);
                }
            }

            /// <summary>
            /// Suspends the thread, if the thread is already suspended it has no effect
            /// BRS : TODO : Change this, it's been deprecated.
            /// </summary>
            public void Suspend()
            {
                threadField.Suspend();
            }

            /// <summary>
            /// Obtain a String that represents the current Object
            /// </summary>
            /// <returns>A String that represents the current Object</returns>
            public override string ToString()
            {
                return "Thread[" + Name + "," + Priority + "," + "" + "]";
            }

            /// <summary>
            /// Gets the currently running thread
            /// </summary>
            /// <returns>The currently running thread</returns>
            public static ThreadClass Current()
            {
                return new ThreadClass {Instance = Thread.CurrentThread};
            }
        }


        /*******************************/
        /// <summary>
        /// Shows a dialog object.
        /// </summary>
        /// <param name="dialog">Dialog to be shown.</param>
        /// <param name="visible">Indicates if the dialog should be shown.</param>
        public static void ShowDialog(System.Windows.Forms.FileDialog dialog, bool visible)
        {
            if (visible)
                dialog.ShowDialog();
        }


        /*******************************/
        /// <summary>
        /// Writes the exception stack trace to the received stream
        /// </summary>
        /// <param name="throwable">Exception to obtain information from</param>
        /// <param name="stream">Output sream used to write to</param>
        public static void WriteStackTrace(System.Exception throwable, TextWriter stream)
        {
            stream.Write(throwable.StackTrace);
            stream.Flush();
        }

        /*******************************/
        /// <summary>
        /// Class used to store and retrieve an object command specified as a String.
        /// </summary>
        public class CommandManager
        {
            /// <summary>
            /// Private Hashtable used to store objects and their commands.
            /// </summary>
            private static readonly Hashtable Commands = new Hashtable();

            /// <summary>
            /// Sets a command to the specified object.
            /// </summary>
            /// <param name="obj">The object that has the command.</param>
            /// <param name="cmd">The command for the object.</param>
            public static void SetCommand(object obj, string cmd)
            {
                if (obj == null) return;

                if (Commands.Contains(obj))
                    Commands[obj] = cmd;
                else
                    Commands.Add(obj, cmd);
            }

            /// <summary>
            /// Gets a command associated with an object.
            /// </summary>
            /// <param name="obj">The object whose command is going to be retrieved.</param>
            /// <returns>The command of the specified object.</returns>
            public static string GetCommand(object obj)
            {
                var result = "";
                if (obj != null)
                    result = Convert.ToString(Commands[obj]);
                return result;
            }



            /// <summary>
            /// Checks if the Control contains a command, if it does not it sets the default
            /// </summary>
            /// <param name="button">The control whose command will be checked</param>
            public static void CheckCommand(System.Windows.Forms.ButtonBase button)
            {
                if (button != null)
                {
                    if (GetCommand(button).Equals(""))
                        SetCommand(button, button.Text);
                }
            }

            /// <summary>
            /// Checks if the Control contains a command, if it does not it sets the default
            /// </summary>
            /// <param name="menuItem">The control whose command will be checked</param>
            public static void CheckCommand(MenuItem menuItem)
            {
                if (menuItem != null)
                {
                    if (GetCommand(menuItem).Equals(""))
                        SetCommand(menuItem, menuItem.Text);
                }
            }

            /// <summary>
            /// Checks if the Control contains a command, if it does not it sets the default
            /// </summary>
            /// <param name="comboBox">The control whose command will be checked</param>
            public static void CheckCommand(ComboBox comboBox)
            {
                if (comboBox != null)
                    if (GetCommand(comboBox).Equals(""))
                        SetCommand(comboBox, "comboBoxChanged");
            }

        }
        /*******************************/
        /// <summary>
        /// This class contains static methods to manage tab controls.
        /// </summary>
        public class TabControlSupport
        {
            /// <summary>
            /// Create a new instance of TabControl and set the alignment property.
            /// </summary>
            /// <param name="alignment">The alignment property value.</param>
            /// <returns>New TabControl instance.</returns>
            public static TabControl CreateTabControl(TabAlignment alignment)
            {
                return new TabControl {Alignment = alignment};
            }

            /// <summary>
            /// Set the alignment property to an instance of TabControl .
            /// </summary>
            /// <param name="tabcontrol">An instance of TabControl.</param>
            /// <param name="alignment">The alignment property value.</param>
            public static void SetTabControl(TabControl tabcontrol, TabAlignment alignment)
            {
                tabcontrol.Alignment = alignment;
            }

            /// <summary>
            /// Method to add TabPages into the TabControl object.
            /// </summary>
            /// <param name="tabControl">The TabControl to be modified.</param>
            /// <param name="component">A component to be added into the new TabControl.</param>
            public static Control AddTab(TabControl tabControl, Control component)
            {
                var tabPage = new TabPage();
                tabPage.Controls.Add(component);
                tabControl.TabPages.Add(tabPage);
                return component;
            }

            /// <summary>
            /// Method to add TabPages into the TabControl object.
            /// </summary>
            /// <param name="tabControl">The TabControl to be modified.</param>
            /// <param name="tabLabel">The label for the new TabPage.</param>
            /// <param name="component">A component to be added into the new TabControl.</param>
            public static Control AddTab(TabControl tabControl, string tabLabel, Control component)
            {
                var tabPage = new TabPage(tabLabel);
                tabPage.Controls.Add(component);
                tabControl.TabPages.Add(tabPage);
                return component;
            }

            /// <summary>
            /// Method to add TabPages into the TabControl object.
            /// </summary>
            /// <param name="tabControl">The TabControl to be modified.</param>
            /// <param name="component">A component to be added into the new TabControl.</param>
            /// <param name="constraints">The object that should be displayed in the tab but won't because of limitations</param>		
            public static void AddTab(TabControl tabControl, Control component, object constraints)
            {
                var tabPage = new TabPage();
                if (constraints is String)
                {
                    tabPage.Text = (String)constraints;
                }
                tabPage.Controls.Add(component);
                tabControl.TabPages.Add(tabPage);
            }

            /// <summary>
            /// Method to add TabPages into the TabControl object.
            /// </summary>
            /// <param name="tabControl">The TabControl to be modified.</param>
            /// <param name="tabLabel">The label for the new TabPage.</param>
            /// <param name="constraints">The object that should be displayed in the tab but won't because of limitations</param>
            /// <param name="component">A component to be added into the new TabControl.</param>
            public static void AddTab(System.Windows.Forms.TabControl tabControl, string tabLabel, object constraints, Control component)
            {
                var tabPage = new TabPage(tabLabel);
                tabPage.Controls.Add(component);
                tabControl.TabPages.Add(tabPage);
            }

            /// <summary>
            /// Method to add TabPages into the TabControl object.
            /// </summary>
            /// <param name="tabControl">The TabControl to be modified.</param>
            /// <param name="tabLabel">The label for the new TabPage.</param>
            /// <param name="image">Background image for the TabPage.</param>
            /// <param name="component">A component to be added into the new TabControl.</param>
            public static void AddTab(TabControl tabControl, string tabLabel, Image image, Control component)
            {
                var tabPage = new TabPage(tabLabel) {BackgroundImage = image};
                tabPage.Controls.Add(component);
                tabControl.TabPages.Add(tabPage);
            }
        }


        /*******************************/
        /// <summary>
        /// Action that will be executed when a toolbar button is clicked.
        /// </summary>
        /// <param name="event_sender">The object that fires the event.</param>
        /// <param name="event_args">An EventArgs that contains the event data.</param>
        public static void ToolBarButtonClicked(object event_sender, ToolBarButtonClickEventArgs event_args)
        {
            var button = (Button)event_args.Button.Tag;
            button.PerformClick();
        }


        /*******************************/
        /// <summary>
        /// Contains methods to get an set a ToolTip
        /// </summary>
        public class ToolTipSupport
        {
            static readonly ToolTip supportToolTip = new ToolTip();

            /// <summary>
            /// Get the ToolTip text for the specific control parameter.
            /// </summary>
            /// <param name="control">The control with the ToolTip</param>
            /// <returns>The ToolTip Text</returns>
            public static string GetToolTipText(System.Windows.Forms.Control control)
            {
                return (supportToolTip.GetToolTip(control));
            }

            /// <summary>
            /// Set the ToolTip text for the specific control parameter.
            /// </summary>
            /// <param name="control">The control to set the ToolTip</param>
            /// <param name="text">The text to show on the ToolTip</param>
            public static void SetToolTipText(Control control, string text)
            {
                supportToolTip.SetToolTip(control, text);
            }
        }

        /*******************************/
        /// <summary>
        /// Support Methods for FileDialog class. Note that several methods receive a DirectoryInfo object, but it won't be used in all cases.
        /// </summary>
        public class FileDialogSupport
        {
            /// <summary>
            /// Creates an OpenFileDialog open in a given path.
            /// </summary>
            /// <param name="path">Path to be opened by the OpenFileDialog.</param>
            /// <returns>A new instance of OpenFileDialog.</returns>
            public static OpenFileDialog CreateOpenFileDialog(System.IO.FileInfo path)
            {
                var temp_fileDialog = new OpenFileDialog();
                temp_fileDialog.InitialDirectory = path.Directory.FullName;
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates an OpenFileDialog open in a given path.
            /// </summary>
            /// <param name="path">Path to be opened by the OpenFileDialog.</param>
            /// <param name="directory">Directory to get the path from.</param>
            /// <returns>A new instance of OpenFileDialog.</returns>
            public static OpenFileDialog CreateOpenFileDialog(System.IO.FileInfo path, DirectoryInfo directory)
            {
                var temp_fileDialog = new OpenFileDialog {InitialDirectory = path.Directory.FullName};
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates a OpenFileDialog open in a given path.
            /// </summary>		
            /// <returns>A new instance of OpenFileDialog.</returns>
            public static OpenFileDialog CreateOpenFileDialog()
            {
                var temp_fileDialog = new OpenFileDialog();
                temp_fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates an OpenFileDialog open in a given path.
            /// </summary>
            /// <param name="path">Path to be opened by the OpenFileDialog</param>
            /// <returns>A new instance of OpenFileDialog.</returns>
            public static OpenFileDialog CreateOpenFileDialog(string path)
            {
                var temp_fileDialog = new OpenFileDialog {InitialDirectory = path};
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates an OpenFileDialog open in a given path.
            /// </summary>
            /// <param name="path">Path to be opened by the OpenFileDialog.</param>
            /// <param name="directory">Directory to get the path from.</param>
            /// <returns>A new instance of OpenFileDialog.</returns>
            public static OpenFileDialog CreateOpenFileDialog(string path, DirectoryInfo directory)
            {
                var temp_fileDialog = new OpenFileDialog {InitialDirectory = path};
                return temp_fileDialog;
            }

            /// <summary>
            /// Modifies an instance of OpenFileDialog, to open a given directory.
            /// </summary>
            /// <param name="fileDialog">OpenFileDialog instance to be modified.</param>
            /// <param name="path">Path to be opened by the OpenFileDialog.</param>
            /// <param name="directory">Directory to get the path from.</param>
            public static void SetOpenFileDialog(FileDialog fileDialog, string path, DirectoryInfo directory)
            {
                fileDialog.InitialDirectory = path;
            }

            /// <summary>
            /// Modifies an instance of OpenFileDialog, to open a given directory.
            /// </summary>
            /// <param name="fileDialog">OpenFileDialog instance to be modified.</param>
            /// <param name="path">Path to be opened by the OpenFileDialog</param>
            public static void SetOpenFileDialog(FileDialog fileDialog, FileInfo path)
            {
                fileDialog.InitialDirectory = path.Directory.FullName;
            }

            /// <summary>
            /// Modifies an instance of OpenFileDialog, to open a given directory.
            /// </summary>
            /// <param name="fileDialog">OpenFileDialog instance to be modified.</param>
            /// <param name="path">Path to be opened by the OpenFileDialog.</param>
            public static void SetOpenFileDialog(FileDialog fileDialog, string path)
            {
                fileDialog.InitialDirectory = path;
            }

            ///
            ///  Use the following static methods to create instances of SaveFileDialog.
            ///  By default, JFileChooser is converted as an OpenFileDialog, the following methods
            ///  are provided to create file dialogs to save files.
            ///	


            /// <summary>
            /// Creates a SaveFileDialog.
            /// </summary>		
            /// <returns>A new instance of SaveFileDialog.</returns>
            public static SaveFileDialog CreateSaveFileDialog()
            {
                var temp_fileDialog = 
                    new SaveFileDialog {InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)};
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates an SaveFileDialog open in a given path.
            /// </summary>
            /// <param name="path">Path to be opened by the SaveFileDialog.</param>
            /// <returns>A new instance of SaveFileDialog.</returns>
            public static SaveFileDialog CreateSaveFileDialog(System.IO.FileInfo path)
            {
                var temp_fileDialog = new SaveFileDialog {InitialDirectory = path.Directory.FullName};
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates an SaveFileDialog open in a given path.
            /// </summary>
            /// <param name="path">Path to be opened by the SaveFileDialog.</param>
            /// <param name="directory">Directory to get the path from.</param>
            /// <returns>A new instance of SaveFileDialog.</returns>
            public static SaveFileDialog CreateSaveFileDialog(System.IO.FileInfo path, DirectoryInfo directory)
            {
                var temp_fileDialog = new SaveFileDialog {InitialDirectory = path.Directory.FullName};
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates a SaveFileDialog open in a given path.
            /// </summary>
            /// <param name="directory">Directory to get the path from.</param>
            /// <returns>A new instance of SaveFileDialog.</returns>
            public static SaveFileDialog CreateSaveFileDialog(DirectoryInfo directory)
            {
                var temp_fileDialog = new SaveFileDialog {InitialDirectory = directory.FullName};
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates an SaveFileDialog open in a given path.
            /// </summary>
            /// <param name="path">Path to be opened by the SaveFileDialog</param>
            /// <returns>A new instance of SaveFileDialog.</returns>
            public static SaveFileDialog CreateSaveFileDialog(string path)
            {
                var temp_fileDialog = new SaveFileDialog {InitialDirectory = path};
                return temp_fileDialog;
            }

            /// <summary>
            /// Creates an SaveFileDialog open in a given path.
            /// </summary>
            /// <param name="path">Path to be opened by the SaveFileDialog.</param>
            /// <param name="directory">Directory to get the path from.</param>
            /// <returns>A new instance of SaveFileDialog.</returns>
            public static SaveFileDialog CreateSaveFileDialog(string path, DirectoryInfo directory)
            {
                var temp_fileDialog = new SaveFileDialog {InitialDirectory = path};
                return temp_fileDialog;
            }
        }
        /*******************************/
        public class FormSupport
        {
            /// <summary>
            /// Creates a Form instance and sets the property Text to the specified parameter.
            /// </summary>
            /// <param name="text">Value for the Form property Text</param>
            /// <returns>A new Form instance</returns>
            public static Form CreateForm(string text)
            {
                var tempForm = new Form {Text = text};
                return tempForm;
            }

            /// <summary>
            /// Creates a Form instance and sets the property Text to the specified parameter.
            /// Adds the received control to the Form's Controls collection in the position 0.
            /// </summary>
            /// <param name="text">Value for the Form Text property.</param>
            /// <param name="control">Control to be added to the Controls collection.</param>
            /// <returns>A new Form instance</returns>
            public static Form CreateForm(string text, Control control)
            {
                var tempForm = new Form {Text = text};
                tempForm.Controls.Add(control);
                tempForm.Controls.SetChildIndex(control, 0);
                return tempForm;
            }


            /// <summary>
            /// Creates a Form instance and sets the property Owner to the specified parameter.
            /// This also sets the FormBorderStyle and ShowInTaskbar properties.
            /// </summary>
            /// <param name="owner">Value for the Form property Owner</param>
            /// <returns>A new Form instance</returns>
            public static Form CreateForm(Form owner)
            {
                var tempForm = new Form { Owner = owner, FormBorderStyle = FormBorderStyle.None, ShowInTaskbar = false };
                return tempForm;
            }


            /// <summary>
            /// Creates a Form instance and sets the property Owner to the specified parameter.
            /// Sets the FormBorderStyle and ShowInTaskbar properties.
            /// Also add the received Control to the Form's Controls collection in the position 0.
            /// </summary>
            /// <param name="owner">Value for the Form property Owner</param>
            /// <param name="header">Control to be added to the Form's Controls collection</param>
            /// <returns>A new Form instance</returns>
            public static Form CreateForm(System.Windows.Forms.Form owner, Control header)
            {
                var tempForm = new Form
                    {
                        Owner = owner,
                        FormBorderStyle = FormBorderStyle.None,
                        ShowInTaskbar = false
                    };
                tempForm.Controls.Add(header);
                tempForm.Controls.SetChildIndex(header, 0);
                return tempForm;
            }

            /// <summary>
            /// Creates a Form instance and sets the FormBorderStyle property.
            /// </summary>
            /// <param name="title">The title of the Form</param>
            /// <param name="resizable">Boolean value indicating if the Form is or not resizable</param>
            /// <returns>A new Form instance</returns>
            public static Form CreateForm(string title, bool resizable)
            {
                var tempForm = new Form
                   {
                       Text = title,
                       TopLevel = false,
                       MaximizeBox = false,
                       MinimizeBox = false,
                       FormBorderStyle =
                           (resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle)
                   };
                return tempForm;
            }

            /// <summary>
            /// Creates a Form instance and sets the FormBorderStyle property.
            /// </summary>
            /// <param name="title">The title of the Form</param>
            /// <param name="resizable">Boolean value indicating if the Form is or not resizable</param>
            /// <param name="maximizable">Boolean value indicating if the Form displays the maximaze box</param>
            /// <returns>A new Form instance</returns>
            public static Form CreateForm(string title, bool resizable, bool maximizable)
            {
                var tempForm = new Form
                   {
                       Text = title,
                       TopLevel = false,
                       MaximizeBox = maximizable,
                       MinimizeBox = false,
                       FormBorderStyle = (resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle)
                   };

                return tempForm;
            }

            /// <summary>
            /// Creates a Form instance and sets the FormBorderStyle property.
            /// </summary>
            /// <param name="title">The title of the Form</param>
            /// <param name="resizable">Boolean value indicating if the Form is or not resizable</param>
            /// <param name="maximizable">Boolean value indicating if the Form displays the maximaze box</param>
            /// <param name="minimizable">Boolean value indicating if the Form displays the minimaze box</param>
            /// <returns>A new Form instance</returns>
            public static Form CreateForm(string title, bool resizable, bool maximizable, bool minimizable)
            {
                var tempForm = new Form
                   {
                       Text = title,
                       TopLevel = false,
                       MaximizeBox = maximizable,
                       MinimizeBox = minimizable,
                       FormBorderStyle = (resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle)
                   };
                return tempForm;
            }

            /// <summary>
            /// Receives a Form instance and sets the property Owner to the specified parameter.
            /// This also sets the FormBorderStyle and ShowInTaskbar properties.
            /// </summary>
            /// <param name="formInstance">Form instance to be set</param>
            /// <param name="owner">Value for the Form property Owner</param>
            public static void SetForm(Form formInstance, Form owner)
            {
                formInstance.Owner = owner;
                formInstance.FormBorderStyle = FormBorderStyle.None;
                formInstance.ShowInTaskbar = false;
            }

            /// <summary>
            /// Receives a Form instance, sets the Text property and adds a Control.
            /// The received Control is added in the position 0 of the Form's Controls collection.
            /// </summary>
            /// <param name="formInstance">Form instance to be set</param>
            /// <param name="text">Value to be set to the Text property.</param>
            /// <param name="control">Control to add to the Controls collection.</param>
            public static void SetForm(Form formInstance, string text, Control control)
            {
                formInstance.Text = text;
                formInstance.Controls.Add(control);
                formInstance.Controls.SetChildIndex(control, 0);
            }

            /// <summary>
            /// Receives a Form instance and sets the property Owner to the specified parameter.
            /// Sets the FormBorderStyle and ShowInTaskbar properties.
            /// Also adds the received Control to the Form's Controls collection in position 0.
            /// </summary>
            /// <param name="formInstance">Form instance to be set</param>
            /// <param name="owner">Value for the Form property Owner</param>
            /// <param name="header">The Control to be added to the Controls collection.</param>
            public static void SetForm(Form formInstance, Form owner, Control header)
            {
                formInstance.Owner = owner;
                formInstance.FormBorderStyle = FormBorderStyle.None;
                formInstance.ShowInTaskbar = false;
                formInstance.Controls.Add(header);
                formInstance.Controls.SetChildIndex(header, 0);
            }
        }
        /*******************************/
        /// <summary>
        /// Gets the current System properties.
        /// </summary>
        /// <returns>The current system properties.</returns>
        public static NameValueCollection GetProperties()
        {
            var properties = new NameValueCollection();
            var keys = new ArrayList(System.Environment.GetEnvironmentVariables().Keys);
            var values = new ArrayList(System.Environment.GetEnvironmentVariables().Values);
            for (var count = 0; count < keys.Count; count++)
                properties.Add(keys[count].ToString(), values[count].ToString());
            return properties;
        }


        /*******************************/
        /// <summary>
        /// Recieves a form and an integer value representing the operation to perform when the closing 
        /// event is fired.
        /// </summary>
        /// <param name="form">The form that fire the event.</param>
        /// <param name="operation">The operation to do while the form is closing.</param>
        public static void CloseOperation(System.Windows.Forms.Form form, int operation)
        {
            switch (operation)
            {
                case 0:
                    break;
                case 1:
                    form.Hide();
                    break;
                case 2:
                    form.Dispose();
                    break;
                case 3:
                    form.Dispose();
                    Application.Exit();
                    break;
            }
        }


        /*******************************/
        /// <summary>
        /// This class contains static methods to manage TreeViews.
        /// </summary>
        public class TreeSupport
        {
            /// <summary>
            /// Creates a new TreeView from the provided HashTable.
            /// </summary> 
            /// <param name="hashTable">HashTable</param>		
            /// <returns>Returns the created tree</returns>
            public static TreeView CreateTreeView(System.Collections.Hashtable hashTable)
            {
                return SetTreeView(new TreeView(), hashTable);
            }

            /// <summary>
            /// Sets a TreeView with data from the provided HashTable.
            /// </summary> 
            /// <param name="treeView">Tree.</param>
            /// <param name="hashTable">HashTable.</param>
            /// <returns>Returns the set tree.</returns>		
            public static TreeView SetTreeView(TreeView treeView, Hashtable hashTable)
            {
                foreach (DictionaryEntry myEntry in hashTable)
                {
                    var root = new TreeNode();

                    if (myEntry.Value is ArrayList)
                    {
                        root.Text = "[";
                        FillNode(root, (ArrayList)myEntry.Value);
                        root.Text = root.Text + "]";
                    }
                    else if (myEntry.Value is object[])
                    {
                        root.Text = "[";
                        FillNode(root, (object[])myEntry.Value);
                        root.Text = root.Text + "]";
                    }
                    else if (myEntry.Value is Hashtable)
                    {
                        root.Text = "[";
                        FillNode(root, (Hashtable)myEntry.Value);
                        root.Text = root.Text + "]";
                    }
                    else
                        root.Text = myEntry.ToString();

                    treeView.Nodes.Add(root);
                }
                return treeView;
            }


            /// <summary>
            /// Creates a new TreeView from the provided ArrayList.
            /// </summary> 
            /// <param name="arrayList">ArrayList.</param>		
            /// <returns>Returns the created tree.</returns>
            public static TreeView CreateTreeView(System.Collections.ArrayList arrayList)
            {
                var tree = new TreeView();
                return SetTreeView(tree, arrayList);
            }

            /// <summary>
            /// Sets a TreeView with data from the provided ArrayList.
            /// </summary> 
            /// <param name="treeView">Tree.</param>
            /// <param name="arrayList">ArrayList.</param>
            /// <returns>Returns the set tree.</returns>
            public static TreeView SetTreeView(TreeView treeView, ArrayList arrayList)
            {
                foreach (var arrayEntry in arrayList)
                {
                    var root = new TreeNode();

                    if (arrayEntry is ArrayList)
                    {
                        root.Text = "[";
                        FillNode(root, (ArrayList)arrayEntry);
                        root.Text = root.Text + "]";
                    }
                    else if (arrayEntry is Hashtable)
                    {
                        root.Text = "[";
                        FillNode(root, (Hashtable)arrayEntry);
                        root.Text = root.Text + "]";
                    }
                    else if (arrayEntry is object[])
                    {
                        root.Text = "[";
                        FillNode(root, (object[])arrayEntry);
                        root.Text = root.Text + "]";
                    }
                    else
                        root.Text = arrayEntry.ToString();


                    treeView.Nodes.Add(root);
                }
                return treeView;
            }

            /// <summary>
            /// Creates a new TreeView from the provided Object Array.
            /// </summary> 
            /// <param name="objectArray">Object Array.</param>		
            /// <returns>Returns the created tree.</returns>
            public static TreeView CreateTreeView(object[] objectArray)
            {
                return SetTreeView(new TreeView(), objectArray);
            }

            /// <summary>
            /// Sets a TreeView with data from the provided Object Array.
            /// </summary> 
            /// <param name="treeView">Tree.</param>
            /// <param name="objectArray">Object Array.</param>
            /// <returns>Returns the created tree.</returns>
            public static TreeView SetTreeView(TreeView treeView, object[] objectArray)
            {
                foreach (var arrayEntry in objectArray)
                {
                    var root = new TreeNode();

                    if (arrayEntry is ArrayList)
                    {
                        root.Text = "[";
                        FillNode(root, (ArrayList)arrayEntry);
                        root.Text = root.Text + "]";
                    }
                    else if (arrayEntry is Hashtable)
                    {
                        root.Text = "[";
                        FillNode(root, (Hashtable)arrayEntry);
                        root.Text = root.Text + "]";
                    }
                    else if (arrayEntry is object[])
                    {
                        root.Text = "[";
                        FillNode(root, (object[])arrayEntry);
                        root.Text = root.Text + "]";
                    }
                    else
                        root.Text = arrayEntry.ToString();

                    treeView.Nodes.Add(root);
                }
                return treeView;
            }

            /// <summary>
            /// Creates a new TreeView with the provided TreeNode as root.
            /// </summary> 
            /// <param name="root">Root.</param>		
            /// <returns>Returns the created tree.</returns>
            public static TreeView CreateTreeView(TreeNode root)
            {
                return SetTreeView(new TreeView(), root);
            }

            /// <summary>
            /// Sets a TreeView with the provided TreeNode as root.
            /// </summary>
            /// <param name="treeView">Tree</param>
            /// <param name="root">Root</param>
            /// <returns>Returns the created tree</returns>
            public static TreeView SetTreeView(TreeView treeView, TreeNode root)
            {
                if (root != null)
                    treeView.Nodes.Add(root);
                return treeView;
            }

            /// <summary>
            /// Sets a TreeView with the provided TreeNode as root.
            /// </summary> 
            /// <param name="tree">Tree</param>
            /// <param name="model">Root data model.</param>
            public static void SetModel(TreeView tree, TreeNode model)
            {
                tree.Nodes.Clear();
                tree.Nodes.Add(model);
            }

            /// <summary>
            /// Get the root TreeNode from a TreeView.
            /// </summary> 
            /// <param name="tree">Tree.</param>
            public static TreeNode GetModel(TreeView tree)
            {
                return tree.Nodes.Count > 0 ? tree.Nodes[0] : null;
            }

            /// <summary>
            /// Sets a TreeNode with data from the provided ArrayList instance.
            /// </summary> 
            /// <param name="treeNode">Node.</param>
            /// <param name="arrayList">ArrayList.</param>
            /// <returns>Returns the set node.</returns>
            public static TreeNode FillNode(System.Windows.Forms.TreeNode treeNode, ArrayList arrayList)
            {
                foreach (var arrayEntry in arrayList)
                {
                    var root = new TreeNode();

                    if (arrayEntry is ArrayList)
                    {
                        root.Text = "[";
                        FillNode(root, (ArrayList)arrayEntry);
                        root.Text = root.Text + "]";
                        treeNode.Nodes.Add(root);
                        treeNode.Text = treeNode.Text + ", " + root.Text;
                    }
                    else if (arrayEntry is object[])
                    {
                        root.Text = "[";
                        FillNode(root, (object[])arrayEntry);
                        root.Text = root.Text + "]";
                        treeNode.Nodes.Add(root);
                        treeNode.Text = treeNode.Text + ", " + root.Text;
                    }
                    else if (arrayEntry is Hashtable)
                    {
                        root.Text = "[";
                        FillNode(root, (Hashtable)arrayEntry);
                        root.Text = root.Text + "]";
                        treeNode.Nodes.Add(root);
                        treeNode.Text = treeNode.Text + ", " + root.Text;
                    }
                    else
                    {
                        treeNode.Nodes.Add(arrayEntry.ToString());
                        if (!(treeNode.Text.Equals("")))
                        {
                            if (treeNode.Text[treeNode.Text.Length - 1].Equals('['))
                                treeNode.Text = treeNode.Text + arrayEntry;
                            else
                                treeNode.Text = treeNode.Text + ", " + arrayEntry;
                        }
                        else
                            treeNode.Text = treeNode.Text + ", " + arrayEntry;
                    }
                }
                return treeNode;
            }


            /// <summary>
            /// Sets a TreeNode with data from the provided ArrayList.
            /// </summary> 
            /// <param name="treeNode">Node.</param>
            /// <param name="objectArray">Object Array.</param>
            /// <returns>Returns the set node.</returns>

            public static TreeNode FillNode(TreeNode treeNode, object[] objectArray)
            {
                foreach (var arrayEntry in objectArray)
                {
                    var root = new TreeNode();

                    if (arrayEntry is ArrayList)
                    {
                        root.Text = "[";
                        FillNode(root, (ArrayList)arrayEntry);
                        root.Text = root.Text + "]";
                    }
                    else if (arrayEntry is Hashtable)
                    {
                        root.Text = "[";
                        FillNode(root, (Hashtable)arrayEntry);
                        root.Text = root.Text + "]";
                    }
                    else
                    {
                        root.Nodes.Add(objectArray.ToString());
                        root.Text = root.Text + ", " + objectArray;
                    }
                    treeNode.Nodes.Add(root);
                    treeNode.Text = treeNode.Text + root.Text;
                }
                return treeNode;
            }

            /// <summary>		
            /// Sets a TreeNode with data from the provided Hashtable.		
            /// </summary> 		
            /// <param name="treeNode">Node.</param>		
            /// <param name="hashTable">Hash Table Object.</param>		
            /// <returns>Returns the set node.</returns>		
            public static TreeNode FillNode(TreeNode treeNode, Hashtable hashTable)
            {
                foreach (DictionaryEntry myEntry in hashTable)
                {
                    var root = new TreeNode();

                    if (myEntry.Value is ArrayList)
                    {
                        FillNode(root, (ArrayList)myEntry.Value);
                        treeNode.Nodes.Add(root);
                    }
                    else if (myEntry.Value is object[])
                    {
                        FillNode(root, (object[])myEntry.Value);
                        treeNode.Nodes.Add(root);
                    }
                    else
                        treeNode.Nodes.Add(myEntry.Key.ToString());
                }
                return treeNode;
            }
        }
        /*******************************/
        /// <summary>
        /// This class gives support for creation of management of DataTable.
        /// </summary>
        public class DataTableSupport
        {
            /// <summary>
            /// Creates a new DataTable with the specified number of columns and rows.
            /// </summary>
            /// <param name="rows">Number of rows.</param>
            /// <param name="columns">Number of columns.</param>
            /// <returns>A DataTable with the number of columns and rows specified, containing null values.</returns>
            public static DataTable CreateDataTable(int rows, int columns)
            {
                if ((rows < 0) || (columns < 0))
                    throw (new ArgumentException("Illegal Capacity " + rows + " or " + columns));

                var table = new DataTable();
                var emptyRow = new Object[columns];
                while (columns > table.Columns.Count)
                    table.Columns.Add();
                while (rows > table.Rows.Count)
                    table.Rows.Add(emptyRow);
                return table;
            }

            /// <summary>
            /// Sets a DataTable with a specific number of columns and rows.
            /// </summary>
            /// <param name="table">System.Data.DataTable instance to be set.</param>
            /// <param name="rows">Number of rows.</param>
            /// <param name="columns">Number of columns.</param>
            /// <returns>A DataTable with the number of columns and rows specified, containing null values.</returns>
            public static void SetDataTable(DataTable table, int rows, int columns)
            {
                if (table == null)
                {
                    throw new ArgumentException("Argument cannot be null.", "table");
                }
                if ((rows < 0) || (columns < 0))
                    throw (new ArgumentException("Illegal Capacity " + rows + " or " + columns));

                var emptyRow = new Object[columns];
                while (columns > table.Columns.Count)
                    table.Columns.Add();
                while (rows > table.Rows.Count)
                    table.Rows.Add(emptyRow);
            }

            /// <summary>
            /// Creates a DataTable with the specified column names and the specified amount of rows.
            /// </summary>
            /// <param name="columnNames">System.Collections.ArrayList containing the names of the columns to add to the DataTable.</param>
            /// <param name="rows">Number of rows.</param>
            /// <returns>A DataTable containing the rows and columns specified, containing null values.</returns>
            public static DataTable CreateDataTable(ArrayList columnNames, int rows)
            {
                return CreateDataTable(columnNames.ToArray(), rows);
            }

            /// <summary>
            /// Sets a DataTable with the specified column names and number of rows.
            /// </summary>
            /// <param name="table">System.Data.DataTable instance to be set.</param>
            /// <param name="columnNames">System.Collections.ArrayList containing the names of the columns to add to the DataTable.</param>
            /// <param name="rows">Number of rows.</param>
            /// <returns>A DataTable containing the rows and columns specified, containing null values.</returns>
            public static void SetDataTable(DataTable table, ArrayList columnNames, int rows)
            {
                SetDataTable(table, columnNames.ToArray(), rows);
            }

            /// <summary>
            /// Creates a DataTable with the specified column names and number of rows.
            /// </summary>
            /// <param name="columnNames">object array containing the names of the columns to add to the DataTable.</param>
            /// <param name="rows">Number of rows.</param>
            /// <returns>A DataTable containing the rows and columns specified, containing null values.</returns>
            public static DataTable CreateDataTable(object[] columnNames, int rows)
            {
                if (rows < 0)
                    throw (new ArgumentException("Illegal Capacity " + rows));

                var table = new DataTable();
                var emptyRow = new object[columnNames.Length];
                foreach (var columnName in columnNames)
                    table.Columns.Add((string)columnName);
                while (rows > table.Rows.Count)
                    table.Rows.Add(emptyRow);
                return table;
            }

            /// <summary>
            /// Sets a DataTable with the specified column names and number of rows.
            /// </summary>
            /// <param name="table">System.Data.DataTable instance to be set.</param>
            /// <param name="columnNames">object array containing the names of the columns to add to the DataTable.</param>
            /// <param name="rows">Number of rows.</param>
            /// <returns>A DataTable containing the rows and columns specified, containing null values.</returns>
            public static void SetDataTable(DataTable table, object[] columnNames, int rows)
            {
                if (table == null)
                    throw new ArgumentException("Argument cannot be null", "table");
                if (rows < 0)
                    throw (new ArgumentException("Illegal Capacity " + rows));

                var emptyRow = new object[columnNames.Length];
                foreach (var columnName in columnNames)
                    table.Columns.Add((string) columnName);
                while (rows > table.Rows.Count)
                    table.Rows.Add(emptyRow);
            }

            /// <summary>
            /// Creates a DataTable with the specified data, column names and number of rows.
            /// </summary>
            /// <param name="data">System.Collections.ArrayList containing the data to add to the DataTable.</param>
            /// <param name="columnNames">System.Collections.ArrayList containing the column names to add to the DataTable.</param>
            /// <returns>A DataTable containing the rows and columns specified, containing the specified values.</returns>
            public static DataTable CreateDataTable(ArrayList data, ArrayList columnNames)
            {
                var table = new DataTable();
                SetDataVector(table, data, columnNames);
                return table;
            }

            /// <summary>
            /// Sets a DataTable with the specified data, column names and number of rows.
            /// </summary>
            /// <param name="table">System.Data.DataTable instance to be set.</param>
            /// <param name="data">System.Collections.ArrayList containing the data to add to the DataTable.</param>
            /// <param name="columnNames">System.Collections.ArrayList containing the column names to add to the DataTable.</param>
            /// <returns>A DataTable containing the rows and columns specified, containing the specified values.</returns>
            public static void SetDataTable(DataTable table, ArrayList data, ArrayList columnNames)
            {
                if (table != null)
                    SetDataVector(table, data, columnNames);
            }

            /// <summary>
            /// Creates a DataTable with the specified data, column names and number of rows.
            /// </summary>
            /// <param name="data">object[][] containing the data to add to the DataTable, the first index is the data's row, and the second one its column.</param>
            /// <param name="columnNames">object[] containing the column names to add to the DataTable.</param>
            /// <returns>A DataTable containing the rows and columns specified, containing the specified values.</returns>
            public static DataTable CreateDataTable(object[][] data, object[] columnNames)
            {
                var table = new DataTable();
                SetDataVector(table, data, columnNames);
                return table;
            }

            /// <summary>
            /// Sets a DataTable with the specified data, column names and number of rows.
            /// </summary>
            /// <param name="table">System.Data.DataTable instance to set</param>
            /// <param name="data">object[][] containing the data to add to the DataTable, the first index is the data's row, and the second one its column.</param>
            /// <param name="columnNames">object[] containing the column names to add to the DataTable.</param>
            /// <returns>A DataTable containing the rows and columns specified, containing the specified values.</returns>
            public static void SetDataTable(DataTable table, object[][] data, object[] columnNames)
            {
                if (table != null)
                    SetDataVector(table, data, columnNames);
            }

            /// <summary>
            /// Sets the amount of rows to the specified value.
            /// </summary>
            /// <param name="table">The DataTable instance to modify.</param>
            /// <param name="rows">The new amount of rows for the DataTable.</param>
            public static void SetRowCount(DataTable table, int rows)
            {
                if (table == null)
                    throw new ArgumentException("Argument cannot be null", "table");

                if (rows < 0)
                    throw new ArgumentException("Illegal Capacity " + rows);

                if (rows > table.Rows.Count)
                {
                    var emptyRow = new Object[table.Columns.Count];
                    while (rows > table.Rows.Count)
                        table.Rows.Add(emptyRow);
                }
                else if (rows < table.Rows.Count)
                    while (rows < table.Rows.Count)
                        table.Rows.RemoveAt(table.Rows.Count);
            }

            /// <summary>
            /// Sets the amount of columns to the specified value.
            /// </summary>
            /// <param name="table">The DataTable instance to modify.</param>
            /// <param name="columns">The new amount of columns for the DataTable.</param>
            public static void SetColumnCount(DataTable table, int columns)
            {
                if (table == null)
                    throw new ArgumentException("Argument cannot be null", "table");
                if (columns < 0)
                    throw new ArgumentException("Illegal Capacity " + columns);

                if (columns > table.Columns.Count)
                    while (columns > table.Columns.Count)
                        table.Columns.Add();
                else if (columns < table.Columns.Count)
                    while (columns < table.Columns.Count)
                        table.Columns.RemoveAt(table.Columns.Count);
            }

            /// <summary>
            /// Sets the column's names to the specified values.
            /// </summary>
            /// <param name="table">The DataTable instance to be modified.</param>
            /// <param name="newIdentifiers">A object[] containing the values that should be applied to column names.</param>
            public static void SetColumnIdentifiers(DataTable table, object[] newIdentifiers)
            {
                if (table == null) return;

                var columns = newIdentifiers.Length;

                if (columns > table.Columns.Count)
                    while (columns > table.Columns.Count)
                        table.Columns.Add();
                else if (columns < table.Columns.Count)
                    while (columns < table.Columns.Count)
                        table.Columns.RemoveAt(table.Columns.Count);
                for (var index = 0; index < table.Columns.Count; index++)
                    table.Columns[index].ColumnName = newIdentifiers[index] as string;
            }

            /// <summary>
            /// Sets the column's names to the specified values.
            /// </summary>
            /// <param name="table">The DataTable instance to be modified.</param>
            /// <param name="newIdentifiers">A ArrayList containing the values that should be applied to column names.</param>
            public static void SetColumnIdentifiers(DataTable table, ArrayList newIdentifiers)
            {
                SetColumnIdentifiers(table, newIdentifiers.ToArray());
            }

            /// <summary>
            /// Sets the specified data to the corresponding columns in the table specified.
            /// </summary>
            /// <param name="table">The DataTable instance to be modified.</param>
            /// <param name="newData">object[][] containing the data to add to the DataTable, the first index is the data's row, and the second one its column.</param>
            /// <param name="columnNames">A object[] containing the names of the columns.</param>
            public static void SetDataVector(DataTable table, object[][] newData, object[] columnNames)
            {
                if (table == null) return;

                var rows = newData.Length;
                SetRowCount(table, rows);
                SetColumnIdentifiers(table, columnNames);
                for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                    for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                        table.Rows[rowIndex][columnIndex] = newData[rowIndex][columnIndex];
            }

            /// <summary>
            /// Sets the specified data to the corresponding columns in the table specified.
            /// </summary>
            /// <param name="table">The DataTable instance to be modified.</param>
            /// <param name="newData">System.Collections.ArrayList containing the data to add to the DataTable.</param>
            /// <param name="columnNames">A ArrayList containing the names of the columns.</param>
            public static void SetDataVector(System.Data.DataTable table, ArrayList newData, ArrayList columnNames)
            {
                SetColumnIdentifiers(table, columnNames);
                var data = new Object[newData.Count][];
                for (var index = 0; index < newData.Count; index++)
                {
                    data[index] = new Object[table.Columns.Count];
                    ((ArrayList)newData[index]).CopyTo(data[index]);
                }
                SetDataVector(table, data, columnNames.ToArray());
            }
        }


        /*******************************/
        /// <summary>
        /// Represents a collection ob objects that contains no duplicate elements.
        /// </summary>	
        public interface SetSupport : IList
        {
            /// <summary>
            /// Adds a new element to the Collection if it is not already present.
            /// </summary>
            /// <param name="obj">The object to add to the collection.</param>
            /// <returns>Returns true if the object was added to the collection, otherwise false.</returns>
            new bool Add(object obj);

            /// <summary>
            /// Adds all the elements of the specified collection to the Set.
            /// </summary>
            /// <param name="c">Collection of objects to add.</param>
            /// <returns>true</returns>
            bool AddAll(ICollection c);
        }


        /*******************************/
        /// <summary>
        /// This class provides functionality not found in .NET collection-related interfaces.
        /// </summary>
        public class ICollectionSupport
        {
            /// <summary>
            /// Adds a new element to the specified collection.
            /// </summary>
            /// <param name="c">Collection where the new element will be added.</param>
            /// <param name="obj">Object to add.</param>
            /// <returns>true</returns>
            public static bool Add(System.Collections.ICollection c, object obj)
            {
                var added = false;

                //Reflection. Invoke either the "add" or "Add" method.
                MethodInfo method;
                
                try
                {
                    //Get the "add" method for proprietary classes
                    method = c.GetType().GetMethod("Add") ?? c.GetType().GetMethod("add");

                    var index = (int)method.Invoke(c, new[] { obj });
                    if (index >= 0)
                        added = true;
                }
                catch (Exception e)
                {
                    throw e;
                }
                return added;
            }

            /// <summary>
            /// Adds all of the elements of the "c" collection to the "target" collection.
            /// </summary>
            /// <param name="target">Collection where the new elements will be added.</param>
            /// <param name="c">Collection whose elements will be added.</param>
            /// <returns>Returns true if at least one element was added, false otherwise.</returns>
            public static bool AddAll(System.Collections.ICollection target, ICollection c)
            {
                var e = new ArrayList(c).GetEnumerator();
                var added = false;

                //Reflection. Invoke "addAll" method for proprietary classes
                try
                {
                    var method = target.GetType().GetMethod("addAll");

                    if (method != null)
                        added = (bool)method.Invoke(target, new object[] { c });
                    else
                    {
                        method = target.GetType().GetMethod("Add");
                        while (e.MoveNext())
                        {
                            var tempBAdded = (int)method.Invoke(target, new[] { e.Current }) >= 0;
                            added = added ? added : tempBAdded; // hmmm...
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return added;
            }

            /// <summary>
            /// Removes all the elements from the collection.
            /// </summary>
            /// <param name="c">The collection to remove elements.</param>
            public static void Clear(ICollection c)
            {
                //Reflection. Invoke "Clear" method or "clear" method for proprietary classes
                try
                {
                    var method = c.GetType().GetMethod("Clear") ?? c.GetType().GetMethod("clear");

                    method.Invoke(c, new object[] { });
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            /// <summary>
            /// Determines whether the collection contains the specified element.
            /// </summary>
            /// <param name="c">The collection to check.</param>
            /// <param name="obj">The object to locate in the collection.</param>
            /// <returns>true if the element is in the collection.</returns>
            public static bool Contains(System.Collections.ICollection c, object obj)
            {
                var contains = false;

                //Reflection. Invoke "contains" method for proprietary classes
                try
                {
                    var method = c.GetType().GetMethod("Contains") ?? c.GetType().GetMethod("contains");

                    contains = (bool)method.Invoke(c, new[] { obj });
                }
                catch (Exception e)
                {
                    throw e;
                }

                return contains;
            }

            /// <summary>
            /// Determines whether the collection contains all the elements in the specified collection.
            /// </summary>
            /// <param name="target">The collection to check.</param>
            /// <param name="c">Collection whose elements would be checked for containment.</param>
            /// <returns>true id the target collection contains all the elements of the specified collection.</returns>
            public static bool ContainsAll(ICollection target, ICollection c)
            {
                var e = c.GetEnumerator();

                var contains = false;

                //Reflection. Invoke "containsAll" method for proprietary classes or "Contains" method for each element in the collection
                try
                {
                    var method = target.GetType().GetMethod("containsAll");

                    if (method != null)
                        contains = (bool) method.Invoke(target, new Object[] {c});
                    else
                    {
                        method = target.GetType().GetMethod("Contains");
                        while (e.MoveNext() == true)
                        {
                            if ((contains = (bool) method.Invoke(target, new[] {e.Current})) == false)
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return contains;
            }

            /// <summary>
            /// Removes the specified element from the collection.
            /// </summary>
            /// <param name="c">The collection where the element will be removed.</param>
            /// <param name="obj">The element to remove from the collection.</param>
            public static bool Remove(ICollection c, object obj)
            {
                var changed = false;

                //Reflection. Invoke "remove" method for proprietary classes or "Remove" method
                try
                {
                    var method = c.GetType().GetMethod("remove");

                    if (method != null)
                        method.Invoke(c, new object[] { obj });
                    else
                    {
                        method = c.GetType().GetMethod("Contains");
                        changed = (bool)method.Invoke(c, new object[] { obj });
                        method = c.GetType().GetMethod("Remove");
                        method.Invoke(c, new object[] { obj });
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

                return changed;
            }

            /// <summary>
            /// Removes all the elements from the specified collection that are contained in the target collection.
            /// </summary>
            /// <param name="target">Collection where the elements will be removed.</param>
            /// <param name="c">Elements to remove from the target collection.</param>
            /// <returns>true</returns>
            public static bool RemoveAll(System.Collections.ICollection target, ICollection c)
            {
                var al = ToArrayList(c);
                var e = al.GetEnumerator();

                //Reflection. Invoke "removeAll" method for proprietary classes or "Remove" for each element in the collection
                try
                {
                    var method = target.GetType().GetMethod("removeAll");

                    if (method != null)
                        method.Invoke(target, new object[] { al });
                    else
                    {
                        method = target.GetType().GetMethod("Remove");
                        var methodContains = target.GetType().GetMethod("Contains");

                        while (e.MoveNext())
                        {
                            while ((bool)methodContains.Invoke(target, new[] { e.Current }))
                                method.Invoke(target, new[] { e.Current });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return true;
            }

            /// <summary>
            /// Retains the elements in the target collection that are contained in the specified collection
            /// </summary>
            /// <param name="target">Collection where the elements will be removed.</param>
            /// <param name="c">Elements to be retained in the target collection.</param>
            /// <returns>true</returns>
            public static bool RetainAll(ICollection target, ICollection c)
            {
                var e = new ArrayList(target).GetEnumerator();
                var al = new ArrayList(c);

                //Reflection. Invoke "retainAll" method for proprietary classes or "Remove" for each element in the collection
                try
                {
                    var method = c.GetType().GetMethod("retainAll");

                    if (method != null)
                        method.Invoke(target, new object[] { c });
                    else
                    {
                        method = c.GetType().GetMethod("Remove");

                        while (e.MoveNext() == true)
                        {
                            if (al.Contains(e.Current) == false)
                                method.Invoke(target, new object[] { e.Current });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return true;
            }

            /// <summary>
            /// Returns an array containing all the elements of the collection.
            /// </summary>
            /// <returns>The array containing all the elements of the collection.</returns>
            public static object[] ToArray(ICollection c)
            {
                var index = 0;
                var objects = new Object[c.Count];
                var e = c.GetEnumerator();

                while (e.MoveNext())
                    objects[index++] = e.Current;

                return objects;
            }

            /// <summary>
            /// Obtains an array containing all the elements of the collection.
            /// </summary>
            /// <param name="c">The source collection</param>
            /// <param name="objects">The array into which the elements of the collection will be stored.</param>
            /// <returns>The array containing all the elements of the collection.</returns>
            public static object[] ToArray(ICollection c, object[] objects)
            {
                int index = 0;

                var type = objects.GetType().GetElementType();
                var objs = (object[])Array.CreateInstance(type, c.Count);

                var e = c.GetEnumerator();

                while (e.MoveNext())
                    objs[index++] = e.Current;

                //If objects is smaller than c then do not return the new array in the parameter
                if (objects.Length >= c.Count)
                    objs.CopyTo(objects, 0);

                return objs;
            }

            /// <summary>
            /// Converts an ICollection instance to an ArrayList instance.
            /// </summary>
            /// <param name="c">The ICollection instance to be converted.</param>
            /// <returns>An ArrayList instance in which its elements are the elements of the ICollection instance.</returns>
            public static ArrayList ToArrayList(ICollection c)
            {
                var tempArrayList = new ArrayList();
                var tempEnumerator = c.GetEnumerator();
                while (tempEnumerator.MoveNext())
                    tempArrayList.Add(tempEnumerator.Current);
                return tempArrayList;
            }
        }


        /*******************************/
        /// <summary>
        /// The SplitterPanel its a panel with two controls separated by a movable splitter.
        /// </summary>
        public class SplitterPanelSupport : Panel
        {
            private Control firstControl;
            private Control secondControl;
            private readonly Splitter splitter;
            private Orientation orientation;
            private int splitterSize;
            private int splitterLocation;
            private int lastSplitterLocation;

            //Default controls
            private readonly Control defaultFirstControl;
            private readonly Control defaultSecondControl;

            /// <summary>
            /// Creates a SplitterPanel with Horizontal orientation and two buttons as the default
            /// controls. The default size of the splitter is set to 5.
            /// </summary>
            public SplitterPanelSupport()
                : base()
            {
                var button1 = new Button();
                var button2 = new Button();
                button1.Text = "button1";
                button2.Text = "button2";

                lastSplitterLocation = -1;
                orientation = Orientation.Horizontal;
                splitterSize = 5;

                defaultFirstControl = button1;
                defaultSecondControl = button2;
                firstControl = defaultFirstControl;
                secondControl = defaultSecondControl;
                splitterLocation = firstControl.Size.Width;
                splitter = new Splitter();
                SuspendLayout();

                //
                // panel1
                //
                Controls.Add(splitter);
                Controls.Add(firstControl);
                Controls.Add(secondControl);

                // 
                // firstControl
                // 
                firstControl.Dock = DockStyle.Left;
                firstControl.Name = "firstControl";
                firstControl.TabIndex = 0;

                // 
                // secondControl
                //
                secondControl.Name = "secondControl";
                secondControl.TabIndex = 1;
                secondControl.Size = new Size((Size.Width - firstControl.Size.Width) + splitterSize, Size.Height);
                secondControl.Location = new Point((firstControl.Location.X + firstControl.Size.Width + splitterSize), 0);

                // 
                // splitter
                //			
                splitter.Name = "splitter";
                splitter.TabIndex = 2;
                splitter.TabStop = false;
                splitter.MinExtra = 10;
                splitter.BorderStyle = BorderStyle.FixedSingle;
                splitter.Size = new Size(splitterSize, Size.Height);
                splitter.SplitterMoved += splitter_SplitterMoved;

                SizeChanged += SplitterPanel_SizeChanged;
                ResumeLayout(false);
            }

            /// <summary>
            /// Creates a new SplitterPanelSupport with two buttons as default controls and the specified
            /// splitter orientation.
            /// </summary>
            /// <param name="newOrientation">The orientation of the SplitterPanel.</param>
            public SplitterPanelSupport(int newOrientation)
                : this()
            {
                SplitterOrientation = (Orientation)newOrientation;
            }

            /// <summary>
            /// Creates a new SplitterPanelSupport with the specified controls and orientation.
            /// </summary>
            /// <param name="newOrientation">The orientation of the SplitterPanel.</param>
            /// <param name="first">The first control of the panel, left-top control.</param>
            /// <param name="second">The second control of the panel, right-botton control.</param>
            public SplitterPanelSupport(int newOrientation, Control first, Control second)
                : this(newOrientation)
            {
                FirstControl = first;
                SecondControl = second;
            }


            /// <summary>
            /// Creates a new SplitterPanelSupport with the specified controls and orientation.
            /// </summary>		
            /// <param name="first">The first control of the panel, left-top control.</param>
            /// <param name="second">The second control of the panel, right-botton control.</param>
            public SplitterPanelSupport(Control first, Control second)
                : this()
            {
                FirstControl = first;
                SecondControl = second;
            }

            /// <summary>
            /// Adds a control to the SplitterPanel in the first available position.
            /// </summary>		
            /// <param name="control">The control to by added.</param>
            public void Add(Control control)
            {
                if (FirstControl == defaultFirstControl)
                    FirstControl = control;
                else if (SecondControl == defaultSecondControl)
                    SecondControl = control;
            }

            /// <summary>
            /// Adds a control to the SplitterPanel in the specified position.
            /// </summary>		
            /// <param name="control">The control to by added.</param>
            /// <param name="position">The position to add the control in the SpliterPanel.</param>
            public void Add(Control control, SpliterPosition position)
            {
                if (position == SpliterPosition.First)
                    FirstControl = control;
                else if (position == SpliterPosition.Second)
                    SecondControl = control;
            }

            /// <summary>
            /// Defines the possible positions of a control in a SpliterPanel.
            /// </summary>		
            public enum SpliterPosition
            {
                First,
                Second,
            }

            /// <summary>
            /// Gets the specified component.
            /// </summary>
            /// <param name="name">the name of the part of the component to get: "nw": first control, 
            /// "se": second control, "splitter": splitter.</param>
            /// <returns>returns the specified component.</returns>
            public virtual Control GetComponent(string name)
            {
                if (name == "nw")
                    return FirstControl;
                if (name == "se")
                    return SecondControl;
                return name == "splitter" ? splitter : null;
            }

            /// <summary>
            /// First control of the panel. When orientation is Horizontal then this is the left control
            /// when the orientation is Vertical then this is the top control.
            /// </summary>
            public virtual Control FirstControl
            {
                get
                {
                    return firstControl;
                }
                set
                {
                    Controls.Remove(firstControl);
                    value.Dock = orientation == Orientation.Horizontal ? DockStyle.Left : DockStyle.Top;
                    value.Size = firstControl.Size;
                    firstControl = value;
                    Controls.Add(firstControl);
                }
            }

            /// <summary>
            /// The second control of the panel. Right control when the panel is Horizontal oriented and
            /// botton control when the SplitterPanel orientation is Vertical.
            /// </summary>
            public virtual Control SecondControl
            {
                get
                {
                    return secondControl;
                }
                set
                {
                    Controls.Remove(secondControl);
                    value.Size = secondControl.Size;
                    value.Location = secondControl.Location;
                    secondControl = value;
                    Controls.Add(secondControl);
                }
            }

            /// <summary>
            /// The orientation of the SplitterPanel. Specifies how the controls are aligned.
            /// Left to right using Horizontal orientation or top to botton with Vertical orientation.
            /// </summary>
            public virtual Orientation SplitterOrientation
            {
                get
                {
                    return orientation;
                }
                set
                {
                    if (value == orientation) return;

                    orientation = value;
                    if (value == Orientation.Vertical)
                    {
                        var lastWidth = firstControl.Size.Width;
                        firstControl.Dock = DockStyle.Top;
                        firstControl.Size = new Size(Width, lastWidth);
                        splitter.Dock = DockStyle.Top;
                    }
                    else
                    {
                        var lastHeight = firstControl.Size.Height;
                        firstControl.Dock = DockStyle.Left;
                        firstControl.Size = new Size(lastHeight, Height);
                        splitter.Dock = DockStyle.Left;
                    }
                    ResizeSecondControl();
                }
            }

            /// <summary>
            /// Specifies the location of the Splitter in the panel.
            /// </summary>
            public virtual int SplitterLocation
            {
                get
                {
                    return splitterLocation;
                }
                set
                {
                    firstControl.Size = orientation == Orientation.Horizontal 
                        ? new Size(value, Height) 
                        : new Size(Width, value);

                    ResizeSecondControl();
                    lastSplitterLocation = splitterLocation;
                    splitterLocation = value;
                }
            }

            /// <summary>
            /// The last location of the splitter on the panel.
            /// </summary>
            public virtual int LastSplitterLocation
            {
                get
                {
                    return lastSplitterLocation;
                }
                set
                {
                    lastSplitterLocation = value;
                }
            }

            /// <summary>
            /// Specifies the size of the splitter divider.
            /// </summary>
            public virtual int SplitterSize
            {
                get
                {
                    return splitterSize;
                }
                set
                {
                    splitterSize = value;
                    splitter.Size = orientation == Orientation.Horizontal 
                        ? new Size(splitterSize, Size.Height) 
                        : new Size(Size.Width, splitterSize);

                    ResizeSecondControl();
                }
            }

            /// <summary>
            /// The minimum location of the splitter on the panel.
            /// </summary>
            /// <returns>The minimum location value for the splitter.</returns>
            public virtual int GetMinimumLocation()
            {
                return splitter.MinSize;
            }

            /// <summary>
            /// The maximum location of the splitter on the panel.
            /// </summary>
            /// <returns>The maximum location value for the splitter.</returns>
            public virtual int GetMaximumLocation()
            {
                if (orientation == Orientation.Horizontal)
                    return Width - (SplitterSize / 2);
                else
                    return Height - (SplitterSize / 2);
            }

            /// <summary>
            /// Adds a control to splitter panel.
            /// </summary>
            /// <param name="controlToAdd">The control to add.</param>
            /// <param name="dockStyle">The dock style for the control, left-top, or botton-right.</param>
            /// <param name="index">The index of the control in the panel control list.</param>
            protected virtual void AddControl(Control controlToAdd, object dockStyle, int index)
            {
                if (dockStyle is String)
                {
                    var dock = (string)dockStyle;
                    if (dock == "botton" || dock == "right")
                        SecondControl = controlToAdd;
                    else if (dock == "top" || dock == "left")
                        FirstControl = controlToAdd;
                    else
                        throw new ArgumentException("Cannot add control: unknown constraint: " + dockStyle);
                    Controls.SetChildIndex(controlToAdd, index);
                }
                else
                    throw new ArgumentException("Cannot add control: unknown constraint: " + dockStyle);
            }

            /// <summary>
            /// Removes the specified control from the panel.
            /// </summary>
            /// <param name="controlToRemove">The control to remove.</param>
            public virtual void RemoveControl(Control controlToRemove)
            {
                if (Contains(controlToRemove))
                {
                    Controls.Remove(controlToRemove);
                    if (firstControl == controlToRemove)
                        secondControl.Dock = DockStyle.Fill;
                    else
                        firstControl.Dock = DockStyle.Fill;
                }
            }

            /// <summary>
            /// Remove the control identified by the specified index.
            /// </summary>
            /// <param name="index">The index of the control to remove.</param>
            public virtual void RemoveControl(int index)
            {
                try
                {
                    Controls.RemoveAt(index);
                    if (firstControl != null)
                        if (Controls.Contains(firstControl))
                            firstControl.Dock = DockStyle.Fill;
                        else if (secondControl != null && (Controls.Contains(secondControl)))
                            secondControl.Dock = DockStyle.Fill;
                } // Compatibility with the conversion assistant.
                catch (ArgumentOutOfRangeException)
                {
                    throw new IndexOutOfRangeException("No such child: " + index);
                }
            }

            /// <summary>
            /// Changes the location of the splitter in the panel as a percentage of the panel's size.
            /// </summary>
            /// <param name="proportion">The percentage from 0.0 to 1.0.</param>
            public virtual void SetLocationProportional(double proportion)
            {
                if ((proportion > 0.0) && (proportion < 1.0))
                    SplitterLocation = (int)((orientation == Orientation.Horizontal) ? (proportion * Width) : (proportion * Height));
                else
                    throw new ArgumentException("Proportional location must be between 0.0 and 1.0");
            }

            private void splitter_SplitterMoved(object sender, SplitterEventArgs e)
            {
                lastSplitterLocation = splitterLocation;
                splitterLocation = orientation == Orientation.Horizontal ? firstControl.Width : firstControl.Height;
                ResizeSecondControl();
            }

            private void SplitterPanel_SizeChanged(object sender, EventArgs e)
            {
                ResizeSecondControl();
            }

            private void ResizeSecondControl()
            {
                if (orientation == Orientation.Horizontal)
                {
                    secondControl.Size = new Size((Width - (firstControl.Size.Width + splitterSize)), Size.Height);
                    secondControl.Location = new Point((firstControl.Size.Width + splitterSize), 0);
                }
                else
                {
                    secondControl.Size = new Size(Size.Width, (Size.Height - (firstControl.Size.Height + splitterSize)));
                    secondControl.Location = new Point(0, (firstControl.Size.Height + splitterSize));
                }
            }
        }


        /*******************************/
        /// <summary>
        /// Writes the serializable fields to the SerializationInfo object, which stores all the data needed to serialize the specified object object.
        /// </summary>
        /// <param name="info">SerializationInfo parameter from the GetObjectData method.</param>
        /// <param name="context">StreamingContext parameter from the GetObjectData method.</param>
        /// <param name="instance">Object to serialize.</param>
        public static void DefaultWriteObject(SerializationInfo info, StreamingContext context, object instance)
        {
            var thisType = instance.GetType();
            var mi = FormatterServices.GetSerializableMembers(thisType, context);
            for (var i = 0; i < mi.Length; i++)
            {
                info.AddValue(mi[i].Name, ((FieldInfo)mi[i]).GetValue(instance));
            }
        }


        /*******************************/
        /// <summary>
        /// Reads the serialized fields written by the DefaultWriteObject method.
        /// </summary>
        /// <param name="info">SerializationInfo parameter from the special deserialization constructor.</param>
        /// <param name="context">StreamingContext parameter from the special deserialization constructor</param>
        /// <param name="instance">Object to deserialize.</param>
        public static void DefaultReadObject(SerializationInfo info, StreamingContext context, object instance)
        {
            var thisType = instance.GetType();
            var mi = FormatterServices.GetSerializableMembers(thisType, context);
            for (var i = 0; i < mi.Length; i++)
            {
                var fi = (FieldInfo)mi[i];
                fi.SetValue(instance, info.GetValue(fi.Name, fi.FieldType));
            }
        }
        /*******************************/
        /// <summary>
        /// Writes an object to the specified Stream
        /// </summary>
        /// <param name="stream">The target Stream</param>
        /// <param name="objectToSend">The object to be sent</param>
        public static void Serialize(Stream stream, object objectToSend)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectToSend);
        }

        /// <summary>
        /// Writes an object to the specified BinaryWriter
        /// </summary>
        /// <param name="writer">The target BinaryWriter</param>
        /// <param name="objectToSend">The object to be sent</param>
        public static void Serialize(BinaryWriter writer, object objectToSend)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(writer.BaseStream, objectToSend);
        }

        /*******************************/
        /// <summary>
        /// Deserializes an object, or an entire graph of connected objects, and returns the object intance
        /// </summary>
        /// <param name="binaryReader">Reader instance used to read the object</param>
        /// <returns>The object instance</returns>
        public static object Deserialize(BinaryReader binaryReader)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(binaryReader.BaseStream);
        }

        /*******************************/
        /// <summary>
        /// Creates a new positive random number 
        /// </summary>
        /// <param name="random">The last random obtained</param>
        /// <returns>Returns a new positive random number</returns>
        public static long NextLong(System.Random random)
        {
            long temporaryLong = random.Next();
            temporaryLong = (temporaryLong << 32) + random.Next();
            if (random.Next(-1, 1) < 0)
                return -temporaryLong;
            else
                return temporaryLong;
        }
        /*******************************/
        /// <summary>
        /// Converts an array of sbytes to an array of bytes
        /// </summary>
        /// <param name="sbyteArray">The array of sbytes to be converted</param>
        /// <returns>The new array of bytes</returns>
        public static byte[] ToByteArray(sbyte[] sbyteArray)
        {
            byte[] byteArray = null;

            if (sbyteArray != null)
            {
                byteArray = new byte[sbyteArray.Length];
                for (var index = 0; index < sbyteArray.Length; index++)
                    byteArray[index] = (byte)sbyteArray[index];
            }
            return byteArray;
        }

        /// <summary>
        /// Converts a string to an array of bytes
        /// </summary>
        /// <param name="sourceString">The string to be converted</param>
        /// <returns>The new array of bytes</returns>
        public static byte[] ToByteArray(string sourceString)
        {
            return UTF8Encoding.UTF8.GetBytes(sourceString);
        }

        /// <summary>
        /// Converts a array of object-type instances to a byte-type array.
        /// </summary>
        /// <param name="tempObjectArray">Array to convert.</param>
        /// <returns>An array of byte type elements.</returns>
        public static byte[] ToByteArray(object[] tempObjectArray)
        {
            byte[] byteArray = null;
            if (tempObjectArray != null)
            {
                byteArray = new byte[tempObjectArray.Length];
                for (var index = 0; index < tempObjectArray.Length; index++)
                    byteArray[index] = (byte)tempObjectArray[index];
            }
            return byteArray;
        }

        /*******************************/
        /// <summary>
        /// SupportClass for the HashSet class.
        /// </summary>
        [Serializable]
        public class HashSetSupport : ArrayList, SetSupport
        {
            public HashSetSupport()
            {
            }

            public HashSetSupport(ICollection c)
            {
                AddAll(c);
            }

            public HashSetSupport(int capacity)
                : base(capacity)
            {
            }

            /// <summary>
            /// Adds a new element to the ArrayList if it is not already present.
            /// </summary>		
            /// <param name="obj">Element to insert to the ArrayList.</param>
            /// <returns>Returns true if the new element was inserted, false otherwise.</returns>
            new public virtual bool Add(object obj)
            {
                bool inserted;

                if ((inserted = Contains(obj)) == false)
                {
                    base.Add(obj);
                }

                return !inserted;
            }

            /// <summary>
            /// Adds all the elements of the specified collection that are not present to the list.
            /// </summary>
            /// <param name="c">Collection where the new elements will be added</param>
            /// <returns>Returns true if at least one element was added, false otherwise.</returns>
            public bool AddAll(ICollection c)
            {
                var e = new ArrayList(c).GetEnumerator();
                var added = false;

                while (e.MoveNext())
                {
                    if (Add(e.Current))
                        added = true;
                }

                return added;
            }

            /// <summary>
            /// Returns a copy of the HashSet instance.
            /// </summary>		
            /// <returns>Returns a shallow copy of the current HashSet.</returns>
            public override object Clone()
            {
                return MemberwiseClone();
            }
        }


        /*******************************/
        /// <summary>
        /// This class has static methods to manage collections.
        /// </summary>
        public class CollectionsSupport
        {
            /// <summary>
            /// Copies the IList to other IList.
            /// </summary>
            /// <param name="SourceList">IList source.</param>
            /// <param name="TargetList">IList target.</param>
            public static void Copy(IList SourceList, IList TargetList)
            {
                for (var i = 0; i < SourceList.Count; i++)
                    TargetList[i] = SourceList[i];
            }

            /// <summary>
            /// Replaces the elements of the specified list with the specified element.
            /// </summary>
            /// <param name="List">The list to be filled with the specified element.</param>
            /// <param name="Element">The element with which to fill the specified list.</param>
            public static void Fill(IList List, object Element)
            {
                for (var i = 0; i < List.Count; i++)
                    List[i] = Element;
            }

            /// <summary>
            /// This class implements IComparer and is used for Comparing two String objects by evaluating 
            /// the numeric values of the corresponding Char objects in each string.
            /// </summary>
            class CompareCharValues : IComparer
            {
                public int Compare(object x, object y)
                {
                    return String.CompareOrdinal((String)x, (String)y);
                }
            }

            /// <summary>
            /// Obtain the maximum element of the given collection with the specified comparator.
            /// </summary>
            /// <param name="Collection">Collection from which the maximum value will be obtained.</param>
            /// <param name="Comparator">The comparator with which to determine the maximum element.</param>
            /// <returns></returns>
            public static object Max(ICollection Collection, IComparer Comparator)
            {
                ArrayList tempArrayList;

                if (((ArrayList)Collection).IsReadOnly)
                    throw new NotSupportedException();

                if ((Comparator == null) || (Comparator is Comparer))
                {
                    try
                    {
                        tempArrayList = new ArrayList(Collection);
                        tempArrayList.Sort();
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new InvalidCastException(e.Message);
                    }
                    return tempArrayList[Collection.Count - 1];
                }
                else
                {
                    try
                    {
                        tempArrayList = new ArrayList(Collection);
                        tempArrayList.Sort(Comparator);
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new InvalidCastException(e.Message);
                    }
                    return tempArrayList[Collection.Count - 1];
                }
            }

            /// <summary>
            /// Obtain the minimum element of the given collection with the specified comparator.
            /// </summary>
            /// <param name="Collection">Collection from which the minimum value will be obtained.</param>
            /// <param name="Comparator">The comparator with which to determine the minimum element.</param>
            /// <returns></returns>
            public static object Min(ICollection Collection, IComparer Comparator)
            {
                ArrayList tempArrayList;

                if (((ArrayList)Collection).IsReadOnly)
                    throw new NotSupportedException();

                if ((Comparator == null) || (Comparator is Comparer))
                {
                    try
                    {
                        tempArrayList = new ArrayList(Collection);
                        tempArrayList.Sort();
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new InvalidCastException(e.Message);
                    }
                    return tempArrayList[0];
                }
                else
                {
                    try
                    {
                        tempArrayList = new ArrayList(Collection);
                        tempArrayList.Sort(Comparator);
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new InvalidCastException(e.Message);
                    }
                    return tempArrayList[0];
                }
            }


            /// <summary>
            /// Sorts an IList collections
            /// </summary>
            /// <param name="list">The IList instance that will be sorted</param>
            /// <param name="Comparator">The Comparator criteria, null to use natural comparator.</param>
            public static void Sort(IList list, IComparer Comparator)
            {
                if (((ArrayList)list).IsReadOnly)
                    throw new NotSupportedException();

                if ((Comparator == null) || (Comparator is Comparer))
                {
                    try
                    {
                        ((ArrayList)list).Sort();
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new InvalidCastException(e.Message);
                    }
                }
                else
                {
                    try
                    {
                        ((ArrayList)list).Sort(Comparator);
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new InvalidCastException(e.Message);
                    }
                }
            }

            /// <summary>
            /// Shuffles the list randomly.
            /// </summary>
            /// <param name="List">The list to be shuffled.</param>
            public static void Shuffle(IList List)
            {
                var RandomList = new Random(unchecked((int)DateTime.Now.Ticks));
                Shuffle(List, RandomList);
            }

            /// <summary>
            /// Shuffles the list randomly.
            /// </summary>
            /// <param name="List">The list to be shuffled.</param>
            /// <param name="RandomList">The random to use to shuffle the list.</param>
            public static void Shuffle(IList List, Random RandomList)
            {
                object source = null;
                int target = 0;

                for (var i = 0; i < List.Count; i++)
                {
                    target = RandomList.Next(List.Count);
                    source = List[i];
                    List[i] = List[target];
                    List[target] = source;
                }
            }
        }


        /*******************************/
        /// <summary>
        /// Converts the specified collection to its string representation.
        /// </summary>
        /// <param name="c">The collection to convert to string.</param>
        /// <returns>A string representation of the specified collection.</returns>
        public static string CollectionToString(System.Collections.ICollection c)
        {
            var s = new StringBuilder();

            if (c != null)
            {

                var l = new ArrayList(c);

                var isDictionary = (c is BitArray || c is Hashtable || c is IDictionary 
                    || c is NameValueCollection || (l.Count > 0 && l[0] is DictionaryEntry));

                for (var index = 0; index < l.Count; index++)
                {
                    if (l[index] == null)
                        s.Append("null");
                    else if (!isDictionary)
                        s.Append(l[index]);
                    else
                    {
                        if (c is NameValueCollection)
                            s.Append(((NameValueCollection)c).GetKey(index));
                        else
                            s.Append(((DictionaryEntry)l[index]).Key);
                        s.Append("=");
                        if (c is NameValueCollection)
                            s.Append(((NameValueCollection)c).GetValues(index)[0]);
                        else
                            s.Append(((DictionaryEntry)l[index]).Value);

                    }
                    if (index < l.Count - 1)
                        s.Append(", ");
                }

                if (isDictionary)
                {
                    if (c is ArrayList)
                        isDictionary = false;
                }
                if (isDictionary)
                {
                    s.Insert(0, "{");
                    s.Append("}");
                }
                else
                {
                    s.Insert(0, "[");
                    s.Append("]");
                }
            }
            else
                s.Insert(0, "null");
            return s.ToString();
        }

        /// <summary>
        /// Tests if the specified object is a collection and converts it to its string representation.
        /// </summary>
        /// <param name="obj">The object to convert to string</param>
        /// <returns>A string representation of the specified object.</returns>
        public static string CollectionToString(object obj)
        {
            string result = "";

            if (obj != null)
            {
                if (obj is ICollection)
                    result = CollectionToString((ICollection)obj);
                else
                    result = obj.ToString();
            }
            else
                result = "null";

            return result;
        }
    }
}
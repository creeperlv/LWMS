using CLUNL.Pipeline;
using LWMS.Core.Authentication;
using LWMS.Core.WR;
using System;
using System.Diagnostics;

namespace LWMS.Management
{
    /// <summary>
    /// Performs like System.Console. Recommend ManageCommands to write messages to here.
    /// </summary>
    public static class Output
    {
        static PipedRoutedConsoleLikeWR CoreStream;
        public static void SetCoreStream(string AuthContext,DefaultProcessor stream)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => { CoreStream.Processor = stream; }, false, true, PermissionID.RTApplyCmdProcessUnits, PermissionID.RuntimeAll);
        }
        static Output()
        {
            CoreStream = new PipedRoutedConsoleLikeWR();
        }
        /// <summary>
        /// Ask all process units to set their foreground color.
        /// </summary>
        /// <param name="color"></param>
        public static void SetForegroundColor(ConsoleColor color,string AuthContext)
        {
            CoreStream.SetForegroundColor(color, AuthContext);
        }
        /// <summary>
        /// Ask all process units to set their background color.
        /// </summary>
        /// <param name="color"></param>
        public static void SetBackgroundColor(ConsoleColor color, string AuthContext)
        {
            CoreStream.SetBackgroundColor(color, AuthContext);
        }
        /// <summary>
        /// Send a signal 'RESETCOLOR' to all process unit.
        /// </summary>
        public static void ResetColor(string AuthContext)
        {
            CoreStream.ResetColor(AuthContext);
        }
        /// <summary>
        /// Write message to PipedRoutedWR with a new line.
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(string str, string AuthContext)
        {
            CoreStream.WriteLine(str, AuthContext);
        }
        /// <summary>
        /// Write message to PipedRoutedWR.
        /// </summary>
        /// <param name="str"></param>
        public static void Write(string str, string AuthContext)
        {
            CoreStream.Write(str,AuthContext);
        }
    }
}

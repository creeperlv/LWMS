using CLUNL.Pipeline;
using LWMS.Core.Authentication;
using LWMS.Core.WR;
using System;

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
            OperatorAuthentication.AuthedAction(AuthContext, () => { CoreStream.Processor = stream; }, false, true, PermissionID.RTApplyCmdProcessUnits, PermissionID.RuntineAll);
        }
        static Output()
        {
            CoreStream = new PipedRoutedConsoleLikeWR();
        }
        /// <summary>
        /// Ask all process units to set their foreground color.
        /// </summary>
        /// <param name="color"></param>
        public static void SetForegroundColor(ConsoleColor color)
        {
            CoreStream.SetForegroundColor(color);
        }
        /// <summary>
        /// Ask all process units to set their background color.
        /// </summary>
        /// <param name="color"></param>
        public static void SetBackgroundColor(ConsoleColor color)
        {
            CoreStream.SetBackgroundColor(color);
        }
        /// <summary>
        /// Send a signal 'RESETCOLOR' to all process unit.
        /// </summary>
        public static void ResetColor()
        {
            CoreStream.ResetColor();
        }
        /// <summary>
        /// Write message to PipedRoutedWR with a new line.
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(string str)
        {
            CoreStream.WriteLine(str);
        }
        /// <summary>
        /// Write message to PipedRoutedWR.
        /// </summary>
        /// <param name="str"></param>
        public static void Write(string str)
        {
            CoreStream.Write(str);
        }
    }
}

using CLUNL;
using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using LWMS.Core.WR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LWMS.Management
{
    /// <summary>
    /// Abstract interface of IManageCommand will be used in ServerController.
    /// </summary>
    public interface IManageCommand
    {
        string CommandName { get; }
        List<string> Alias { get; }
        int Version { get; }
        void Invoke(params CommandPack[] args);
    }
    /// <summary>
    /// Performs like System.Console. Recommend ManageCommands to write messages to here.
    /// </summary>
    public static class Output
    {
        public static PipedRoutedConsoleLikeWR CoreStream;
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
    /// <summary>
    /// Command Pack. Contains result resolved from a command line or others. e.g.: -Option=1 => PackTotal = "-Option=1", PackParted= {"Option","1"}
    /// </summary>
    public class CommandPack
    {
        public string PackTotal;
        public List<string> PackParted = new List<string>();
        public static CommandPack FromRegexMatch(Match m)
        {
            CommandPack cp = new CommandPack();
            cp.PackTotal = m.Value.Trim();
            string _ = null;
            for (int i = 1; i < m.Groups.Count; i++)
            {
                if ((_ = m.Groups[i].Value.Trim()) != "")
                {
                    cp.PackParted.Add(_);
                }

            }
            return cp;
        }
        public static implicit operator string(CommandPack p)
        {
            return p.PackTotal;
        }
        public static implicit operator CommandPack(Match m)
        {
            return FromRegexMatch(m);
        }
        public string ToUpper()
        {
            return PackTotal.ToUpper();
        }
    }
}

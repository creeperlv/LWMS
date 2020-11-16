using CLUNL;
using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LWMS.Management
{
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
            
            cp.PackTotal = cp.PackTotal.Trim('\"');
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

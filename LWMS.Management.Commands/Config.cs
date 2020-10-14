using System;
using System.Diagnostics;

namespace LWMS.Management.Commands
{
    public class Config : IManageCommand
    {
        public string CommandName => "Config";

        public void Invoke(params CommandPack[] args)
        {
            Trace.WriteLine("Working");
        }
    }
}

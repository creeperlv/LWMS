using System.Collections.Generic;

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
        void Invoke(string AuthContext,params CommandPack[] args);
    }
}

using LWMS.Core.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LWMS.Core.FileSystem
{
    /// <summary>
    /// Represents a folder.
    /// </summary>
    public partial class StorageFolder : StorageItem
    {
        internal StorageFolder()
        {
            StorageItemType = StorageItemType.Folder;
        }
        /// <summary>
        /// Determines whether current folder is system root folder.
        /// </summary>
        public bool isSystemRoot
        {
            get
            {
                return (realPath == "{Root}" && parent == null);
            }
        }
        /// <summary>
        /// Determine whether current folder is a root folder.
        /// </summary>
        public bool isRoot
        {
            get
            {
                return isroot;
            }
        }
        /// <summary>
        /// Delete all items including files and folders.
        /// </summary>
        public virtual void DeleteAllItems(bool IgnoreDeletionError = false)
        {
            if (DeletePermissionID == null)
            {
                if (IgnoreDeletionError)
                {
                    foreach (var item in GetFolders())
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    foreach (var item in GetFiles())
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
                else
                {
                    foreach (var item in GetFolders())
                    {
                        item.Delete();
                    }
                    foreach (var item in GetFiles())
                    {
                        item.Delete();
                    }
                }
            }
            else
            {
                throw new UnauthorizedException(null, DeletePermissionID[0]);
            }
        }
        /// <summary>
        /// Delete all items including files and folders.
        /// </summary>
        /// <param name="Auth">In case this operation requires permission</param>
        public virtual void DeleteAllItems(string Auth, bool IgnoreDeletionError = false)
        {
            if (DeletePermissionID == null)
            {
                DeleteAllItems();
            }
            else
            {
                OperatorAuthentication.AuthedAction(Auth, () =>
                {
                    if (IgnoreDeletionError)
                    {
                        foreach (var item in GetFolders(Auth))
                        {
                            try
                            {
                                item.Delete(Auth);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        foreach (var item in GetFiles(Auth))
                        {
                            try
                            {
                                item.Delete(Auth);
                            }
                            catch (Exception)
                            {
                            }
                        }

                    }
                    else
                    {
                        foreach (var item in GetFolders(Auth))
                        {
                            item.Delete(Auth);
                        }
                        foreach (var item in GetFiles(Auth))
                        {
                            item.Delete(Auth);
                        }
                    }
                }, false, true, DeletePermissionID);
            }
        }
        /// <summary>
        /// Get a contained folder. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public virtual StorageFile GetFile(string Name, bool CaseSensitivity = false)
        {
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            StorageFile storageItem = new StorageFile();
            storageItem.DeletePermissionID = DeletePermissionID;
            storageItem.BaseWritePermission = BaseWritePermission;
            storageItem.BaseReadPermission = BaseReadPermission;
            var entries = Directory.EnumerateFiles(realPath);
            string Target = Path.Combine(realPath, Name);
            string TARGET = Target.ToUpper();
            foreach (var item in entries)
            {
                if (CaseSensitivity == false)
                {

                    if (item.ToUpper() == TARGET)
                    {
                        storageItem.SetPath(Target);
                        storageItem.Parent = this;
                        return storageItem;
                    }
                }
                else
                {

                    if (item == Target)
                    {
                        storageItem.SetPath(Target);
                        storageItem.Parent = this;
                        return storageItem;
                    }
                }
            }
            throw new StorageItemNotExistException(Path.Combine(realPath, Name));
        }
        /// <summary>
        /// Get a contained folder. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Auth"></param>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public virtual StorageFile GetFile(string Auth, string Name, bool CaseSensitivity = false)
        {
            if (BaseReadPermission is null) return GetFile(Name, CaseSensitivity);
            StorageFile storageItem = new StorageFile();

            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                storageItem.DeletePermissionID = DeletePermissionID;
                storageItem.BaseWritePermission = BaseWritePermission;
                storageItem.BaseReadPermission = BaseReadPermission;
                var entries = Directory.EnumerateFiles(realPath);
                string Target = Path.Combine(realPath, Name);
                string TARGET = Target.ToUpper();
                foreach (var item in entries)
                {
                    if (CaseSensitivity == false)
                    {

                        if (item.ToUpper() == TARGET)
                        {
                            storageItem.SetPath(Target);
                            storageItem.Parent = this;
                            return;
                        }
                    }
                    else
                    {

                        if (item == Target)
                        {
                            storageItem.SetPath(Target);
                            storageItem.Parent = this;
                            return;
                        }
                    }
                }
                storageItem = null;
            }, false, true, BaseReadPermission);
            if (storageItem is null)
                throw new StorageItemNotExistException(Path.Combine(realPath, Name));
            return storageItem;
        }
        /// <summary>
        /// Get a contained file. Return false when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <param name="CaseSensitivity"></param>
        /// <returns></returns>
        public virtual bool GetFile(string Name, out StorageFile OutItem, bool CaseSensitivity = false)
        {
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            StorageFile storageItem = new StorageFile();
            storageItem.DeletePermissionID = DeletePermissionID;
            storageItem.BaseWritePermission = BaseWritePermission;
            storageItem.BaseReadPermission = BaseReadPermission;
            var entries = Directory.EnumerateFiles(realPath);
            string Target = Path.Combine(realPath, Name);
            string TARGET = Target.ToUpper();
            foreach (var item in entries)
            {
                if (CaseSensitivity == false)
                {

                    if (item.ToUpper() == TARGET)
                    {
                        storageItem.SetPath(Target);
                        storageItem.Parent = this;
                        OutItem = storageItem;
                        return true;
                    }
                }
                else
                {

                    if (item == Target)
                    {
                        storageItem.SetPath(Target);
                        storageItem.Parent = this;
                        OutItem = storageItem;
                        return true;
                    }
                }
            }
            OutItem = null;
            return false;
        }
        /// <summary>
        /// Get a contained file. Return false when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <param name="CaseSensitivity"></param>
        /// <returns></returns>
        public virtual bool GetFile(string Auth, string Name, out StorageFile OutItem, bool CaseSensitivity = false)
        {
            if (BaseReadPermission is null)
            {
                return GetFile(Name, out OutItem, CaseSensitivity);
            }
            StorageFile storageItem = new StorageFile();
            bool v = false;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {

                storageItem.DeletePermissionID = DeletePermissionID;
                storageItem.BaseWritePermission = BaseWritePermission;
                storageItem.BaseReadPermission = BaseReadPermission;
                var entries = Directory.EnumerateFiles(realPath);
                string Target = Path.Combine(realPath, Name);
                string TARGET = Target.ToUpper();
                foreach (var item in entries)
                {
                    if (CaseSensitivity == false)
                    {

                        if (item.ToUpper() == TARGET)
                        {
                            storageItem.SetPath(Target);
                            storageItem.Parent = this;
                            v = true;
                            return;
                        }
                    }
                    else
                    {

                        if (item == Target)
                        {
                            storageItem.SetPath(Target);
                            storageItem.Parent = this;
                            v = true;
                            return;
                        }
                    }
                }
                storageItem = null;
            }, false, true, BaseReadPermission);
            OutItem = storageItem;
            return v;
        }
        /// <summary>
        /// Get a contained folder. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public virtual StorageFolder GetFolder(string Name, bool CaseSensitivity = false)
        {

            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            StorageFolder storageItem = new StorageFolder();
            storageItem.DeletePermissionID = DeletePermissionID;
            storageItem.BaseWritePermission = BaseWritePermission;
            storageItem.BaseReadPermission = BaseReadPermission;
            var F = GetFolder(Name, out storageItem, CaseSensitivity);
            if (F == false)
            {
                throw new StorageItemNotExistException(Path.Combine(realPath, Name));
            }
            return storageItem;
        }
        /// <summary>
        /// Get a contained folder. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Auth"></param>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public virtual StorageFolder GetFolder(string Auth, string Name, bool CaseSensitivity = false)
        {

            if (BaseReadPermission is null) return GetFolder(Name, CaseSensitivity);
            StorageFolder storageItem = new StorageFolder();
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                storageItem.DeletePermissionID = DeletePermissionID;
                storageItem.BaseWritePermission = BaseWritePermission;
                storageItem.BaseReadPermission = BaseReadPermission;
                var F = GetFolder(Auth, Name, out storageItem, CaseSensitivity);
                if (F == false)
                {
                    throw new StorageItemNotExistException(Path.Combine(realPath, Name));
                }
            }, false, true, BaseReadPermission);
            return storageItem;
        }
        /// <summary>
        /// Get a contained folder. Return false when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity"></param>
        /// <param name="OutFolder"></param>
        /// <returns></returns>
        public virtual bool GetFolder(string Name, out StorageFolder OutFolder, bool CaseSensitivity = false)
        {
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            if (isSystemRoot)
            {
                switch (Name)
                {
                    case PredefinedRootFolders.WebRoot:
                        OutFolder = ApplicationStorage.Webroot;
                        return true;
                    case PredefinedRootFolders.Configuration:
                        OutFolder = ApplicationStorage.Configuration;
                        return true;
                    case PredefinedRootFolders.Logs:
                        OutFolder = ApplicationStorage.Logs;
                        return true;
                    default:
                        OutFolder = null;
                        return false;
                }
            }
            StorageFolder storageItem = new StorageFolder();
            storageItem.DeletePermissionID = DeletePermissionID;
            storageItem.BaseWritePermission = BaseWritePermission;
            storageItem.BaseReadPermission = BaseReadPermission;
            var entries = Directory.EnumerateDirectories(realPath);
            string Target = Path.Combine(realPath, Name);
            string TARGET = Target.ToUpper();
            foreach (var item in entries)
            {
                if (CaseSensitivity == false)
                {

                    if (item.ToUpper() == TARGET)
                    {
                        storageItem.SetPath(Target);
                        storageItem.Parent = this;
                        OutFolder = storageItem;
                        return true;
                    }
                }
                else
                {

                    if (item == Target)
                    {
                        storageItem.SetPath(Target);
                        storageItem.Parent = this;
                        OutFolder = storageItem;
                        return true;
                    }
                }
            }
            OutFolder = null;
            return false;
        }
        /// <summary>
        /// Get a contained folder. Return false when cannot find it.
        /// </summary>
        /// <param name="Auth">Authentication Context</param>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity"></param>
        /// <param name="OutFolder"></param>
        /// <returns></returns>
        public virtual bool GetFolder(string Auth, string Name, out StorageFolder OutFolder, bool CaseSensitivity = false)
        {
            if (BaseReadPermission is null) return GetFolder(Name, out OutFolder, CaseSensitivity);
            StorageFolder storageItem = new StorageFolder();
            bool v = false;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                if (isSystemRoot)
                {
                    switch (Name)
                    {
                        case PredefinedRootFolders.WebRoot:
                            storageItem = ApplicationStorage.Webroot;
                            v = true;
                            return;
                        case PredefinedRootFolders.Configuration:
                            storageItem = ApplicationStorage.Configuration;
                            v = true;
                            return;
                        case PredefinedRootFolders.Logs:
                            storageItem = ApplicationStorage.Logs;
                            v = true;
                            return;
                        default:
                            storageItem = null;
                            v = false;
                            return;
                    }
                }
                storageItem.DeletePermissionID = DeletePermissionID;
                storageItem.BaseWritePermission = BaseWritePermission;
                storageItem.BaseReadPermission = BaseReadPermission;
                var entries = Directory.EnumerateDirectories(realPath);
                string Target = Path.Combine(realPath, Name);
                string TARGET = Target.ToUpper();
                foreach (var item in entries)
                {
                    if (CaseSensitivity == false)
                    {

                        if (item.ToUpper() == TARGET)
                        {
                            storageItem.SetPath(Target);
                            storageItem.Parent = this;
                            v = true;
                            return;
                        }
                    }
                    else
                    {

                        if (item == Target)
                        {
                            storageItem.SetPath(Target);
                            storageItem.Parent = this;
                            v = true;
                            return;
                        }
                    }
                }
                storageItem = null;
            }, false, true, BaseReadPermission);
            OutFolder = storageItem;
            return v;
        }
        /// <summary>
        /// Get all contained files in current folder.
        /// </summary>
        /// <returns></returns>
        public virtual List<StorageFile> GetFiles()
        {
            string[] FileNames = Directory.EnumerateFiles(realPath).ToArray();
            List<StorageFile> storageFiles = new List<StorageFile>(FileNames.Length);
            foreach (var item in FileNames)
            {
                storageFiles.Add(GetFile(item, true));
            }
            return storageFiles;
        }
        /// <summary>
        /// Get all contained files in current folder.
        /// </summary>
        /// <returns></returns>
        public virtual List<StorageFile> GetFiles(string Auth)
        {
            string[] FileNames = Directory.EnumerateFiles(realPath).ToArray();
            List<StorageFile> storageFiles = new List<StorageFile>(FileNames.Length);
            foreach (var item in FileNames)
            {
                storageFiles.Add(GetFile(Auth, item, true));
            }
            return storageFiles;
        }
        /// <summary>
        /// Get all contained folders in current folder.
        /// </summary>
        /// <returns></returns>
        public virtual List<StorageFolder> GetFolders()
        {
            string[] FileNames = Directory.EnumerateDirectories(realPath).ToArray();
            List<StorageFolder> storageFolders = new List<StorageFolder>(FileNames.Length);
            foreach (var item in FileNames)
            {
                storageFolders.Add(GetFolder(item, true));
            }
            return storageFolders;
        }
        /// <summary>
        /// Get all contained folders in current folder.
        /// </summary>
        /// <returns></returns>
        public virtual List<StorageFolder> GetFolders(string Auth)
        {
            string[] FileNames = Directory.EnumerateDirectories(realPath).ToArray();
            List<StorageFolder> storageFolders = new List<StorageFolder>(FileNames.Length);
            foreach (var item in FileNames)
            {
                storageFolders.Add(GetFolder(Auth,item, true));
            }
            return storageFolders;
        }
        /// <summary>
        /// Get all contained items in current folder.
        /// </summary>
        /// <returns></returns>
        public virtual List<StorageItem> GetItems()
        {
            string[] ItemNames = Directory.EnumerateFileSystemEntries(realPath).ToArray();
            List<StorageItem> storageItems = new List<StorageItem>(ItemNames.Length);
            foreach (var item in ItemNames)
            {
                storageItems.Add(GetItem(item, true));
            }
            return storageItems;
        }
        /// <summary>
        /// Get a contained item. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public virtual StorageItem GetItem(string Name, bool CaseSensitivity = false)
        {
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            StorageItem storageItem = new StorageItem();
            if (isSystemRoot)
            {
                return GetFolder(Name, CaseSensitivity);
            }
            var entries = Directory.EnumerateFileSystemEntries(realPath);
            string Target = Path.Combine(realPath, Name);
            string TARGET = Target.ToUpper();
            foreach (var item in entries)
            {
                if (CaseSensitivity == false)
                {

                    if (item.ToUpper() == TARGET)
                    {
                        storageItem.SetPath(Target);
                        if (Directory.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.Folder;
                        if (File.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.File;
                        storageItem.Parent = this;
                        if (isreadonlyR)
                        {
                            storageItem.isreadonly = true;
                            storageItem.isreadonlyR = true;
                        }
                        return storageItem;
                    }
                }
                else
                {

                    if (item == Target)
                    {
                        storageItem.SetPath(Target);
                        if (Directory.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.Folder;
                        if (File.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.File;
                        storageItem.Parent = this;
                        if (isreadonlyR)
                        {
                            storageItem.isreadonly = true;
                            storageItem.isreadonlyR = true;
                        }
                        return storageItem;
                    }
                }
            }
            throw new StorageItemNotExistException(Path.Combine(realPath, Name));
        }
        /// <summary>
        /// Get a contained item. Return false when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <param name="CaseSensitivity"></param>
        /// <returns></returns>
        public virtual bool GetItem(string Name, out StorageItem OutItem, bool CaseSensitivity = false)
        {
            StorageItem storageItem = new StorageItem();
            if (isSystemRoot)
            {
                var b = GetFolder(Name, out StorageFolder item, CaseSensitivity);
                OutItem = item;
                return b;
            }
            var entries = Directory.EnumerateFileSystemEntries(realPath);
            string Target = Path.Combine(realPath, Name);
            string TARGET = Target.ToUpper();
            foreach (var item in entries)
            {
                if (CaseSensitivity == false)
                {

                    if (item.ToUpper() == TARGET)
                    {
                        storageItem.SetPath(Target);
                        if (Directory.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.Folder;
                        if (File.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.File;
                        storageItem.Parent = this;
                        if (isreadonlyR)
                        {
                            storageItem.isreadonly = true;
                            storageItem.isreadonlyR = true;
                        }
                        OutItem = storageItem;
                        return true;
                    }
                }
                else
                {

                    if (item == Target)
                    {
                        storageItem.SetPath(Target);
                        if (Directory.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.Folder;
                        if (File.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.File;
                        storageItem.Parent = this;
                        if (isreadonlyR)
                        {
                            storageItem.isreadonly = true;
                            storageItem.isreadonlyR = true;
                        }
                        OutItem = storageItem;
                        return true;
                    }
                }
            }
            OutItem = null;
            return false;
        }
        /// <summary>
        /// Gets an item.
        /// </summary>
        /// <param name="L"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        public static StorageItem operator /(StorageFolder L, string R)
        {
            return L.GetItem(R);
        }
        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="L"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        public static bool operator -(StorageFolder L, string R)
        {
            var a = L / R;
            try
            {
                a.Delete();
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Creates a file.
        /// </summary>
        /// <param name="L"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        public static StorageFile operator +(StorageFolder L, string R)
        {
            StorageFile sf;
            _ = L.CreateFile(R, out sf);
            return sf;
        }
        /// <summary>
        /// Creates a folder.
        /// </summary>
        /// <param name="L"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        public static StorageFolder operator *(StorageFolder L, string R)
        {
            StorageFolder sf;
            _ = L.CreateFolder(R, out sf);
            return sf;
        }
    }
    public static class PredefinedRootFolders
    {
        public const string WebRoot = "/Webroot";
        public const string Configuration = "/Config";
        public const string Logs = "/Logs";
        public const string Miscellaneous = "/Miscellaneous";
        public const string ModuleStorage = "/ModuleStorage";
    }
    [Serializable]
    public class FindFileInRootFolderException : Exception
    {
        public FindFileInRootFolderException() { }
    }
}

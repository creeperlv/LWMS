using LWMS.Core.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWMS.Core.FileSystem
{
    public partial class StorageFolder : StorageItem
    {
        internal string[] CreateItemPermission = null;
        internal void SetCreateItemPermission(params string[] Permission)
        {
            CreateItemPermission = Permission;
        }
        /// <summary>
        /// Create a file in current folder.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// <exception cref="ItemAlreadyExistException"></exception>
        public StorageFile CreateFile(string Name)
        {
            if (CreateItemPermission is not null) throw new UnauthorizedException(null, CreateItemPermission[0]);
            if (isReadOnly)
            {
                throw new ItemReadOnlyException();
            }
            StorageFile storageFile;
            if (CreateFile(name, out storageFile) == false)
            {
                throw new ItemAlreadyExistException();
            }
            return storageFile;
        }
        public StorageFile CreateFile(string Auth, string Name)
        {
            if (CreateItemPermission is null) CreateFile(Name);
            StorageFile f = null;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                if (CreateItemPermission is not null) throw new UnauthorizedException(null, CreateItemPermission[0]);
                if (isReadOnly)
                {
                    throw new ItemReadOnlyException();
                }
                StorageFile storageFile;
                if (CreateFile(Auth, name, out storageFile) == false)
                {
                    throw new ItemAlreadyExistException();
                }
                f = storageFile;
            }, false, true, CreateItemPermission);
            return f;
        }
        /// <summary>
        /// Create a file in current folder, when the file is already exists, returns false.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <returns></returns>
        public bool CreateFile(string Name, out StorageFile OutItem)
        {
            if (CreateItemPermission is not null)
            {
                OutItem = null;
                return false;
            }
            if (isReadOnly)
            {
                OutItem = null;
                return false;
            }
            var path = Path.Combine(realPath, Name);
            bool result;
            if (File.Exists(path))
            {
                result = false;
            }
            else
            {
                File.Create(path).Close();
                result = true;
            }
            StorageFile storageFile = new StorageFile();
            storageFile.parent = this;
            storageFile.SetPath(path);
            OutItem = storageFile;

            return result;
        }
        /// <summary>
        /// Create a file in current folder, when the file is already exists, returns false.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <returns></returns>
        public bool CreateFile(string Auth, string Name, out StorageFile OutItem)
        {
            if (CreateItemPermission is null)
            {
                return CreateFile(Name, out OutItem);
            }
            bool result = false;
            StorageFile sf = null;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                if (isReadOnly)
                {
                    sf = null;
                    result = false;
                    return;
                }
                var path = Path.Combine(realPath, Name);
                if (File.Exists(path))
                {
                    result = false;
                }
                else
                {
                    File.Create(path).Close();
                    result = true;
                }
                StorageFile storageFile = new StorageFile();
                storageFile.parent = this;
                storageFile.SetPath(path);
                sf = storageFile;
            }, false, true, CreateItemPermission);
            OutItem = sf;
            return result;
        }
        /// <summary>
        /// Create a file in current folder, when the file is already exists or current folder is read-only, returns false.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <returns></returns>
        public bool CreateFolder(string Name, out StorageFolder OutItem)
        {
            if (CreateItemPermission is not null)
            {
                OutItem = null;
                return false;
            }
            if (isReadOnly)
            {
                OutItem = null;
                return false;
            }
            var path = Path.Combine(realPath, Name);
            bool result;
            if (Directory.Exists(path))
            {
                result = false;
            }
            else
            {
                Directory.CreateDirectory(path);
                result = true;
            }
            StorageFolder storageFile = new StorageFolder();
            storageFile.parent = this;
            storageFile.SetPath(path);
            OutItem = storageFile;

            return result;
        }
        /// <summary>
        /// Create a file in current folder, when the file is already exists or current folder is read-only, returns false.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <returns></returns>
        public bool CreateFolder(string Auth, string Name, out StorageFolder OutItem)
        {
            if (CreateItemPermission is null)
            {
                return CreateFolder(Name, out OutItem);
            }
            StorageFolder sf = null;
            bool result = false;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                if (isReadOnly)
                {
                    result = false;
                    return;
                }
                var path = Path.Combine(realPath, Name);
                if (Directory.Exists(path))
                {
                    result = false;
                }
                else
                {
                    Directory.CreateDirectory(path);
                    result = true;
                }
                sf = new StorageFolder();
                sf.parent = this;
                sf.SetPath(path);
            }, false, true, CreateItemPermission);
            OutItem = sf;

            return result;
        }
        /// <summary>
        /// Create a folder in current folder.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// <exception cref="ItemAlreadyExistException"></exception>
        public StorageFolder CreateFolder(string Name)
        {
            if (CreateItemPermission is not null) throw new UnauthorizedException(null, CreateItemPermission[0]);
            if (isReadOnly)
            {
                throw new ItemReadOnlyException();
            }
            StorageFolder storageFolder;
            if (CreateFolder(Name, out storageFolder) == false)
            {
                throw new ItemAlreadyExistException();
            }
            return storageFolder;
        }
        /// <summary>
        /// Create a folder in current folder.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// <exception cref="ItemAlreadyExistException"></exception>
        public StorageFolder CreateFolder(string Auth, string Name)
        {
            if (CreateItemPermission is null) CreateFolder(Name);
            StorageFolder storageFolder = null;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                if (isReadOnly)
                {
                    throw new ItemReadOnlyException();
                }
                if (CreateFolder(Auth, Name, out storageFolder) == false)
                {
                    throw new ItemAlreadyExistException();
                }
            }, false, true, CreateItemPermission);
            return storageFolder;
        }
        /// <summary>
        /// Create a folder in current folder.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="isIgnoreExistence"></param>
        /// <returns></returns>
        /// <exception cref="ItemAlreadyExistException"></exception>
        public StorageFolder CreateFolder(string Name, bool isIgnoreExistence)
        {
            if (CreateItemPermission is not null) throw new UnauthorizedException(null, CreateItemPermission[0]);
            if (isReadOnly)
            {
                throw new ItemReadOnlyException();
            }
            StorageFolder storageFolder;
            if (CreateFolder(Name, out storageFolder) == false)
            {
                if (isIgnoreExistence == true)
                    storageFolder = GetFolder(Name);
                else
                    throw new ItemAlreadyExistException();
            }
            return storageFolder;
        }
        /// <summary>
        /// Create a folder in current folder.
        /// </summary>
        /// <param name="Auth"></param>
        /// <param name="Name"></param>
        /// <param name="isIgnoreExistence"></param>
        /// <returns></returns>
        /// <exception cref="ItemAlreadyExistException"></exception>
        public StorageFolder CreateFolder(string Auth, string Name, bool isIgnoreExistence)
        {
            if (CreateItemPermission is null)
                if (CreateItemPermission is null) CreateFolder(Name, isIgnoreExistence);
            if (isReadOnly)
            {
                throw new ItemReadOnlyException();
            }
            StorageFolder storageFolder = null;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                if (CreateFolder(Name, out storageFolder) == false)
                {
                    if (isIgnoreExistence == true)
                        storageFolder = GetFolder(Name);
                    else
                        throw new ItemAlreadyExistException();
                }
            }, false, true, CreateItemPermission);
            return storageFolder;
        }
    }

    [Serializable]
    public class ItemReadOnlyException : Exception
    {
        public ItemReadOnlyException() : base("Target item is read-only!") { }
    }
    [Serializable]
    public class ItemAlreadyExistException : Exception
    {
        public ItemAlreadyExistException() { }
    }
}

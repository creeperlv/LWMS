using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWMS.Core.FileSystem
{
    public partial class StorageFolder : StorageItem
    {
        /// <summary>
        /// Create a file in current folder.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// <exception cref="ItemAlreadyExistException"></exception>
        public StorageFile CreateFile(string Name)
        {
            if (isReadOnly)
            {
                throw new ItemReadOnlyException();
            }
            StorageFile storageFile;
            if(CreateFile(name,out storageFile)==false)
            {
                throw new ItemAlreadyExistException();
            }
            return storageFile;
        }
        /// <summary>
        /// Create a file in current folder, when the file is already exists, returns false.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <returns></returns>
        public bool CreateFile(string Name, out StorageFile OutItem)
        {
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
        /// Create a file in current folder, when the file is already exists or current folder is read-only, returns false.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <returns></returns>
        public bool CreateFolder(string Name, out StorageFolder OutItem)
        {
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
        /// Create a folder in current folder.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// <exception cref="ItemAlreadyExistException"></exception>
        public StorageFolder CreateFolder(string Name)
        {

            if (isReadOnly)
            {
                throw new ItemReadOnlyException();
            }
            StorageFolder storageFolder;
            if(CreateFolder(Name, out storageFolder) == false)
            {
                throw new ItemAlreadyExistException();
            }
            return storageFolder;
        }
    }

    [Serializable]
    public class ItemReadOnlyException : Exception
    {
        public ItemReadOnlyException():base("Target item is read-only!") { }
    }
    [Serializable]
    public class ItemAlreadyExistException : Exception
    {
        public ItemAlreadyExistException() { }
    }
}

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
            var path = Path.Combine(realPath, Name);
            if (File.Exists(path))
            {
                throw new ItemAlreadyExistException();
            }
            FileInfo fileInfo = new FileInfo(path);
            fileInfo.Create().Close();
            StorageFile storageFile = new StorageFile();
            storageFile.parent = this;
            storageFile.SetPath(path);
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
            var path = Path.Combine(realPath, Name);
            bool result;
            if (File.Exists(path))
            {
                result = false;
            }
            else
            {
                FileInfo fileInfo = new FileInfo(path);
                fileInfo.Create().Close();
                result = true;
            }
            StorageFile storageFile = new StorageFile();
            storageFile.parent = this;
            storageFile.SetPath(path);
            OutItem = storageFile;

            return result;
        }
    }

    [Serializable]
    public class ItemAlreadyExistException : Exception
    {
        public ItemAlreadyExistException() { }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LWMS.Core.FileSystem
{

    [Serializable]
    public class StorageItemException : Exception
    {
        public StorageItemException(string path) : base(path) { }
    }
    public class StorageItem
    {
        internal StorageItem()
        {

        }
        internal string realPath;
        public string ItemPath { get => realPath; }
        public StorageItem Parent;
        public StorageItemType StorageItemType;
        public virtual void Delete()
        {
            if (File.Exists(realPath)) { File.Delete(realPath); return; }
            if (Directory.Exists(realPath)) Directory.Delete(realPath, true);
        }
        public virtual void CopyTo(StorageItem Destination)
        {
        }
    }
    public enum StorageItemType
    {
        File, Folder
    }
}

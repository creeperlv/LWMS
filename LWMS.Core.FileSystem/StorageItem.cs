using System;
using System.IO;

namespace LWMS.Core.FileSystem
{

    [Serializable]
    public class StorageItemNotExistException : Exception
    {
        public StorageItemNotExistException(string path) : base(path) { }
    }
    public class StorageItem
    {
        internal StorageItem()
        {

        }
        internal string realPath;
        internal DirectoryInfo RealDirectory;
        internal FileInfo RealFile;
        internal void SetPath(string path)
        {
            realPath = path;
            if (Directory.Exists(path))
            {
                StorageItemType = StorageItemType.Folder;
                RealDirectory = new DirectoryInfo(realPath);
                name = RealDirectory.Name;
            }
            if (File.Exists(path))
            {
                StorageItemType = StorageItemType.File;
                RealFile = new FileInfo(realPath);
                name = RealFile.Name;
            }

        }
        public string ItemPath { get => realPath; }
        internal StorageFolder parent;
        internal string name;
        public string Name
        {
            get { return name; }
            set
            {
                switch (StorageItemType)
                {
                    case StorageItemType.File:
                        {
                            File.Move(realPath, Path.Combine(parent.realPath, value));
                            name = value;
                        }
                        break;
                    case StorageItemType.Folder:
                        {
                            Directory.Move(realPath, Path.Combine(parent.realPath, value));
                            name = value;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        public StorageFolder Parent
        {
            get { return parent; }
            set
            {
                if (this != ApplicationStorage.SystemRoot)
                {
                    parent = value;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        public StorageItemType StorageItemType;
        /// <summary>
        /// Delete a file.
        /// </summary>
        public virtual void Delete()
        {
            if (File.Exists(realPath)) { File.Delete(realPath); return; }
            if (Directory.Exists(realPath)) Directory.Delete(realPath, true);
        }
        public StorageFile ToStorageFile()
        {
            if (StorageItemType == StorageItemType.File)
            {

                StorageFile storageFile = new StorageFile();
                storageFile.Parent = Parent;
                storageFile.SetPath(realPath);
                return storageFile;
            }
            else throw new NotSupportedException();
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

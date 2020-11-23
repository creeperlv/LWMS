using System;
using System.IO;

namespace LWMS.Core.FileSystem
{

    [Serializable]
    public class StorageItemNotExistException : Exception
    {
        public StorageItemNotExistException(string path) : base(path) { }
    }
    /// <summary>
    /// Represnt an item in file system it can be file or a folder.
    /// </summary>
    public class StorageItem
    {
        internal StorageItem()
        {

        }
        internal string realPath;
        internal DirectoryInfo RealDirectory;
        internal FileInfo RealFile;
        internal bool isroot;
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
        /// <summary>
        /// Gets the real path of the item.
        /// </summary>
        public string ItemPath { get => realPath; }
        internal StorageFolder parent;
        internal string name;
        internal bool isvirtual=false;
        /// <summary>
        /// Gets whether the item is virtual item.
        /// </summary>
        public bool isVirtual { get => isvirtual; }
        public string Name
        {
            get { return name; }
            set
            {
                if (isroot) throw new NotSupportedException();
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
        public StorageItemType StorageItemType { get; internal set; }
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

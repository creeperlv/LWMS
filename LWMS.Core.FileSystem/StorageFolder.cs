using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LWMS.Core.FileSystem
{
    public class StorageFolder : StorageItem
    {
        internal StorageFolder()
        {
            StorageItemType = StorageItemType.Folder;
        }
        /// <summary>
        /// Get a contained folder. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public StorageFile GetContainedFile(string Name, bool CaseSensitivity = false)
        {

            StorageFile storageItem = new StorageFile();
            var entries = Directory.EnumerateFiles(realPath);
            foreach (var item in entries)
            {
                if (CaseSensitivity == false)
                {

                    if (item.ToUpper() == Name.ToUpper())
                    {
                        storageItem.SetPath(Path.Combine(realPath, item));
                        storageItem.Parent = this;
                        return storageItem;
                    }
                }
                else
                {

                    if (item == Name)
                    {
                        storageItem.SetPath(Path.Combine(realPath, item));
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
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public StorageFolder GetContainedFolder(string Name, bool CaseSensitivity = false)
        {

            StorageFolder storageItem = new StorageFolder();
            var entries = Directory.EnumerateDirectories(realPath);
            foreach (var item in entries)
            {
                if (CaseSensitivity == false)
                {

                    if (item.ToUpper() == Name.ToUpper())
                    {
                        storageItem.SetPath(Path.Combine(realPath, item));
                        storageItem.Parent = this;
                        return storageItem;
                    }
                }
                else
                {

                    if (item == Name)
                    {
                        storageItem.SetPath(Path.Combine(realPath, item));
                        storageItem.Parent = this;
                        return storageItem;
                    }
                }
            }
            throw new StorageItemNotExistException(Path.Combine(realPath, Name));
        }
        /// <summary>
        /// Get all contained files in current folder.
        /// </summary>
        /// <returns></returns>
        public List<StorageFile> GetFiles()
        {
            string[] FileNames = Directory.EnumerateFiles(realPath).ToArray();
            List<StorageFile> storageFiles = new List<StorageFile>(FileNames.Length);
            foreach (var item in FileNames)
            {
                storageFiles.Add(GetContainedFile(item,true));
            }
            return storageFiles;
        }
        /// <summary>
        /// Get all contained folders in current folder.
        /// </summary>
        /// <returns></returns>
        public List<StorageFolder> GetFolders()
        {
            string[] FileNames = Directory.EnumerateDirectories(realPath).ToArray();
            List<StorageFolder> storageFolders = new List<StorageFolder>(FileNames.Length);
            foreach (var item in FileNames)
            {
                storageFolders.Add(GetContainedFolder(item, true));
            }
            return storageFolders;
        }
        /// <summary>
        /// Get all contained items in current folder.
        /// </summary>
        /// <returns></returns>
        public List<StorageItem> GetItems()
        {
            string[] ItemNames = Directory.EnumerateFileSystemEntries(realPath).ToArray();
            List<StorageItem> storageItems = new List<StorageItem>(ItemNames.Length);
            foreach (var item in ItemNames)
            {
                storageItems.Add(GetContainedItem(item, true));
            }
            return storageItems;
        }
        /// <summary>
        /// Get a contained item. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public StorageItem GetContainedItem(string Name, bool CaseSensitivity = false)
        {
            StorageItem storageItem = new StorageItem();
            var entries = Directory.EnumerateFileSystemEntries(realPath);
            foreach (var item in entries)
            {
                if (CaseSensitivity == false)
                {

                    if (item.ToUpper() == Name.ToUpper())
                    {
                        storageItem.SetPath(Path.Combine(realPath, item));
                        if (Directory.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.Folder;
                        if (File.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.File;
                        storageItem.Parent = this;
                        return storageItem;
                    }
                }
                else
                {

                    if (item == Name)
                    {
                        storageItem.SetPath(Path.Combine(realPath, item));
                        if (Directory.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.Folder;
                        if (File.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.File;
                        storageItem.Parent = this;
                        return storageItem;
                    }
                }
            }
            throw new StorageItemNotExistException(Path.Combine(realPath, Name));
        }
    }
}

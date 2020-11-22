﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LWMS.Core.FileSystem
{
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
        /// Get a contained folder. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public StorageFile GetContainedFile(string Name, bool CaseSensitivity = false)
        {

            StorageFile storageItem = new StorageFile();
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
        /// Get a contained file. Return false when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="OutItem"></param>
        /// <param name="CaseSensitivity"></param>
        /// <returns></returns>
        public bool GetContainedFile(string Name, out StorageFile OutItem, bool CaseSensitivity = false)
        {

            StorageFile storageItem = new StorageFile();
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
        /// Get a contained folder. Throw an StorageItemNotExistException when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity">Whether case sensitive</param>
        /// <returns></returns>
        public StorageFolder GetContainedFolder(string Name, bool CaseSensitivity = false)
        {

            StorageFolder storageItem = new StorageFolder();
            var F = GetContainedFolder(Name, out storageItem, CaseSensitivity);
            if (F == false)
            {
                throw new StorageItemNotExistException(Path.Combine(realPath, Name));
            }
            return storageItem;
        }
        /// <summary>
        /// Get a contained folder. Return false when cannot find it.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="CaseSensitivity"></param>
        /// <param name="OutFolder"></param>
        /// <returns></returns>
        public bool GetContainedFolder(string Name, out StorageFolder OutFolder, bool CaseSensitivity = false)
        {
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
                        break;
                }
            }
            StorageFolder storageItem = new StorageFolder();
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
        /// Get all contained files in current folder.
        /// </summary>
        /// <returns></returns>
        public List<StorageFile> GetFiles()
        {
            string[] FileNames = Directory.EnumerateFiles(realPath).ToArray();
            List<StorageFile> storageFiles = new List<StorageFile>(FileNames.Length);
            foreach (var item in FileNames)
            {
                storageFiles.Add(GetContainedFile(item, true));
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
        public bool GetContainedItem(string Name, out StorageItem OutItem, bool CaseSensitivity = false)
        {
            StorageItem storageItem = new StorageItem();

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
            return L.GetContainedItem(R);
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
    }
    [Serializable]
    public class FindFileInRootFolderException : Exception
    {
        public FindFileInRootFolderException() { }
    }
}

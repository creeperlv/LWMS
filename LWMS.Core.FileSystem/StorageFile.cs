using System;
using System.IO;

namespace LWMS.Core.FileSystem
{
    /// <summary>
    /// Represent a file.
    /// </summary>
    public class StorageFile : StorageItem
    {
        internal StorageFile()
        {
            StorageItemType = StorageItemType.File;
        }
        /// <summary>
        /// Whether equals to given object. If given object is a StorageFile, will judge according to realPath.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is StorageFile)
            {
                return (obj as StorageFile).realPath == realPath;
            }
            else return base.Equals(obj);
        }
        /// <summary>
        /// Same as Object.GetHashCode()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Returns the full name of the file. (Same as FileInfo.FullName)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return RealFile.FullName;
        }
        /// <summary>
        /// Gets the type(file name extension) of the file.
        /// </summary>
        public string FileType { get => RealFile.Extension; }
        /// <summary>
        /// Gets the length of the file.
        /// </summary>
        public long Length { get => RealFile.Length; }
        /// <summary>
        /// Gets the date and time when the current file was created.
        /// </summary>
        public DateTime DateCreated { get => RealFile.CreationTime; }
        /// <summary>
        /// Open the file.
        /// </summary>
        /// <returns></returns>
        public FileStream OpenFile()
        {
            return RealFile.Open(FileMode.Open, FileAccess.ReadWrite);
        }
        /// <summary>
        /// Write a content to target file.
        /// </summary>
        /// <param name="content"></param>
        public void WriteAllText(string content)
        {
            File.WriteAllText(realPath, content);
        }
        public static implicit operator FileInfo(StorageFile file)
        {
            return file.RealFile;
        }
        /// <summary>
        /// If destination is a file, will overwrite it.
        /// </summary>
        /// <param name="Destination"></param>
        public override void CopyTo(StorageItem Destination)
        {
            if (Destination.StorageItemType == StorageItemType.Folder)
            {
                File.Copy(realPath, Path.Combine(Destination.realPath, RealFile.Name));
            }
            else
            {
                File.Copy(realPath, Destination.realPath, true);
            }
        }
    }
}

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
                FileInfo fileInfo = new FileInfo(realPath);

                File.Copy(realPath, Path.Combine(Destination.realPath, fileInfo.Name));
            }
            else
            {
                File.Copy(realPath, Destination.realPath, true);
            }
        }
    }
}

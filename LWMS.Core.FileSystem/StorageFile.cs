using System.IO;

namespace LWMS.Core.FileSystem
{
    public class StorageFile : StorageItem
    {
        internal StorageFile()
        {
            StorageItemType = StorageItemType.File;
        }
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

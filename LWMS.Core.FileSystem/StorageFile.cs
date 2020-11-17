using System.IO;
using System.Xml.XPath;

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
            return File.Open(realPath, FileMode.Open, FileAccess.ReadWrite);
        }
        public static implicit operator FileInfo(StorageFile file)
        {
            FileInfo fileInfo = new FileInfo(file.realPath);
            return fileInfo;
        }
        /// <summary>
        /// If destination is a file, will overwrite it.
        /// </summary>
        /// <param name="Destination"></param>
        public override void CopyTo(StorageItem Destination)
        {
            if(Destination.StorageItemType== StorageItemType.Folder)
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

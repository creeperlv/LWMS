using System.IO;

namespace LWMS.Core.FileSystem
{
    public class StorageFile : StorageItem
    {
        internal StorageFile()
        {
            StorageItemType = StorageItemType.File;
        }
        public Stream OpenFile()
        {
            return File.Open(realPath, FileMode.Open, FileAccess.ReadWrite);
        }
        public static implicit operator FileInfo(StorageFile file)
        {
            FileInfo fileInfo = new FileInfo(file.realPath);
            return fileInfo;
        }
    }
}

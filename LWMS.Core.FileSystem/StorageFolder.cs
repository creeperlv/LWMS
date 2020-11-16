using System.IO;

namespace LWMS.Core.FileSystem
{
    public class StorageFolder : StorageItem
    {
        internal StorageFolder()
        {
            StorageItemType = StorageItemType.Folder;
        }
        public StorageFile GetContainedFile(string Name)
        {

            StorageFile storageItem = new StorageFile();
            var entries = Directory.EnumerateFiles(realPath);
            foreach (var item in entries)
            {
                if (item.ToUpper() == Name.ToUpper())
                {
                    storageItem.realPath = Path.Combine(realPath, item);
                    storageItem.Parent = this;
                    return storageItem;
                }
            }
            throw new StorageItemException(Path.Combine(realPath, Name));
        }
        public StorageItem GetContainedItem(string Name)
        {
            StorageItem storageItem = new StorageItem();
            var entries=Directory.EnumerateFileSystemEntries(realPath);
            foreach (var item in entries)
            {
                if (item.ToUpper() == Name.ToUpper())
                {
                    storageItem.realPath = Path.Combine(realPath, item);
                    if (Directory.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.Folder;
                    if (File.Exists(storageItem.realPath)) storageItem.StorageItemType = StorageItemType.File;
                    storageItem.Parent = this;
                    return storageItem;
                }
            }
            throw new StorageItemException(Path.Combine(realPath,Name));
        }
    }
}

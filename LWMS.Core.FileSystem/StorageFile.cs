using LWMS.Core.Authentication;
using System;
using System.Collections.Generic;
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
        public virtual Stream OpenFileWR()
        {
            if (BaseWritePermission is not null) throw new UnauthorizedException(null, BaseWritePermission[0]);
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            return RealFile.Open(FileMode.Open, FileAccess.ReadWrite);
        }
        /// <summary>
        /// Open the file.
        /// </summary>
        /// <returns></returns>
        public virtual Stream OpenFileW()
        {
            if (BaseWritePermission is not null) throw new UnauthorizedException(null, BaseWritePermission[0]);
            return RealFile.Open(FileMode.Open, FileAccess.Write);
        }
        /// <summary>
        /// Open the file.
        /// </summary>
        /// <returns></returns>
        public virtual Stream OpenFileR()
        {
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            return RealFile.Open(FileMode.Open, FileAccess.Read);
        }
        /// <summary>
        /// Open the file.
        /// </summary>
        /// <returns></returns>
        public virtual Stream OpenFileWR(string Auth)
        {
            if (BaseWritePermission is null) if (BaseReadPermission is null) return OpenFileWR();
            FileStream fs = null;
            List<string> TotalPermission = new List<string>(BaseWritePermission);
            TotalPermission.AddRange(BaseReadPermission);
            OperatorAuthentication.AuthedAction(Auth, () => { fs = RealFile.Open(FileMode.Open, FileAccess.ReadWrite); }, false, true, TotalPermission.ToArray());
            return fs;
        }
        /// <summary>
        /// Open the file.
        /// </summary>
        /// <returns></returns>
        public virtual Stream OpenFileW(string Auth)
        {
            if (BaseWritePermission is null) return OpenFileWR();
            FileStream fs = null;
            OperatorAuthentication.AuthedAction(Auth, () => { fs = RealFile.Open(FileMode.Open, FileAccess.Write); }, false, true, BaseWritePermission);
            return fs;
        }
        /// <summary>
        /// Open the file.
        /// </summary>
        /// <returns></returns>
        public virtual Stream OpenFileR(string Auth)
        {
            if (BaseReadPermission is null) return OpenFileWR();
            FileStream fs = null;
            OperatorAuthentication.AuthedAction(Auth, () => { fs = RealFile.Open(FileMode.Open, FileAccess.Read); }, false, true, BaseReadPermission);
            return fs;
        }
        /// <summary>
        /// Write a content to target file.
        /// </summary>
        /// <param name="content"></param>
        public void WriteAllText(string content)
        {
            File.WriteAllText(realPath, content);
        }
        /// <summary>
        /// Convert to FileInfo.
        /// </summary>
        /// <returns></returns>
        public FileInfo ToFileInfo()
        {
            if (BaseWritePermission is not null) throw new UnauthorizedException(null, BaseWritePermission[0]);
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            return RealFile;
        }
        /// <summary>
        /// Convert to FileInfo.
        /// </summary>
        /// <returns></returns>
        public FileInfo ToFileInfo(string AuthContext)
        {
            if (BaseWritePermission is null) if (BaseReadPermission is null) return ToFileInfo();
            FileInfo fi = null;
            List<string> TotalPermission = new List<string>(BaseWritePermission);
            TotalPermission.AddRange(BaseReadPermission);
            OperatorAuthentication.AuthedAction(AuthContext, () => { fi = RealFile; }, false, true, TotalPermission.ToArray());
            return fi;
        }
        /// <summary>
        /// If destination is a file, will overwrite it.
        /// </summary>
        /// <param name="Destination"></param>
        public override void CopyTo(StorageItem Destination)
        {

            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            if (Destination.StorageItemType == StorageItemType.Folder)
            {
                File.Copy(realPath, Path.Combine(Destination.realPath, RealFile.Name));
            }
            else
            {
                File.Copy(realPath, Destination.realPath, true);
            }
        }
        /// <summary>
        /// If destination is a file, will overwrite it.
        /// </summary>
        /// <param name="Destination"></param>
        public void CopyTo(StorageItem Destination, string Auth)
        {

            if (BaseReadPermission is null) {
                CopyTo(Destination); 
                return;
            }
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                if (Destination.StorageItemType == StorageItemType.Folder)
                {
                    File.Copy(realPath, Path.Combine(Destination.realPath, RealFile.Name));
                }
                else
                {
                    File.Copy(realPath, Destination.realPath, true);
                }
            }, false, true, BaseReadPermission);
        }
    }
}

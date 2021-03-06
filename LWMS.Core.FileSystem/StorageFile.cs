﻿using LWMS.Core.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// Write content to target file.
        /// </summary>
        /// <param name="content"></param>
        public virtual void WriteAllText(string content)
        {
            if (BaseWritePermission is not null) throw new UnauthorizedException(null, BaseWritePermission[0]);
            _WriteAllText(content);
        }
        internal void _WriteAllText(string content)
        {
            File.WriteAllText(realPath, content);

        }
        /// <summary>
        /// Write content to target file.
        /// </summary>
        /// <param name="AuthContext"></param>
        /// <param name="content"></param>
        public virtual void WriteAllText(string AuthContext, string content)
        {
            if (BaseWritePermission is not null)
                OperatorAuthentication.AuthedAction(AuthContext, () =>
                {
                    _WriteAllText(content);
                }, false, true, BaseWritePermission);
            else
            {
                _WriteAllText(content);
            }
        }
        internal string _ReadAllText()
        {
            return File.ReadAllText(realPath);
        }
        internal List<string> _ReadAllLines()
        {
            return File.ReadAllLines(realPath).ToList();
        }
        /// <summary>
        /// Read all content of target file as text.
        /// </summary>
        /// <returns></returns>
        public virtual string ReadAllText()
        {
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            return _ReadAllText();
        }
        /// <summary>
        /// Read all content of target file as text.
        /// </summary>
        /// <param name="AuthContext"></param>
        /// <returns></returns>
        public virtual string ReadAllText(string AuthContext)
        {
            if (BaseReadPermission is not null)
            {
                var content = "";
                OperatorAuthentication.AuthedAction(AuthContext, () =>
                {
                    content = _ReadAllText();
                }, false, true, BaseReadPermission);
                return content;
            }
            else return _ReadAllText();
        }
        /// <summary>
        /// Read all content of target file as text, splited by line.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> ReadAllLines()
        {
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            return _ReadAllLines();
        }
        /// <summary>
        /// Read all content of target file as text, splited by line.
        /// </summary>
        /// <param name="AuthContext"></param>
        /// <returns></returns>
        public virtual List<string> ReadAllLines(string AuthContext)
        {
            if (BaseReadPermission is not null)
            {
                List<string> content = null;
                OperatorAuthentication.AuthedAction(AuthContext, () =>
                {
                    content = _ReadAllLines();
                }, false, true, BaseReadPermission);
                return content;
            }
            else return _ReadAllLines();
        }
        /// <summary>
        /// Convert to FileInfo.
        /// </summary>
        /// <returns></returns>
        public virtual FileInfo ToFileInfo()
        {
            if (BaseWritePermission is not null) throw new UnauthorizedException(null, BaseWritePermission[0]);
            if (BaseReadPermission is not null) throw new UnauthorizedException(null, BaseReadPermission[0]);
            return RealFile;
        }
        /// <summary>
        /// Convert to FileInfo.
        /// </summary>
        /// <returns></returns>
        public virtual FileInfo ToFileInfo(string AuthContext)
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
        public virtual void CopyTo(StorageItem Destination, string Auth)
        {

            if (BaseReadPermission is null)
            {
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

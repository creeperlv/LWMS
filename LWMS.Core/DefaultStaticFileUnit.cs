using CLUNL.Pipeline;
using LWMS.Core.Configuration;
using LWMS.Core.FileSystem;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Utilities;
using System;
using System.Diagnostics;
using System.IO;

namespace LWMS.Core
{
    /// <summary>
    /// A basic unit that will perform as a static server: sending out static content on disk by calling Tools00.SendFile.
    /// </summary>
    public class DefaultStaticFileUnit : IPipedProcessUnit
    {
        public DefaultStaticFileUnit()
        {
        }
        public PipelineData Process(PipelineData Input)
        {
            HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
            var path0 = context.Request.Url.LocalPath.Substring(1);
            StorageItem Result;
            if(ApplicationStorage.ObtainItemFromRelativeURL(path0, out Result, false))
            {
                if(Result.StorageItemType== StorageItemType.Folder)
                {
                    StorageFile DefaultPage;
                    if (((StorageFolder)Result).GetFile(GlobalConfiguration.DefaultPage,out DefaultPage,false))
                    {

                        Tools00.SendFile(context, DefaultPage);
                        (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                        return Input;
                    }
                }
                else
                {
                    Tools00.SendFile(context, Result.ToStorageFile());
                    (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                    return Input;
                }
            }
            return Input;
        }

    }
}

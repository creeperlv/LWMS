using CLUNL.Pipeline;
using LWMS.Core;
using LWMS.Core.Configuration;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.SBSDomain;
using System;
using System.Collections.Generic;
using System.IO;

namespace LWMS.EventDrivenSupport
{
    public class EDREntry : IPipedProcessUnit
    {
        internal static List<MappedType> RouteTargets = null;
        public EDREntry()
        {

        }
        public PipelineData Process(PipelineData Input)
        {
            {
                HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
                var requests = ApplicationConfiguration.Current.GetValueArray("RoutedRequests");
                if (RouteTargets == null)
                {
                    var targets = ApplicationConfiguration.Current.GetValueArray("RouteTargets");
                    RouteTargets = new List<MappedType>();
                    //Initialize the targets.
                    foreach (var item in targets)
                    {
                        var parted = item.Split(',');
                        FileInfo fi = new(parted[0]);
                        var asm = DomainManager.LoadFromFile(context.PipelineAuth, fi.FullName);
                        var t = asm.GetType(parted[1]);
                        MappedType mappedType = new MappedType(fi.Name, Activator.CreateInstance(t));
                        RouteTargets.Add(mappedType);
                    }
                }
                else if (RouteTargets.Count is 0)
                {
                    var targets = ApplicationConfiguration.Current.GetValueArray("RouteTargets");
                    foreach (var item in targets)
                    {
                        var parted = item.Split(',');
                        FileInfo fi = new(parted[0]);
                        var asm = DomainManager.LoadFromFile(context.PipelineAuth, fi.FullName);
                        var t = asm.GetType(parted[1]);
                        MappedType mappedType = new MappedType(fi.Name, Activator.CreateInstance(t));
                        RouteTargets.Add(mappedType);
                    }
                }
                var path0 = context.Request.Url.LocalPath.Substring(1);//Dispose the first '/'
                if (path0 is not "")//Ignore when the path is empty to prevent potential problems when performing matching.
                {

                    for (int i = 0; i < requests.Length; i++)
                    {
                        if (path0.ToUpper().StartsWith(requests[i].ToUpper()))//Ignore case
                        {
                            (Input.SecondaryData as HttpPipelineArguments).isHandled = (RouteTargets[i].TargetObject as IHttpEventHandler).Handle(context);
                            return Input;
                        }
                    }
                }
            }
            return Input;
        }
    }
    public interface IHttpEventHandler
    {
        bool Handle(HttpListenerRoutedContext context);
    }
}

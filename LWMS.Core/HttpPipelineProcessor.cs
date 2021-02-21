using CLUNL;
using CLUNL.Pipeline;
using LWMS.Core.SBSDomain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LWMS.Core
{
    public class HttpPipelineProcessor : IPipelineProcessor
    {
        List<IPipedProcessUnit> processUnits = new List<IPipedProcessUnit>();
        List<MappedType> _processUnits = new List<MappedType>();
        public void Init(params IPipedProcessUnit[] units)
        {
            if (units.Length == 0)
            {
                processUnits.Add(new DefaultProcessUnit());
            }
            else
            {
                processUnits = new List<IPipedProcessUnit>(units);
            }
        }
        public void Init(params MappedType[] units)
        {
            if (units.Length == 0)
            {
                FileInfo fi = new FileInfo(Assembly.GetAssembly(typeof(DefaultProcessUnit)).FullName);
                _processUnits.Add(new MappedType(fi.Name,new DefaultProcessUnit()));
            }
            else
            {
                _processUnits = new List<MappedType>(units);
            }
        }
        public void Init()
        {
            Init(new IPipedProcessUnit[0]);
        }

        public void Init(ProcessUnitManifest manifest)
        {
            processUnits = manifest.GetUnitInstances();
        }
        public PipelineData Process(PipelineData Input)
        {
            bool willIgnore = false;
            try
            {
                if (LibraryInfo.GetFlag(FeatureFlags.Pipeline_IgnoreError) == 1)
                {
                    willIgnore = true;
                }
            }
            catch (Exception)
            {
                //Ignore
            }
            return Process(Input, willIgnore);
        }

        public PipelineData Process(PipelineData Input, bool IgnoreError)
        {
            if (IgnoreError)
            {

                foreach (var item in _processUnits)
                {
                    try
                    {
                        if (((HttpPipelineArguments)Input.SecondaryData).isHandled == true) return Input;
                        var output = (item.TargetObject as IPipedProcessUnit).Process(Input);
                        if (Input.CheckContinuity(output))
                        {
                            Input = output;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return Input;
            }
            else
            {

                foreach (var item in _processUnits)
                {
                    if (((HttpPipelineArguments)Input.SecondaryData).isHandled == true) return Input;
                    var output = (item.TargetObject as IPipedProcessUnit).Process(Input);
                    if (Input.CheckContinuity(output))
                    {
                        Input = output;
                    }
                    else
                    {
                        throw new PipelineDataContinuityException((item.TargetObject as IPipedProcessUnit));
                    }
                }
                return Input;
            }
        }
    }
    public class HttpPipelineArguments
    {
        public bool isHandled = false;
    }
}

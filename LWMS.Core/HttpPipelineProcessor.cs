using CLUNL;
using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace LWMS.Core
{
    public class HttpPipelineProcessor : IPipelineProcessor
    {
        List<IPipedProcessUnit> processUnits = new List<IPipedProcessUnit>();
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

                foreach (var item in processUnits)
                {
                    try
                    {
                        if (((HttpPipelineArguments)Input.SecondaryData).isHandled == true) return Input;
                        var output = item.Process(Input);
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

                foreach (var item in processUnits)
                {
                    if (((HttpPipelineArguments)Input.SecondaryData).isHandled == true) return Input;
                    var output = item.Process(Input);
                    if (Input.CheckContinuity(output))
                    {
                        Input = output;
                    }
                    else
                    {
                        throw new PipelineDataContinuityException(item);
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

using CLUNL.Pipeline;
using LWMS.Core.FileSystem;
using System;

namespace LWMS.FunctionTestModule
{
    public class Class1 : IPipedProcessUnit
    {
        /// <summary>
        /// Initialize and run some tests.
        /// </summary>
        public Class1()
        {
            try
            {
                var folder = ApplicationStorage.CurrentModule;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public PipelineData Process(PipelineData Input)
        {
            return Input;
        }
    }
}

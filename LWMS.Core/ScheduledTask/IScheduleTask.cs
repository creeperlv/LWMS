using LWMS.Core.SBSDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Core.ScheduledTask
{
    public class ScheduleTask
    {
        internal static Dictionary<string, SingleTask> ScheduledTasks = new();
        internal class SingleTask
        {
            internal ScheduleTaskGap gap;
            internal MappedType RealTask;
        }
        public static void Schedule(Type type, string TaskName, ScheduleTaskGap gap)
        {

        }
    }
    internal class TaskRunner
    {
        internal static bool Stop = false;
        internal static bool Pause = false;
        internal static void StartRun()
        {
            Task.Run(async () =>
            {
                int step = 1;
                while (Stop is true)
                {
                    await Task.Delay(5000);
                    if (Pause is false)
                    {

                        foreach (var item in ScheduleTask.ScheduledTasks)
                        {
                            bool willExecute = false;
                            switch (item.Value.gap)
                            {
                                case ScheduleTaskGap.Sec5:
                                    willExecute = step % 1 == 0;
                                    break;
                                case ScheduleTaskGap.Sec10:
                                    willExecute = step % 2 == 0;
                                    break;
                                case ScheduleTaskGap.Sec30:
                                    willExecute = step % 6 == 0;
                                    break;
                                case ScheduleTaskGap.Min1:
                                    willExecute = step % 12 == 0;
                                    break;
                                case ScheduleTaskGap.Min5:
                                    willExecute = step % 60 == 0;
                                    break;
                                case ScheduleTaskGap.Min15:
                                    willExecute = step % 180 == 0;
                                    break;
                                case ScheduleTaskGap.Min30:
                                    willExecute = step % 360 == 0;
                                    break;
                                case ScheduleTaskGap.Hour1:
                                    willExecute = step % 720 == 0;
                                    break;
                                case ScheduleTaskGap.Hour6:
                                    willExecute = step % 4320 == 0;
                                    break;
                                case ScheduleTaskGap.Hour12:
                                    willExecute = step % 8640 == 0;
                                    break;
                                case ScheduleTaskGap.Day1:
                                    willExecute = step % 17280 == 0;
                                    break;
                                default:
                                    break;
                            }
                            if (willExecute is true)
                            {
                                (item.Value.RealTask.TargetObject as IScheduleTask).Task();
                            }
                        }
                        if (step % 17280 == 0) step = 0;
                        step++;
                    }
                }
            });
        }
    }
    public enum ScheduleTaskGap
    {
        Sec5, Sec10, Sec30, Min1, Min5, Min15, Min30, Hour1, Hour6, Hour12, Day1
    }
    public interface IScheduleTask
    {
        void Task();
    }
}

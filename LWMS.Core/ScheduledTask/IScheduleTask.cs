﻿using LWMS.Core.Authentication;
using LWMS.Core.SBSDomain;
using LWMS.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            internal string parent;
        }
        /// <summary>
        /// Update tasks with the newest dll files.
        /// </summary>
        /// <param name="AuthContext">Auth who has `Core.SBS.Update` permission</param>
        public static void UpdateTasks(string AuthContext)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => {
                foreach (var item in ScheduledTasks)
                {
                    item.Value.RealTask.Update(AuthContext);
                }
            }, false, false, PermissionID.Core_SBS_Update, PermissionID.Core_SBS_All);
        }
        /// <summary>
        /// Register a task.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="TaskName">Identifiable Name, should be unique.</param>
        /// <param name="gap"></param>
        public static void Schedule(Type type, string TaskName, ScheduleTaskGap gap)
        {
            StackTrace st = new StackTrace(1);
            var item = st.GetFrame(0);
            var ModuleName = item.GetMethod().DeclaringType.Assembly.GetName().Name;
            SingleTask ST = new SingleTask();
            ST.gap = gap;
            ST.parent = ModuleName;
            ST.RealTask = MappedType.CreateFrom(type);
            if (!ScheduledTasks.ContainsKey(TaskName))
            {
                ScheduledTasks.Add(TaskName, ST);

                Trace.WriteLine(Language.Query("LWMS.ScheduleTask.Schedule", "{0} has been added into scheduled tasks.", TaskName));
            }
        }
        /// <summary>
        /// Unregister a task. Only tasks that belongs to the assembly that caller existed in can be unregisterd.
        /// </summary>
        /// <param name="TaskName"></param>
        public static void Unschedule(string TaskName)
        {
            if (ScheduledTasks.ContainsKey(TaskName))
            {
                StackTrace st = new StackTrace(1);
                var item = st.GetFrame(0);
                var ModuleName = item.GetMethod().DeclaringType.Assembly.GetName().Name;
                if (ScheduledTasks[TaskName].parent == ModuleName)
                {
                    ScheduledTasks.Remove(TaskName);
                }
                else
                {
                    throw new OperateNotProcessedScheduleTaskException();
                }
            }
        }
        internal static void _Unschedule(string TaskName)
        {
            if (ScheduledTasks.ContainsKey(TaskName))
                ScheduledTasks.Remove(TaskName);
        }
        /// <summary>
        /// Unregister a task with auth. If the auth processes `Core.ScheduleTask.Unschedule` permission, all tasks can be removed.
        /// </summary>
        /// <param name="Auth"></param>
        /// <param name="TaskName"></param>
        public static void Unschedule(string Auth, string TaskName)
        {
            OperatorAuthentication.AuthedAction(Auth, () =>
            {

                if (ScheduledTasks.ContainsKey(TaskName))
                    ScheduledTasks.Remove(TaskName);
            }, false, true, PermissionID.ScheduleTask_Unschedule, PermissionID.ScheduleTask_All);
        }
    }

    [Serializable]
    public class OperateNotProcessedScheduleTaskException : Exception
    {
        public OperateNotProcessedScheduleTaskException() : base("Operating task that is not belong to current module.") { }
    }
    internal class TaskRunner
    {
        internal static bool Stop = false;
        internal static bool Pause = false;
        async static void Run()
        {

            int step = 0;
            while (Stop is false)
            {
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
                            Trace.WriteLine(Language.Query("LWMS.ScheduleTask.Run", "Task {0} has been invoked on schedule.", item.Key));
                        }
                    }
                    if (step % 17280 == 0) step = 0;
                    step++;
                }
                await Task.Delay(5000);
            }
        }
        internal static void StartRun()
        {
            Task.Run(Run);
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

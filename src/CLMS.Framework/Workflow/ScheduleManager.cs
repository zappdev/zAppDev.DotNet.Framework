using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using NCrontab;
using NHibernate;
using CLMS.Framework.Data;
using CLMS.Framework.Data.DAL;

namespace CLMS.Framework.Workflow
{
    public class ScheduleManager
    {
        private static IRepositoryBuilder Builder;
        private readonly ILog _log;

        public ScheduleManager()
        {
            _log = LogManager.GetLogger(typeof(ScheduleManager));
        }

        public void ProcessSchedules()
        {
            try
            {
                ProcessExpiredAndDelayedPendingJobs();
                ProcessAutoStartSchedules();
            }
            catch (Exception ex)
            {
                _log.Error("Error processing schedules.", ex);
                throw;
            }
        }

        private void ProcessAutoStartSchedules()
        {
            var schedules = GetSchedules();
            _log.DebugFormat("Found {0} Active Schedules.", schedules.Count);
            foreach (var schedule in schedules.Where(a => a.Active))
            {
                try
                {
                    if (schedule.ExpireOn <= DateTime.UtcNow)
                    {
                        // Schedule has Expired but it is still active
                        // Mark as Inactive and update the database
                        schedule.Active = false;
                        MiniSessionManager
                        .ExecuteInUoW(manager =>
                        {
                            Builder.CreateCreateRepository(manager).Save(schedule);
                        });
                        continue;
                    }
                    // Should be evaluated?
                    var nextExecTime = GetNextExecutionTime(schedule);
                    if (nextExecTime != DateTime.MinValue && nextExecTime <= DateTime.UtcNow)
                    {
                        // Execute
                        ExecuteSchedule(schedule);
                    }
                    else
                    {
                        _log.DebugFormat("Scheduled Activity: {0} - {1} will be executed on: {2}", schedule.Workflow,
                                         schedule.Workflow,
                                         nextExecTime);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Error processing Scheduled Activity: {schedule.Workflow} - {schedule.Workflow}", ex);
                }
            }
        }

        public IWorkflowExecutionResult ExecuteSchedule(WorkflowSchedule schedule)
        {
            _log.Debug("------------------------------------------------------");
            _log.DebugFormat("Executing Scheduled Activity: {0} - {1}", schedule.Workflow, schedule.Workflow);
            IWorkflowExecutionResult result = null;
            try
            {
                MiniSessionManager
                .ExecuteInUoW(manager =>
                {
                    result = WorkflowManager.Current.ExecuteWorkflow(schedule.Workflow);
                });
                schedule.IsLastExecutionSuccess = true;
                schedule.LastExecutionMessage = "Executed Successfully";
            }
            catch (Exception ex)
            {
                _log.Error($"Error executing Scheduled Activity: {schedule.Workflow}", ex);
                schedule.IsLastExecutionSuccess = false;
                schedule.LastExecutionMessage = $"{ex.Message} - {ex.StackTrace}";
                result = new WorkflowExecutionResult { Status = WorkflowStatus.Failed };
            }
            schedule.LastExecution = DateTime.UtcNow;
            MiniSessionManager.ExecuteInUoW(manager =>
            {
                Builder.CreateCreateRepository(manager).Save(schedule);
            });
            _log.DebugFormat("Finished Executing Scheduled Activity: {0} - {1}", schedule.Workflow, schedule.Workflow);
            _log.Debug("------------------------------------------------------");
            return result;
        }

        private DateTime GetNextExecutionTime(WorkflowSchedule schedule)
        {
            if (!schedule.Active) return DateTime.MinValue; // Inactive
            if (schedule.ExpireOn <= DateTime.UtcNow) return DateTime.MinValue; // Expired
            var baseTime = schedule.StartDateTime == null || schedule.StartDateTime <= DateTime.UtcNow ? schedule.LastExecution : schedule.StartDateTime;
            return baseTime == null ? DateTime.UtcNow : Utilities.Common.GetNextExecutionTime(schedule.CronExpression, baseTime);
        }

        public static void SetSchedules(List<WorkflowSchedule> schedules)
        {
            MiniSessionManager
            .ExecuteInUoW(manager =>
            {
                var allScheduleNames = Builder.CreateRetrieveRepository().GetAll<WorkflowSchedule>().Select(s => s.Workflow).ToList();
                //add new
                foreach (var newSchedule in schedules.Where(s => !allScheduleNames.Contains(s.Workflow)))
                {
                    newSchedule.Active = true;
                    Builder.CreateCreateRepository(manager).Save(newSchedule);
                }
            });
        }

        internal static List<WorkflowSchedule> GetSchedules(bool onlyactive = true)
        {
            return MiniSessionManager
                   .ExecuteInUoW(manager =>
            {
                var data = Builder.CreateRetrieveRepository().GetAsQueryable<WorkflowSchedule>();
                if (onlyactive)
                {
                    data = data.Where(s => s.Active);
                }
                return data.ToList();
            });
        }

        private void ProcessExpiredAndDelayedPendingJobs()
        {
            var pendingJobs = GetExpiredAndDelayedPendingJobs();
            _log.DebugFormat("Found {0} expired/delayed Pending Jobs.", pendingJobs.Count);
            foreach (var pendingJob in pendingJobs)
            {
                try
                {
                    if (pendingJob.Expires && pendingJob.ExpirationDateTime <= DateTime.UtcNow)
                    {
                        // Continue the expired workflow
                        WorkflowManager.Current.Expire(pendingJob.Id.Value);
                    }
                    else
                    {
                        // Continue delayed workflows
                        _log.Warn("NOT IMPLEMENTED! Implement handling of delayed/system pending jobs");
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(
                        $"Error processing Expired/Delayed Pending Job: {pendingJob.Id.Value} - {pendingJob.Name} - {pendingJob.PendingStep}", ex);
                    throw;
                }
            }
        }

        private static List<WorkflowContextBase> GetExpiredAndDelayedPendingJobs()
        {
            return MiniSessionManager.ExecuteInUoW(manager =>
            {
                var expiredAndDelayedPendingJobs = Builder.CreateRetrieveRepository().GetAsQueryable<WorkflowContextBase>()
                .Where(p => p.Expires && p.ExpirationDateTime <= DateTime.UtcNow)
                .ToList();
                return expiredAndDelayedPendingJobs;
            });
        }
    }
}
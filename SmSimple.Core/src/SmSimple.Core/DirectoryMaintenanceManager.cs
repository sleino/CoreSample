using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace SmSimple.Core
{
    /// <summary>
    /// Deletes old files from directory. Add directory and day limit using AddDirectory() method. 
    /// DirectoryMaintenanceManager runs a separate thread which deletes files older than the limit value.
    /// Start the thread calling Start(), stop it calling Stop().
    /// </summary>
    public sealed class DirectoryMaintenanceManager : IDisposable
    {
        private readonly TaskHelper taskHelper;

        private volatile Dictionary<string, int> directoryNamesAndDays = new Dictionary<string, int>();
        private volatile Dictionary<string, bool> directoryNamesAndRemoveDirectory = new Dictionary<string, bool>();

        private DateTime lastFileMaintenanceTime = DateTime.MinValue;
        public bool Verbose { get; set; }

        public string FileFilter { get; set; } = "*";

        public DirectoryMaintenanceManager() {
            taskHelper = new TaskHelper();
            taskHelper.StartRecurringTask(CheckForFileMaintenance,TimeSpan.FromSeconds(1));
        }

        // -1: do not delete files, 0: delete files immediately
        public void AddDirectory(string directoryName, int days, bool removeDirectoryIfEmpty = false)
        {
            if (Verbose)
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, String.Format("Adding directory {0} under automatic maintenance. ", directoryName));
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, String.Format("Files older than {0} days will be deleted automatically", days.ToIcString()));
            }
            directoryNamesAndDays[directoryName] = days;
            directoryNamesAndRemoveDirectory[directoryName] = removeDirectoryIfEmpty;
        }
  
        private void CheckForFileMaintenance()
        {
#if DEBUG
            foreach (var key in directoryNamesAndDays.Keys)
                Debug.Assert(key.Length > 0);
#endif

            TimeSpan timeSinceLastMaintenance = DateTime.Now.Subtract(lastFileMaintenanceTime);
            if (timeSinceLastMaintenance.TotalMinutes < minutesBetweenMaintenance)
                return;

            DoMaintenanceNow();
        }

        private const int minutesBetweenMaintenance = 10;

        internal void DoMaintenanceNow()
        {
            lastFileMaintenanceTime = DateTimeEx.Now;
            foreach (var kvp in directoryNamesAndDays)
            {
                int days = kvp.Value;
                if (days < 0)
                    continue;

                if (Verbose)
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, kvp.Key + ": Deleting files older than " + days + " days.");
                bool removeDirectory = false;
                if (directoryNamesAndRemoveDirectory.ContainsKey(kvp.Key))
                    removeDirectory = directoryNamesAndRemoveDirectory[kvp.Key];

                DeleteOldFiles(kvp.Key, days, removeDirectory);
            }

            if (Verbose)
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Maintenance complete. Next maintenance time: " + DateTimeEx.ToStdDateTimeFormat(lastFileMaintenanceTime.AddMinutes(minutesBetweenMaintenance)));
        }

        private void DeleteOldFiles(string directory, int daysToKeepFiles, bool removeDirectory)
        {
            if (string.IsNullOrEmpty(directory))
                return;
            if (daysToKeepFiles < 0)
                return;
            try
            {
                if (!Directory.Exists(directory))
                    return;

                var limitDate = DateTime.Now.AddDays(-daysToKeepFiles);
                FileSystem.DeleteOldFilesFromSubDirectories(directory, FileFilter, daysToKeepFiles, removeDirectory);
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in DeleteOldFiles " + directory + ".", ex);
            }
        }

    
        #region Dispose methods

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool isDisposing)
        {
            if (disposed)
                return;

            if (isDisposing)
            {
                if (taskHelper != null) {
                    taskHelper.Dispose();
                    taskHelper.StopTask();
                }
                    
                disposed = true;
            }
        }

        #endregion
       
    }
}

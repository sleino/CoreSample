using System;
using System.Diagnostics;
using System.IO;

namespace SmSimple.Core
{
    public static class FileSystem
    {
  
        public static bool CopyFileDoNotOverwrite(string sourceFileNameWithPath, string destinationDirectory)
        {
            try
            {
                if (!Directory.Exists(destinationDirectory))
                    if (!TryCreateDirectory(destinationDirectory))
                        return false;

                if (!File.Exists(sourceFileNameWithPath))
                    return false;

                string fileName = Path.GetFileName(sourceFileNameWithPath);
                string destinationFileName = destinationDirectory + "\\" + fileName;
                while (File.Exists(destinationFileName))
                    destinationFileName = destinationDirectory + "\\" + Path.GetRandomFileName() + fileName;

                File.Copy(sourceFileNameWithPath, destinationFileName, true);
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Exception while copying file " + ex.Message +
                    " 1." + sourceFileNameWithPath +
                    " 2. " + destinationDirectory;
                ExceptionRecorder.RecordException(msg);
                return false;
            }
        }

        public static bool CopyFile(string sourceFileNameWithPath, string destinationDirectory)
        {
            try
            {
                string fileName = Path.GetFileName(sourceFileNameWithPath);
                return CopyFileOverwrite(sourceFileNameWithPath, destinationDirectory, fileName);
            }
            catch (Exception ex)
            {
                string msg = "Exception while copying file " + ex.Message +
                    " 1." + sourceFileNameWithPath +
                    " 2. " + destinationDirectory;
                ExceptionRecorder.RecordException(msg);
                return false;
            }
        }

        public static bool CopyFileOverwrite(string sourceFileNameWithPath, string destinationDirectory,  string destinationFileName)
        {
            try
            {
                if (!Directory.Exists(destinationDirectory))
                    if (!TryCreateDirectory(destinationDirectory))
                        return false;
                
                if (!File.Exists(sourceFileNameWithPath))
                    return false;

                string target = destinationDirectory + "\\" + destinationFileName;
                if (target == sourceFileNameWithPath)
                    return false;

                File.Copy(sourceFileNameWithPath, target, true);
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Exception while copying file " + ex.Message  +
                    " 1." + sourceFileNameWithPath + 
                    " 2. "+ destinationDirectory + 
                    " 3. " + destinationFileName;
                ExceptionRecorder.RecordException(msg);
                return false;
            }
        }

        public static bool CopyFile(string sourceDirectory, string sourceFileName, string destinationDirectory)
        {
            try
            {
                if (!Directory.Exists(destinationDirectory))
                    if (!TryCreateDirectory(destinationDirectory))
                        return false;

                File.Copy(sourceDirectory + "\\" + sourceFileName, destinationDirectory + "\\" + sourceFileName, true);
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Exception while copying file " + ex.Message;
                if (sourceFileName != null)
                    msg += msg + ". Source: " + sourceFileName;
                if (destinationDirectory != null)
                    msg += msg + ".DestinationDirectory:" + destinationDirectory;
                ExceptionRecorder.RecordException(msg);
                return false;
            }
        }

        public static bool TryCopy(string fullSourceFileName, string fullDestinationFileName, bool allowOverWrite = false) {
            try {
                if (fullSourceFileName == fullDestinationFileName)
                    return false;

                string destinationDirectory = Path.GetDirectoryName(fullDestinationFileName);
                if (!Directory.Exists(destinationDirectory))
                    if (!TryCreateDirectory(destinationDirectory))
                        return false;


                File.Copy(fullSourceFileName, fullDestinationFileName, allowOverWrite);
                return true;
            }
            catch (Exception ex ) {
                string msg = "Exception while copying file " + ex.Message;
                if (fullSourceFileName != null)
                    msg += msg + ". Source: " + fullSourceFileName;
                if (fullDestinationFileName != null)
                    msg += msg + ".DestinationFile:" + fullDestinationFileName;
                ExceptionRecorder.RecordException(msg);
                return false;
            } 
        }     

        public static bool TryCopyFile(string fullSourceFileName, string destinationDirectory,   bool allowOverWrite = false)
        {
            try
            {
                if (!Directory.Exists(destinationDirectory))
                    if (!TryCreateDirectory(destinationDirectory))
                        return false;

                string destinationFile = destinationDirectory + "\\" + Path.GetFileName(fullSourceFileName);

                if (destinationFile == fullSourceFileName)
                    return false;

                File.Copy(fullSourceFileName, destinationFile, allowOverWrite);
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Exception while copying file " + ex.Message;
                if (fullSourceFileName != null)
                    msg += msg + ". Source: " + fullSourceFileName;
                if (destinationDirectory != null)
                    msg += msg + ".DestinationDirectory:" + destinationDirectory;
                ExceptionRecorder.RecordException(msg);
                return false;
            }
        }

        public static bool TryMoveFile(string fullSourceFileName, string destinationDirectory, bool overwriteExistingTarget= false)
        {
            try
            {
                 if (!Directory.Exists(destinationDirectory))
                    if (!TryCreateDirectory(destinationDirectory))
                        return false;

                string target = destinationDirectory + "\\" + Path.GetFileName(fullSourceFileName);
                if (fullSourceFileName == target)
                {
                    Debug.Assert(false, "trying to copy file over itself");
                    return true;
                }

                if (overwriteExistingTarget && File.Exists(target))
                    TryDeleteFile(target);


                File.Move(fullSourceFileName, target);
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Exception while moving file " + ex.Message;
                if (fullSourceFileName != null)
                    msg += msg + ". Source: " + fullSourceFileName;
                if (destinationDirectory != null)
                    msg += msg + ".DestinationDirectory:" + destinationDirectory;
                ExceptionRecorder.RecordException(msg);
                return false;
            }
        }

        public static bool MoveFile(string sourceDirectory, string sourceFileName, string destinationDirectory)
        {
            try
            {
                if (!Directory.Exists(destinationDirectory))
                    if (!TryCreateDirectory(destinationDirectory))
                        return false;

                File.Copy(sourceDirectory + "\\" + sourceFileName, destinationDirectory + "\\" + sourceFileName, true);
                File.Delete(sourceDirectory + "\\" + sourceFileName);
                return true;
            }
            catch (Exception ex)
            {
                string msg = "Exception while moving file " + ex.Message;
                if (sourceFileName != null)
                    msg += msg + ". Source: " + sourceFileName;
                if (destinationDirectory != null)
                    msg += msg + ".DestinationDirectory:" + destinationDirectory;
                ExceptionRecorder.RecordException(msg);
                return false;
            }
        }

        public static void ClearDirectoryRecursively(string directory)
        {
            int test = 0;
            try
            {
                if (Directory.Exists(directory))
                {
                    test = 1;
                    Directory.Delete(directory, true);
                }
                test = 2;
                Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                var msg = "Exception while clearing directory " + test + ":" + directory + ":";
                ExceptionRecorder.RecordException( msg, ex);
            }
        }



        public static bool DeleteOldFile(string path, DateTime lastWriteTime, int daysToKeep)
        {
            try
            {
                TimeSpan age = DateTimeEx.Now.Subtract(lastWriteTime);
                if (age.TotalDays > daysToKeep)
                    return FileSystem.TryDeleteFile(path);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception while deleting file " + path + ".", ex);
                return false;
            }
        }

        // return true if no errors took place, even if file was not deleted
        public static bool DeleteOldFile(string fileName, int daysToKeep)
        {
            try
            {
                TimeSpan age = DateTimeEx.Now.Subtract(File.GetLastWriteTime(fileName));
                if (age.TotalDays > daysToKeep)
                    return FileSystem.TryDeleteFile(fileName);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception while deleting file " + fileName + ".", ex);
                return false;
            }
        }

        public static void DeleteOldFilesFromSubDirectories(string directoryPath, string filter, int daysToKeep,
            bool removeEmptySubDirectories)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    return;
               
                //System.Threading.Thread.Sleep(0);
                // FileData[] fileDataArray = FastDirectoryEnumerator.GetFiles(directoryPath, filter, SearchOption.TopDirectoryOnly);

                string[] files = Directory.GetFiles(directoryPath, filter, SearchOption.TopDirectoryOnly);

              
                foreach (var fileName in files)
                {
                    DateTime lastWriteTime = Directory.GetLastAccessTime(fileName);
                    FileSystem.DeleteOldFile(fileName, lastWriteTime,daysToKeep);
                }

                foreach (var subdirectoryName in Directory.GetDirectories(directoryPath))
                {
                    DeleteOldFilesFromSubDirectories(subdirectoryName, filter, daysToKeep, removeEmptySubDirectories);
                }

                if (removeEmptySubDirectories)
                    if (Directory.GetFiles(directoryPath).GetLength(0) == 0 &&
                        Directory.GetDirectories(directoryPath).GetLength(0) == 0)
                        Directory.Delete(directoryPath);
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception while deleting old files : ", ex);
            }
        }

        public static bool DeleteFilesFromDirectory(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                    return true;

                foreach (var file in Directory.GetFiles(directory))
                    File.Delete(file);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception while deleting files : ", ex);
                return false;
            }
        }


        private static bool IsFileLocked(Exception ioException)
        {
            int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ioException) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }

        public static void UnlockFile(FileStream fileStream)
        {
            if (fileStream == null)
                return;
            fileStream.Flush();
        }

        public static long FileSize(string fileName)
        {
            FileInfo fi = new System.IO.FileInfo(fileName);
            return fi.Length;
        }

        public static DateTime FileCreationTime(string fileName)
        {
            FileInfo fi = new System.IO.FileInfo(fileName);
            return fi.CreationTime;
        }

        // Copy directory structure recursively
        public static bool CopyDirectory(string sourceDirectory, string destDirectory, string fileSearchPattern)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(destDirectory) || fileSearchPattern == null)
                    return false;

                Debug.Assert(sourceDirectory.Length > 0 || destDirectory.Length > 0, "copyDirectory");
                
                if (!PrepareSourceDirectory(sourceDirectory))
                    return false;

                if (destDirectory[destDirectory.Length - 1] != Path.DirectorySeparatorChar)
                    destDirectory += Path.DirectorySeparatorChar;

                if (!FileSystem.TryCreateDirectory(destDirectory))
                    return false;

                String[] Files = Directory.GetFiles(sourceDirectory, fileSearchPattern);
                foreach (var Element in Files)
                    File.Copy(Element, destDirectory + Path.GetFileName(Element), true);

                bool ok = true;
                String[] dirs = Directory.GetDirectories(sourceDirectory);
                foreach (var Element in dirs)
                    ok = ok && CopyDirectory(Element, destDirectory + Path.GetFileName(Element), fileSearchPattern);
                return ok;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("CopyDirectory ", ex);
                return false;
            }
        }

        private static bool PrepareSourceDirectory(string sourceDirectory)
        {
            if (!FileSystem.TryCreateDirectory(sourceDirectory))
                return false;

            return true;
        }

        public static bool CopyDirectory(string sourceDirectory, string destDirectory)
        {
            return CopyDirectory(sourceDirectory, destDirectory, "*");
        }

        public static string TryGetFileName(string path)
        {
            try
            {
                return Path.GetFileName(path);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("TryGetFileName", ex);
                return string.Empty;
            }
        }

        public static string TryGetDirectoryName(string path)
        {
            try
            {
                if (path == string.Empty)
                    return string.Empty;
                return Path.GetDirectoryName(path);
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Invalid file name entered while trying to get path name . ", ex);
                return string.Empty;
            }
        }

        public static bool TryDeleteDirectory(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    ExceptionRecorder.RecordException(
                        "Null or empty path name entered while trying to delete directory. ");
                    return false;
                }

                if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    int index = path.IndexOfAny(Path.GetInvalidPathChars());
                    ExceptionRecorder.RecordException("Path name contains invalid character:" + path[index]);
                    return false;
                }
                const bool recursive = true;
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive);

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                ExceptionRecorder.RecordException(
                    "UnauthorizedAccessException while trying to delete directory. Check if Explorer or some other program is accessing directory, subdirectory or any file being deleted. Path: " + path + "." + ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException(
                    "Exception while trying to delete directory. " + path + "." + ex.Message, ex);
                return false;
            }
        }


        public static bool TryDeleteFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception while trying to delete file. " + fileName + ".", ex);
                return false;
            }
        }

        public static bool DriveExists(string path)
        {
            int index = path.IndexOf(":");
            if (index == -1)
                return false;
            if (index > 1)
                return false;
            string driveName = path.Substring(0, 2) + @"\";
            return Directory.Exists(driveName);
        }

        public static bool TryCreateDirectory(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    ExceptionRecorder.RecordException(
                        "Null or empty path name entred while trying to create directory. ");
                    return false;
                }

                if (Directory.Exists(path))
                    return true;

                if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    int index = path.IndexOfAny(Path.GetInvalidPathChars());
                    ExceptionRecorder.RecordException("Path name contains invalid character:" + path[index]);
                    return false;
                }

                Directory.CreateDirectory(path);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                ExceptionRecorder.RecordException(
                    "UnauthorizedAccessException while trying to create directory. " + path + "." + ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException(
                    "Exception while trying to create directory. " + path + "." + ex.Message, ex);
                return false;
            }
        }




        public static string[] TryGetSubDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException(
                    "Exception while trying to read subdirectories directory. " + path + "." + ex.Message, ex);
                return null;
            }
        }

        public static string RemoveInvalidChars(string illegal)
        {
            //string illegal = "\"M\"\\a/ry/ h**ad:>> a\\/:*?\"| li*tt|le|| la\"mb.?";

            foreach (char c in Path.GetInvalidFileNameChars())
                illegal = illegal.Replace(c.ToString(), string.Empty);

            foreach (char c in Path.GetInvalidPathChars())
                illegal = illegal.Replace(c.ToString(), string.Empty);

            return illegal;
        }


        // copied from Validation ...
        private static bool IsValidDirectoryName(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                return false;
            char[] invChars = Path.GetInvalidPathChars();
            if (directoryName.IndexOfAny(invChars) > -1)
                return false;
            return true;
        }

        // used by DbControl
        public static long SumOfFileSizes(string root, string fileSearchPattern)
        {
            if (root == null)
                return 0;
            Debug.Assert(root.Length > 0, "SumOfFileSizes");
            if (!Directory.Exists(root))
                return 0;

            DirectoryInfo di = new DirectoryInfo(root);
            long result = 0;

            foreach (FileInfo fi in di.GetFiles(fileSearchPattern))
                result += fi.Length;

            foreach (DirectoryInfo subDir in di.GetDirectories())
                result += SumOfFileSizes(subDir.FullName, fileSearchPattern);
            return result;
        }


        public static bool TryGetDirectoryName(string path, out string directory)
        {
            directory = string.Empty;
            if (string.IsNullOrEmpty(path))
                return false;

            try
            {
                directory = System.IO.Path.GetDirectoryName(path);
                return true;
            }
            catch (System.ArgumentException ex)
            {
                ExceptionRecorder.RecordException("Argument exception", ex);
                return false;
            }
            catch (System.IO.PathTooLongException ex)
            {
                ExceptionRecorder.RecordException("PathTooLongException exception", ex);
                return false;
            }
        }

        public static string GetApplicationPath
        {
            get  { return AppContext.BaseDirectory;}
        }


        public static string GetJustDirectory(string fileWithFullPath)
        {
            try
            {
                if (string.IsNullOrEmpty(fileWithFullPath))
                    return string.Empty;

                return Directory.GetParent(fileWithFullPath).Name;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("Exception in GetJustDirectory " + fileWithFullPath, ex);
                return string.Empty;
            }
        }

        public static string GetDefaultObservationsDirectory
        {
            get { return Directory.GetDirectoryRoot(GetApplicationPath) + @"Observations\"; }
        }

        public static string GetParentDirectoryName(string directory)
        {
            try
            {
                if (string.IsNullOrEmpty(directory))
                    return string.Empty;
                if (!Directory.Exists(directory))
                    return string.Empty;

                return Directory.GetParent(directory).FullName;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in GetParentDirectoryName " + directory + "." + ex);
                return string.Empty;
            }
        }

        public static void SetAttributeOn(string fullPath, FileAttributes attributeToSet)
        {
            FileAttributes attributes = File.GetAttributes(fullPath);
            if ((attributes & attributeToSet) != attributeToSet)
                File.SetAttributes(fullPath, File.GetAttributes(fullPath) | attributeToSet);
        }

        public static void SetAttributeOff(string fullPath, FileAttributes attributeToRemove)
        {
            FileAttributes attributes = File.GetAttributes(fullPath);
            if ((attributes & attributeToRemove) == attributeToRemove)
            {
                attributes = RemoveAttribute(attributes, attributeToRemove);
                File.SetAttributes(fullPath, attributes);
            }
        }

        public static bool AttributeIsOn(string fullPath, FileAttributes attributeToTest)
        {
            FileAttributes attributes = File.GetAttributes(fullPath);
            return ((attributes & attributeToTest) == attributeToTest);
        }

        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }


        // return number of occurrences replaced
        public static void ReplaceInFile(string fullPath, string strToBeReplaced, string replacementString)
        {
            try
            {
                if (!File.Exists(fullPath))
                    return;

                string fileContents = string.Empty;
                using (StreamReader sr = File.OpenText(fullPath))
                {
                    fileContents = sr.ReadToEnd().Replace(strToBeReplaced, replacementString);
                }
                using (var file = System.IO.File.Create(fullPath))
                using (var sw = new System.IO.StreamWriter(file))
                {
                    sw.Write(fileContents);
                }
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in ReplaceInFile. ", ex);
            }
        }
    }
}
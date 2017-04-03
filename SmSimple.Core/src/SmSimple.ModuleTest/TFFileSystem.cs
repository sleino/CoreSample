using SmSimple.Core;
using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using Xunit;

namespace SmSimple.Core.ModuleTest
{
    public class TFFileSystem
    {

        [Fact]
        public void DirectoryCopy()
        {
            string sourceRoot = @"c:\temp\sourceRoot";
            if (Directory.Exists(sourceRoot))
                Directory.Delete(sourceRoot, true);


            FileSystem.TryCreateDirectory(sourceRoot);
            CreateTestFile(sourceRoot + @"\testFile1.txt");


            string sourceSub = @"c:\temp\sourceRoot\sub";
            FileSystem.TryCreateDirectory(sourceSub);
            CreateTestFile(sourceSub + @"\testFile2.txt");

            string targetRoot = @"c:\temp\targetRoot";
            if (Directory.Exists(targetRoot))
                Directory.Delete(targetRoot, true);

            FileSystem.CopyDirectory(sourceRoot, targetRoot);
            Assert.True(Directory.Exists(targetRoot));
            Assert.True(File.Exists(targetRoot + @"\testFile1.txt"));
            Assert.True(File.Exists(targetRoot + @"\sub\testFile2.txt"));

            Directory.Delete(sourceRoot, true);
            Directory.Delete(targetRoot, true);
        }

        private void CreateTestFile(string path)
        {
            using (StreamWriter file1 = File.CreateText(path))
            {
                file1.WriteLine("test_" + Path.GetRandomFileName());
            }
        }


        /*
         * 
         * Copy these files 
           c:\temp\sourceRoot\testFile.txt
           c:\temp\sourceRoot\testFile.dat
           c:\temp\sourceRoot\sub\testFile2.txt

            to
           c:\temp\targetRoot\testFile.txt
           c:\temp\targetRoot\testFile.dat
           c:\temp\targetRoot\sub\testFile2.txt
         */
        [Fact]
        public void DirectoryCopyWithPattern()
        {
            string sourceRoot = @"c:\temp\sourceRoot";
            if (Directory.Exists(sourceRoot))
                Directory.Delete(sourceRoot, true);

            FileSystem.TryCreateDirectory(sourceRoot);
            string testFileTxt = @"\" + Path.GetRandomFileName() + ".txt";
            string testFileDat = @"\" + Path.GetRandomFileName() + ".dat";
            CreateTestFile(sourceRoot + testFileTxt);
            CreateTestFile(sourceRoot + testFileDat);


            string sourceSub = @"c:\temp\sourceRoot\sub";
            FileSystem.TryCreateDirectory(sourceSub);
            string testFile2Txt = @"\" + Path.GetRandomFileName() + ".txt";
            CreateTestFile(sourceSub +  testFile2Txt);

            string targetRoot = @"c:\temp\targetRoot";
            if (Directory.Exists(targetRoot))
                Directory.Delete(targetRoot, true);

            FileSystem.CopyDirectory(sourceRoot, targetRoot, "*.txt");
            Assert.True(Directory.Exists(targetRoot));
            Assert.True(File.Exists(targetRoot + testFileTxt));
            Assert.True(File.Exists(targetRoot + @"\sub" + testFile2Txt));
            Assert.False(File.Exists(targetRoot + testFileDat));

            Directory.Delete(sourceRoot, true);
            Directory.Delete(targetRoot, true);
        }

        [Fact]
        public void FileSizeSum()
        {
            string sourceRoot = @"c:\temp\sourceRoot";
            if (Directory.Exists(sourceRoot))
                Directory.Delete(sourceRoot, true);

            FileSystem.TryCreateDirectory(sourceRoot);
            using (StreamWriter file1 = File.CreateText(sourceRoot + @"\testFile.txt"))
                file1.Write("The size of this file is 33 bytes");

            using (StreamWriter file3 = File.CreateText(sourceRoot + @"\testFile.dat"))
            {
                file3.Write("The size of this file is more than 33 bytes");
            }

            string sourceSub = @"c:\temp\sourceRoot\sub";
            FileSystem.TryCreateDirectory(sourceSub);
            using (StreamWriter file1 = File.CreateText(sourceRoot + @"\testFile2.txt"))
            { }

            long iSum = FileSystem.SumOfFileSizes(sourceRoot, "*.txt");
            Assert.True(iSum == 33);
        }

        [Fact]
        public void ApplicationPath()
        {
            string path = FileSystem.GetApplicationPath;
            Assert.True(path.Length > 0);
            Assert.True(Directory.Exists(path));
        }




        [Fact]
        public void ReplaceInFilesTest()
        {
            string sourceRoot = @"c:\temp\sourceRoot";
            if (Directory.Exists(sourceRoot))
                Directory.Delete(sourceRoot, true);

            FileSystem.TryCreateDirectory(sourceRoot);
            string fullFileName = sourceRoot + @"\testFile.txt";
            const string strToBeReplaced = "Vaisala Observation Console";
            using (var file1 = File.CreateText(fullFileName))
            {
                file1.WriteLine("Row 1");
                file1.WriteLine(strToBeReplaced);
             
            }

            FileSystem.ReplaceInFile(fullFileName, "Vaisala Observation Console", "New Name");
            using (StreamReader sr = File.OpenText(fullFileName))
            {
                string contents = sr.ReadToEnd();
               Assert.False(contents.Contains(strToBeReplaced));
            }
        }

        [Fact]
        public void FileAttributeTest()
        {
            string sourceRoot = @"c:\temp\sourceRoot";
            if (Directory.Exists(sourceRoot))
                Directory.Delete(sourceRoot, true);

            FileSystem.TryCreateDirectory(sourceRoot);
            string fullFileName = sourceRoot + @"\testFile.txt";
            using (StreamWriter file1 = File.CreateText(sourceRoot + @"\testFile.txt"))
            {
                file1.WriteLine("test " + DateTimeEx.NowToStringWithMs);
            }
             

            FileSystem.SetAttributeOn(fullFileName, FileAttributes.Archive);
            Assert.True(FileSystem.AttributeIsOn(fullFileName, FileAttributes.Archive));

            FileSystem.SetAttributeOn(fullFileName, FileAttributes.Archive);
            Assert.True(FileSystem.AttributeIsOn(fullFileName, FileAttributes.Archive));

            FileSystem.SetAttributeOff(fullFileName, FileAttributes.Archive);
           Assert.False(FileSystem.AttributeIsOn(fullFileName, FileAttributes.Archive));

            FileSystem.SetAttributeOff(fullFileName, FileAttributes.Archive);
           Assert.False(FileSystem.AttributeIsOn(fullFileName, FileAttributes.Archive));

            Directory.Delete(sourceRoot, true);
        }

        private void CreateTestDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            FileSystem.TryCreateDirectory(directory);
        }

        private void CreateTestFile(string directory, string fileName, int creationDateOffset)
        {
            string fullPath = directory + @"\" + fileName;
            using (StreamWriter file1 = File.CreateText(fullPath))
            {

            }

            DateTime dateTime = DateTimeEx.Now.AddDays(creationDateOffset);
            File.SetCreationTime(fullPath, dateTime);
            File.SetLastAccessTime(fullPath, dateTime);
            File.SetLastWriteTime(fullPath, dateTime);
        }


        [Fact]
        public void DirectoryHierarchyFileDeleteTest()
        {
            int creationDateOffset = -4;

            string testRoot = @"c:\temp\testRoot";
            CreateTestDirectory(testRoot);
            CreateTestFile(testRoot, "testFile0.txt", creationDateOffset);

            string testSub1 = @"c:\temp\testRoot\sub1";
            CreateTestDirectory(testSub1);
            CreateTestFile(testSub1, "testFile1.txt", creationDateOffset);

            string testSub2 = @"c:\temp\testRoot\sub2";
            CreateTestDirectory(testSub2);
            CreateTestFile(testSub2, "testFile2.txt", creationDateOffset);

            string testSub3 = @"c:\temp\testRoot\sub1\sub3";
            CreateTestDirectory(testSub3);
            CreateTestFile(testSub3, "testFile3.txt", creationDateOffset);

            string testSub4 = @"c:\temp\testRoot\sub1\sub4";
            CreateTestDirectory(testSub4);

            Assert.True(Directory.Exists(testRoot));
            Assert.True(Directory.Exists(testSub1));
            Assert.True(Directory.Exists(testSub2));
            Assert.True(Directory.Exists(testSub3));
            Assert.True(Directory.Exists(testSub4));

            string filter = "*.rep";
            int daysToKeep = 5;
            FileSystem.DeleteOldFilesFromSubDirectories(testRoot, filter, daysToKeep, true);

            Assert.True(Directory.Exists(testRoot));
            Assert.True(Directory.Exists(testSub1));
            Assert.True(Directory.Exists(testSub2));
            Assert.True(Directory.Exists(testSub3));
            Assert.True(!Directory.Exists(testSub4));

            filter = "*.txt";
            daysToKeep = 5;
            FileSystem.DeleteOldFilesFromSubDirectories(testRoot, filter, daysToKeep, true);
            Assert.True(Directory.Exists(testRoot));
            Assert.True(Directory.Exists(testSub1));
            Assert.True(Directory.Exists(testSub2));
            Assert.True(Directory.Exists(testSub3));
            Assert.True(!Directory.Exists(testSub4));

            filter = "*.txt";
            daysToKeep = 3;
            FileSystem.DeleteOldFilesFromSubDirectories(testRoot, filter, daysToKeep, true);
            Assert.True(!Directory.Exists(testRoot));
            Assert.True(!Directory.Exists(testSub1));
            Assert.True(!Directory.Exists(testSub2));
            Assert.True(!Directory.Exists(testSub3));
            Assert.True(!Directory.Exists(testSub4));
        }

        /*
        [Fact]
        public void FastDirectoryEnumeratorTest()
        {
            int creationDateOffset = -4;

            string testRoot = @"c:\temp\testRoot";
            CreateTestDirectory(testRoot);
            CreateTestFile(testRoot, "testFile0.txt", creationDateOffset);

            string testSub1 = @"c:\temp\testRoot\sub1";
            CreateTestDirectory(testSub1);
            CreateTestFile(testSub1, "testFile1.txt", creationDateOffset);

            string testSub2 = @"c:\temp\testRoot\sub2";
            CreateTestDirectory(testSub2);
            CreateTestFile(testSub2, "testFile2.txt", creationDateOffset);

            string testSub3 = @"c:\temp\testRoot\sub1\sub3";
            CreateTestDirectory(testSub3);
            CreateTestFile(testSub3, "testFile3.txt", creationDateOffset);

            string testSub4 = @"c:\temp\testRoot\sub1\sub4";
            CreateTestDirectory(testSub4);

            Assert.True(Directory.Exists(testRoot));
            Assert.True(Directory.Exists(testSub1));
            Assert.True(Directory.Exists(testSub2));
            Assert.True(Directory.Exists(testSub3));
            Assert.True(Directory.Exists(testSub4));

            FileData[] fileData = FastDirectoryEnumerator.GetFiles(testRoot, "*.txt", SearchOption.AllDirectories);
            Assert.True(fileData.GetLength(0) == 4);

            foreach (var curFileData in FastDirectoryEnumerator.GetFiles(testRoot, "*.txt", SearchOption.AllDirectories)
                )
            {
                Assert.True(curFileData.Name.Contains("testFile"));
            }
        }
        */
    }
}

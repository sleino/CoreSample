using SmSimple.Core;
using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using Xunit;
using System.Threading.Tasks;

namespace SmSimple.Core.ModuleTest
{
    public class TFDirectoryMaintenanceManager
    {

        [Fact]
        public void BasicTest()
        {
            Debug.WriteLine("BasicTest 1");
            string sourceRoot = @"c:\temp\sourceRoot";
            if (Directory.Exists(sourceRoot))
                Directory.Delete(sourceRoot, true);

            Console.WriteLine("BasicTest 2");
            var fileName = sourceRoot + @"\testFile1.txt";
            bool ok = FileSystem.TryCreateDirectory(sourceRoot);

            Assert.True(ok, "BasicTest TryCreateDirectory, fileName = " + fileName);
            CreateTestFile(fileName);

            Debug.WriteLine("BasicTest CreateTestFile");
        

            bool fileExists = File.Exists(fileName);
            Assert.True(fileExists, "BasicTest File.Exist");
            Debug.WriteLine("BasicTest File.Exist");

            using (var dmm = new DirectoryMaintenanceManager())
            {
                dmm.AddDirectory(sourceRoot, 0);
          
                Debug.WriteLine("BasicTest dmm.AddDirectory");

                Task.Delay(TimeSpan.FromSeconds(5)).Wait();
             
                fileExists = File.Exists(fileName);
                Assert.True(!fileExists);
                Debug.WriteLine("BasicTest !File.Exist");

                FileSystem.TryDeleteDirectory(sourceRoot);
            
            }
            Debug.WriteLine("BasicTest End");
        }


        [Fact]
        public void FileFilter()
        {
            string sourceRoot = @"c:\temp\sourceRoot";
            if (Directory.Exists(sourceRoot))
                Directory.Delete(sourceRoot, true);

            var fileName = sourceRoot + @"\testFile1.txt";
            FileSystem.TryCreateDirectory(sourceRoot);
            CreateTestFile(fileName);

            bool fileExists = File.Exists(fileName);
            Assert.True(fileExists);

            var dmm = new DirectoryMaintenanceManager();
            dmm.FileFilter = "*.csv";
            dmm.AddDirectory(sourceRoot, 0);

            System.Threading.Thread.Sleep(1000 * 5);


            fileExists = File.Exists(fileName);
            Assert.True(fileExists);
            Directory.Delete(sourceRoot, true);
        }


        private void CreateTestFile(string path)
        {
            using (StreamWriter file1 = File.CreateText(path))
            {
                file1.WriteLine("test_" + Path.GetRandomFileName());
            }
        }

    }
}

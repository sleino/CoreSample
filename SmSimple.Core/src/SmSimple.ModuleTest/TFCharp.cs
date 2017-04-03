using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;
using System;

namespace SmSimple.Core.ModuleTest
{
    public class TFCharp
    {

        [Fact]
        public void NullTest()
        {
            string s = null;
            string t = "This is t";
            string w = s ??t;
            Assert.True(w == "This is t");
            
        }


        [Fact]
        public void LoopTest1()
        {

            int max = 1000 * 1000;
            var collection = new List<string>();
            for (int i = 0; i < max; i++)
                collection.Add(i.ToIcString());


            var icollection = new List<int>();

            Console.WriteLine("Start");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < max; i++)
            {
                int.TryParse(collection[i], out int data);
                icollection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(icollection.Count == max);
            Console.WriteLine("End test 1. Duration: " + stopWatch.ElapsedTicks);

            icollection.Clear();
            stopWatch.Restart();

            for (int i = 0; i < max; i++)
            {
                int.TryParse(collection[i], out int data);
                icollection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(icollection.Count == max);
            Console.WriteLine("End test 2. Duration: " + stopWatch.ElapsedTicks);


            icollection.Clear();
            stopWatch.Restart();

            for (int i = 0; i < collection.Count; i++)
            {
                int.TryParse(collection[i], out int data);
                icollection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(icollection.Count == max);
            Console.WriteLine("End test 3. Duration: " + stopWatch.ElapsedTicks);
        }

        [Fact]
        public void LoopTest2()
        {
            Console.WriteLine(DateTimeEx.ToStdDateTimeFormatWithMs(DateTimeEx.NowNoMs) + "\tGetIntListSlowly()");
            List<int> list = GetIntListSlowly();
            int max = list.Count;

            List<string> collection = new List<string>();

            Console.WriteLine("Start");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < max; i++)
            {
                string data = list[i].ToIcString();
                collection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(collection.Count == max);
            Console.WriteLine("End test 1. Duration: " + stopWatch.ElapsedTicks);

            collection.Clear();
            stopWatch.Restart();

            for (int i = 0; i < GetIntListSlowly().Count; i++)
            {
                string data = list[i].ToIcString();
                collection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(collection.Count == max);
            Console.WriteLine("End test 2. Duration: " + stopWatch.ElapsedTicks);


            collection.Clear();
            stopWatch.Restart();

            foreach (int i in GetIntListSlowly())
            {
                string data = list[i].ToIcString();
                collection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(collection.Count == max);
            Console.WriteLine("End test 3. Duration: " + stopWatch.ElapsedTicks);

            collection.Clear();
            stopWatch.Restart();
            for (int i = 0; i < GetIntListSlowlyUsingGetter.Count; i++)
            {
                string data = list[i].ToIcString();
                collection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(collection.Count == max);
            Console.WriteLine("End test 4. Duration: " + stopWatch.ElapsedTicks);


            collection.Clear();
            stopWatch.Restart();

            foreach (int i in GetIntListSlowlyUsingGetter)
            {
                string data = list[i].ToIcString();
                collection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(collection.Count == max);
            Console.WriteLine("End test 5. Duration: " + stopWatch.ElapsedTicks);


            collection.Clear();
            stopWatch.Restart();

            var tmpList = GetIntListSlowlyUsingGetter;
            foreach (int i in tmpList)
            {
                string data = list[i].ToIcString();
                collection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(collection.Count == max);
            Console.WriteLine("End test 6. Duration: " + stopWatch.ElapsedTicks);


            collection.Clear();
            stopWatch.Restart();


            foreach (int i in tmpList)
            {
                string data = list[i].ToIcString();
                collection.Add(data);
            }
            stopWatch.Stop();
            Assert.True(collection.Count == max);
            Console.WriteLine("End test 6b. Duration: " + stopWatch.ElapsedTicks);


        }

        private List<int> GetIntListSlowly()
        {
          
            var result = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Thread.Sleep(100);
            return result;
        }

        private List<int> GetIntListSlowlyUsingGetter
        {
            get
            {
                var result = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                Thread.Sleep(100);
                return result;
            }
        }



    }
}

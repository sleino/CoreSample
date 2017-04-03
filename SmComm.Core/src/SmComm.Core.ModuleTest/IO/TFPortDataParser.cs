using SmComm.Core.IO;
using SmSimple.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace SmComm.Core.ModuleTest.IO
{
    public class TFPortDataParser
    {

        private readonly List<string> expectedData = new List<string>();

       
        [Fact]
        public void TestAppendAndParse()
        {

            NamedPortEventRaiser.VirtualPortEvent += VirtualPortEventRaiser_VirtualPortEvent;

            var stopWatch2 = new Stopwatch();

            var tcpIpClient = new VaiTcpIpClient();
            var portDataParser = new PortDataParser(tcpIpClient);

          
            
          stopWatch2.Start();
          RunNonCheckSumTests(portDataParser, portDataParser.AppendAndParse);
          stopWatch2.Stop();
          Debug.WriteLine("Stopwatch2:" + stopWatch2.ElapsedMilliseconds);


          portDataParser.UseCrc32 = true;

          stopWatch2.Start();
          RunCheckSumTests(portDataParser, portDataParser.AppendAndParse);
          stopWatch2.Stop();
          Debug.WriteLine("Stopwatch2:" + stopWatch2.ElapsedMilliseconds);
         

        }
     

        private void RunNonCheckSumTests(PortDataParser portDataParser, Func<string, bool> testMethod)
        {
            expectedData.Clear();
            expectedData.Add(string.Empty);

            expectedData[0] = "(test message)";
            var ok = testMethod(expectedData[0]);
            Assert.True(ok);

            ok = testMethod(expectedData[0]);
            Assert.True(ok);

            expectedData[0] = ")";
            expectedData.Add("()");
            ok = testMethod(")()");
            Assert.True(ok);

            expectedData[0] = "";
            expectedData[1] = "";

            expectedData[0] = "(test message)";
            ok = testMethod("(test mess");
            Assert.True(ok);
            ok = testMethod("age) sdfsdf ( te");
            Assert.True(ok);

            expectedData[0] = " sdfsdf ( test message 2)";
            ok = testMethod("st message 2)sdffsadfsda");
            Assert.True(ok);

            portDataParser.ClearDataReceptionBuffer();

            expectedData[0] = ")";
            expectedData[1] = "(2134)";
            ok = testMethod(")");
            Assert.True(ok);
            ok = testMethod("(2");
            Assert.True(ok);
            ok = testMethod("134)");
            Assert.True(ok);
            
        }

        private void RunCheckSumTests(PortDataParser portDataParser, Func<string, bool> testMethod)
        {
            expectedData.Clear();
            expectedData.Add(string.Empty);
            expectedData.Add(string.Empty);


            expectedData[0] = "(test message)12345678\r\n";
            var ok = testMethod(expectedData[0]);
            Assert.True(ok);

            ok = testMethod(expectedData[0]);
            Assert.True(ok);

            expectedData[0] = "(test message)12345678\r\n";
            expectedData[1] = ")";
            ok = testMethod(")(test message)12345678\r\n");
            Assert.True(ok);

            expectedData[0] = "";
            expectedData[1] = "";

            expectedData[0] = "(test message)12345678\r\n";
            ok = testMethod("(test mess");
            ok = testMethod("age)12345678\r\n sdfsdf ( te");
            Assert.True(ok);

            expectedData[0] = " sdfsdf ( test message 2)12345678\r\n";
            ok = testMethod("st message 2)12345678\r\nsdffsadfsda");
            Assert.True(ok);

            portDataParser.ClearDataReceptionBuffer();

            expectedData[0] = ")12345678\r\n";
            expectedData[1] = "(2134)12345678\r\n";
            ok = testMethod(")12345678\r\n");
            Assert.True(ok);
            ok = testMethod("(2");
            Assert.True(ok);
            ok = testMethod("134)12345678\r\n");
            Assert.True(ok);
        }

        private void VirtualPortEventRaiser_VirtualPortEvent(object sender, NamedPortEventArgs e)
        {
            try
            {
                if (e.PortEvent != NamedPortEventArgs.PortEventType.DataWithEndCharReceived)
                    return;

                Debug.WriteLine("DataReceived:" + e.PortEvent + ":" + e.Text);

                char[] trimChars = { '\r', '\n' };

                string data = e.Text.TrimStart(trimChars);
                var ok = false;
                foreach (var dataValue in expectedData)
                    if (dataValue == data || dataValue.TrimEnd(trimChars) == data)
                    {
                        ok = true;
                        break;
                    }
                if (!ok)
                    Debug.Assert(true);
                Assert.True(ok, "Unexpected text received:" + e.Text);
            }
            catch (Exception ex)
            {
                string message = "VirtualPortEventRaiser_VirtualPortEvent:" + (e.Text?? "e.Text==null");
                ExceptionHandler.HandleException(ex, message);
            }
        }

    }
}

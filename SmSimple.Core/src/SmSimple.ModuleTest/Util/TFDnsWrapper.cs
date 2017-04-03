using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;
using System;
using SmSimple.Core.Util;

namespace SmSimple.Core.ModuleTest
{
    public class TFDnsWrapper
    {

       [Fact]
        public void TestAddressList()
        {
            var hostName = DnsWrapper.GetHostName;
            Assert.True(!String.IsNullOrWhiteSpace(hostName));

            var addresses = DnsWrapper.GetHostIp4Addresses(hostName) ;
        

            Assert.True(addresses.Count>0);
        }
   


       [Fact]
        public void GetPrimaryIp()
        {
            string ip = DnsWrapper.GetPrimaryIp;
            Assert.True(ip.Length > 0);
        }

        [Fact]
        public void GetSecondaryIp()
        {
            string ip = DnsWrapper.GetSecondaryIp;
            Assert.True(ip.Length > 0);
        }
        
    }
}

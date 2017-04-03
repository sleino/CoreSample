using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SmSimple.Core.Util
{
    public static class DnsWrapper
    {
        private static IPAddress[] GetAddresses(string hostName) {
            var task = Dns.GetHostEntryAsync(hostName);
            IPAddress[] addresses = task.Result.AddressList;
            return addresses;
        }

        public static List<IPAddress> GetHostAddresses(string hostName)
        {
            try
            {
                IPAddress[] addresses = GetAddresses(hostName);
                return addresses.ToList<IPAddress>();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("Exception in HostNameToIP ", ex);
                return null;
            }
        }

       
        public static List<IPAddress> GetHostIp4Addresses(string hostName)
        {
            try
            {
                List<IPAddress> result = new List<IPAddress>();

                IPAddress[] addresses = GetAddresses(hostName);
                foreach (var address in addresses) {
                   if (!address.ToString().Contains(":"))
                        result.Add(address);
                    }
                return result;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("Exception in HostNameToIP ", ex);
                return null;
            }
        }
        

        public static string GetHostName
        {
            get
            {
                try
                {
                    return Dns.GetHostName();
                }
                catch (Exception ex)
                {
                    ExceptionRecorder.RecordException("Exception in Dns.GetHostName", ex);
                    return string.Empty;
                }
            }
        }


        public static string GetPrimaryIp
        {
            get { return GetNthIpAddress(0); }
        }

        public static int GetIpAddressCount
        {
            get
            {
                return GetAddresses(string.Empty).GetLength(0);
            }
        }

        public static string GetSecondaryIp
        {
            get { return GetNthIpAddress(1); }
        }

        public static bool HasIpAddress(string ipAdress)
        {
            IPAddress[] address = GetAddresses(string.Empty);
            for (var index = 0; index < address.GetLength(0); index++)
            {
                if (address[index].AddressFamily == AddressFamily.InterNetwork)
                    if (address[index].ToString() == ipAdress)
                        return true;
            }
            return false;
        }

        public static IList<string> GetIpAddresses()
        {
            var result = new List<string>();

            var addresses = GetHostAddresses(string.Empty);
            int max = addresses.Count;
            for (var index = 0; index < max; index++)
            {
                if (addresses[index].AddressFamily == AddressFamily.InterNetwork)
                    result.Add(addresses[index].ToString());
            }
            result.Sort();
            return result;
        }


        private static string GetNthIpAddress(int n)
        {
            const string defaultIp = "127.0.0.1";
            try
            {
                // SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "GetNthIpAddress n=" + n);

                var list = GetHostAddresses(Dns.GetHostName());
                int numAddresses = list.Count;
                // SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "address:" + address);

                for (var newIndex = n; newIndex < numAddresses; newIndex++)
                {
                    if (list[newIndex].AddressFamily == AddressFamily.InterNetwork)
                        return list[newIndex].ToString();
                }

                return defaultIp;
                /*
                int index = n - 1;
                if (index >= numAddresses )
                    return GetNthIpAddress(index);
    
                if (address[index].AddressFamily == AddressFamily.InterNetwork)
                    return address[index].ToString();

                return GetNthIpAddress(index);
                 * */
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception while getting  ip address. n=" + n + " . " + ex.Message);
                return defaultIp;
            }
        }

    }
}

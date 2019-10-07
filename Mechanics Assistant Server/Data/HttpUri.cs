using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace MechanicsAssistantServer.Data
{
    public class HttpUri
    {
        public string Hostname { get; set; }
        public string Port { get; set; }
        public string Location { get; set; }
        public string Protocol { get; set; }

        public bool UsedDefaultPort { get; set; } = false;

        public string Prefix { get
            {
                if (UsedDefaultPort)
                    return Protocol + Hostname + Location;
                else
                    return Protocol + Hostname + ":" + Port + Location;
            }
        }

        public HttpUri(string uriString)
        {
            ParseUri(uriString);
        }

        private void ParseUri(string uriString)
        {
            if (!uriString.Contains("//"))
                throw new UriFormatException("Protocol not found. Missing double forward slashes");
            string uriParse = uriString;
            if (!uriParse.EndsWith('/'))
                uriParse += '/';
            int doubleSlashIndex = uriParse.IndexOf("//") + 2;
            string protocolString = uriParse.Substring(0, doubleSlashIndex);
            if(!(protocolString.Equals("http://") || protocolString.Equals("https://")))
                throw new UriFormatException("Unrecognized protocol: " + protocolString);
            Protocol = protocolString;
            string defaultPort = "80";
            if (Protocol.Equals("https://"))
                defaultPort = "443";
            string hostname = uriParse.Substring(
                doubleSlashIndex, uriParse.IndexOf("/", doubleSlashIndex) - doubleSlashIndex
                );
            if(hostname.Contains(':'))
            {
                var split = hostname.Split(':');
                Port = split[1];
                Hostname = split[0];
            } else
            {
                Port = defaultPort;
                UsedDefaultPort = true;
                Hostname = hostname;
            }
            Location = uriParse.Substring(uriParse.IndexOf("/", doubleSlashIndex));
        }

        public bool IsPrefixOf(HttpUri other)
        {
            if (!(Hostname.Equals("*") || Hostname.Equals("+")))
                if (!Hostname.Equals(other.Hostname))
                    return false;
            if (!Port.Equals(other.Port))
                return false;
            if (UsedDefaultPort ^ other.UsedDefaultPort)
                return false;
            if (!other.Location.StartsWith(Location))
                return false;
            return true;
        }
    }
}

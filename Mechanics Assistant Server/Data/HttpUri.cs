using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace OldManinTheShopServer.Data
{
    /**
     * <summary>Class that represents an Http protocol URI with support for the prefixes required by an HttpListener</summary>
     */
    public class HttpUri
    {
        /** <summary>Hostname portion of the uri</summary> */
        public string Hostname { get; set; }
        /** <summary>Port portion of the uri</summary> */
        public string Port { get; set; }
        /** <summary>Location portion of the uri, aka the portion after the first hostname:port part of the screen</summary> */
        public string Location { get; set; }
        /** <summary>Protocol portion of the uri</summary> */
        public string Protocol { get; set; }
        /** <summary>Boolean of whether the default port for the protocol was used as a fallback</summary> */
        public bool UsedDefaultPort { get; set; } = false;

        /** <summary>Returns a prefix representation of the URI for use with an HttpListener</summary> */
        public string Prefix { get
            {
                if (UsedDefaultPort)
                    return Protocol + Hostname + Location;
                else
                    return Protocol + Hostname + ":" + Port + Location;
            }
        }

        /** <summary>Constructs an HttpUri from the provided string</summary>
         * <param name="uriString">String to convert into an HttpUri</param>
         */
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

        /** <summary>Returns true if this uri is a prefix for the specified uri</summary>
         * <param name="other">The specified uri</param>
         */
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

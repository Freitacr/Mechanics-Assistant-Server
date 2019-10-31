using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    class MysqlDataConvertingUtil
    {
        /// <summary>
        /// Converts the byte array into a MySql binary literal
        /// </summary>
        /// <param name="a">The byte array to convert</param>
        /// <returns>A string representing the MySql binary literal that corresponds to the byte array passed in</returns>
        public static string ConvertToHexString(byte[] a)
        {
            if (a == null)
                return "x'00'";
            StringBuilder ret = new StringBuilder("x'");
            foreach (byte b in a)
            {
                if (b < 16)
                    ret.Append("0" + Convert.ToString(b, 16));
                else
                    ret.Append(Convert.ToString(b, 16));
            }
            ret.Append("'");
            return ret.ToString();
        }
    }
}

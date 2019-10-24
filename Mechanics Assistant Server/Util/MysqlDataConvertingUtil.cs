using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    class MysqlDataConvertingUtil
    {
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

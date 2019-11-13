using System;
using System.Collections.Generic;
using System.Security;
using System.Runtime.InteropServices;
using System.Text;

namespace OldManInTheShopServer.Util
{
    public static class SecureStringUtils
    {

        public static string ConvertToString(this SecureString secureString)
        {
            if (secureString == null)
                throw new ArgumentNullException("Secure String was null");

            IntPtr unmanagedStr = IntPtr.Zero;
            try
            {
                unmanagedStr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedStr);
            } finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedStr);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    class AccessLevelMasks
    {
        public static readonly byte PartMask = 0x4;
        public static readonly byte AdminMask = 0x8;
        public static readonly byte SafetyMask = 0x2;
        public static readonly byte MechanicMask = 0x1;
    }
}

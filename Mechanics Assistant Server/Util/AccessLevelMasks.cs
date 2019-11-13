using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    /// <summary>
    /// Bit masks that determine what privilages and roles a user has
    /// </summary>
    class AccessLevelMasks
    {
        public static readonly byte PartMask = 0x4;
        public static readonly byte AdminMask = 0x8;
        public static readonly byte SafetyMask = 0x2;
        public static readonly byte MechanicMask = 0x1;
    }
}

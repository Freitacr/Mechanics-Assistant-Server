﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using OMISSecLib;

namespace OldManInTheShopServer.Util
{
    class SecurityLibWrapper
    {
        public static readonly SecuritySchemaLib SecLib = new SecuritySchemaLib(SHA512.Create().ComputeHash);
    }
}

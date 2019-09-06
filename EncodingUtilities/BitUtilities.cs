using System;
using System.Collections.Generic;
using System.Text;

namespace EncodingUtilities
{
    public class BitUtilities
    {
        public static int GetNumberBitsToRepresent(int value)
        {
            for (int i = 31 ; i >= 0; i--)
                if ((value & (1u << i)) != 0)
                    return i+1;
            return 1;
        }

        public static int GetNumberBitsToRepresent(uint value)
        {
            for (int i = 31; i >= 0; i--)
                if ((value & (1u << i)) != 0)
                    return i + 1;
            return 1;
        }
    }
}

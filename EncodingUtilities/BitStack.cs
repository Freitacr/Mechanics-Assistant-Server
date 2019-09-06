using System;
using System.Collections.Generic;
using System.Text;

namespace EncodingUtilities
{
    public class BitStack
    {
        public uint Underlying { get; private set; }
        public int CurrentPos { get; private set; } = 0;
        public bool ByteFull { get { return CurrentPos == 8; } }
        public bool Empty { get { return CurrentPos == 0; } }
        public void PushBit(bool bitIn)
        {
            if (CurrentPos == 32)
                throw new InvalidOperationException("Cannot push bit onto stack, stack is full");
            if (bitIn)
                Underlying = (Underlying | (1u << CurrentPos));
            CurrentPos++;
        }

        public bool PopBit()
        {
            if (CurrentPos == 0)
                throw new InvalidOperationException("Cannot pop bit from stack; stack is empty");
            bool ret = (Underlying & (1u << CurrentPos)) != 0;
            CurrentPos--;
            return ret;
        }

        public void Clear()
        {
            Underlying = 0;
            CurrentPos = 0;
        }
    }
}

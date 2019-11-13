using System;
using System.Collections.Generic;
using System.Text;
using EncodingUtilities;

namespace ANSEncodingLib
{
    /**
     * INDEXING IS LITTLE ENDIAN
     */
    public class AnsState
    {
        public uint Underlying;
        private int Capacity;
        private ReversedMemoryStream Output;
        private BitStack BitStack;
        public int Contained { get { return CalculateContainedBits(); } }

        public bool this[int index]
        {
            get
            {
                uint mask = 1u << index;
                return (Underlying & mask) != 0;
            }
            set
            {
                uint mask = ~(1u << index);
                Underlying &= mask;
                uint toPut = (value ? 1u : 0u) << index;
                Underlying |= toPut;
            }
        }

        public AnsState(int capacity, ReversedMemoryStream outputStream, uint initialState)
        {
            Underlying = initialState;
            Capacity = capacity;
            //Contained = Capacity;
            Output = outputStream;
            BitStack = new BitStack();
        }

        private int CalculateContainedBits()
        {
            return BitUtilities.GetNumberBitsToRepresent((int)Underlying);
        }

        public void PopLower()
        {
            //if (Contained == 0)
            //    throw new InvalidOperationException("State is empty, cannot pop lower");
            OutputBit(this[0]);
            Underlying >>= 1;
        }

        public void PushLower(bool bitIn)
        {
            Underlying <<= 1;
            if (bitIn)
                Underlying |= 1;
            this[Capacity] = false;
        }

        public int ToInt()
        {
            return (int)Underlying;
        }

        private void OutputBit(bool bitIn)
        {
            Output.WriteBit(bitIn);
        }

        public void Flush()
        {
            int i = 0;
            int toWrite = Contained;
            for (; i < toWrite; i++)
            {
                OutputBit(this[0]);
                Underlying >>= 1;
            }
            Underlying = 0;
            BitStack.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SharpJoycon.Utilities
{
    // just utilities for handling nibbles
    public static class Nibble
    {
        public static byte[] Split(byte b)
        {
            byte[] bs = new byte[2];
            bs[0] = (byte) (b & 0x0F);
            bs[1] = (byte)((b & 0xF0) >> 4);
            return bs;
        }

        public static byte Combine(byte b1, byte b2)
        {
            return (byte)((b1 << 4) | b2);
        }

        public static byte Combine(byte[] bs)
        {
            return Combine(bs[0], bs[1]);
        }
    }
}

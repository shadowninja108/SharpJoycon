using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joycon_Glue.Source
{
    public abstract class RandomAccess
    {
        public abstract bool CanRead { get; }
        public abstract bool CanWrite { get; }
        public abstract int Size { get; }
        public abstract bool IsOpen { get; }

        public abstract byte[] Read(int address, int length);

        public byte Read(int address)
        {
            return Read(address, 1)[0];
        }

        public bool ReadBoolean(int address)
        {
            byte data = Read(address);
            return Convert.ToBoolean(data);
        }
        public bool ReadBit(int address, byte index)
        {
            byte data = Read(address);
            BitArray bits = new BitArray(data);
            return bits[index];
        }
        public char ReadChar(int address)
        {
            return ReadString(address, 1).First();
        }
        public string ReadString(int address, int length)
        {
            byte[] data = Read(address, length * 2);
            return Encoding.UTF8.GetString(data);
        }
        public Int16 ReadInt16(int address)
        {
            byte[] data = Read(address, 2);
            return BitConverter.ToInt16(data, 0);
        }

        public Int32 ReadInt32(int address)
        {
            byte[] data = Read(address, 4);
            return BitConverter.ToInt32(data, 0);
        }

        public Int64 ReadInt64(int address)
        {
            byte[] data = Read(address, 8);
            return BitConverter.ToInt64(data, 0);
        }
        public UInt16 ReadUInt16(int address)
        {
            return Convert.ToUInt16(ReadInt16(address));
        }
        public UInt32 ReadUInt32(int address)
        {
            return Convert.ToUInt32(ReadInt32(address));
        }
        public UInt64 ReadUInt64(int address)
        {
            return Convert.ToUInt64(ReadInt64(address));
        }


        //TODO: write access

        public abstract void Dispose();
    }
}

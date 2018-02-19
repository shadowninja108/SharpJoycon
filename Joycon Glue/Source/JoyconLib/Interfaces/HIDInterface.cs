using HidLibrary;
using System;
using System.Linq;


namespace SharpJoycon.Interfaces
{
    public class HIDInterface : AbstractInterface
    {
        private HidDevice hid;

        public HIDInterface(NintendoController controller) : base(controller)
        {
            hid = controller.GetHid();
        }

        public void Write(byte[] bytes)
        {
            hid.Write(bytes);
        }

        public PacketData ReadPacket()
        {

            PacketData packet = new PacketData();
            byte[] data = ReadData();
            packet.header = data.Take(15).ToArray();
            packet.data = data.Skip(15).ToArray();

            return packet;
        }

        public byte[] ReadData()
        {
           return hid.Read().Data;
        }

        public String GetSerialNumber()
        {
            byte[] bytes;
            hid.ReadSerialNumber(out bytes);
            return BytesToString(bytes);
        }

        public string GetProductString()
        {
            byte[] bytes;
            hid.ReadProduct(out bytes);
            return BytesToString(bytes);
        }

        public string GetManufacturerString()
        {
            byte[] bytes;
            hid.ReadManufacturer(out bytes);
            return BytesToString(bytes);
        }

        private string BytesToString(byte[] input)
        {
            string str = "";
            foreach (byte b in input)
            {
                if (b > 0)
                    str += ((char)b).ToString();
            }
            return str;
        }

        public override void Poll(HIDInterface.PacketData data)
        {
            // nothing to read
        }

        public struct PacketData
        {
            public byte[] header, data;
        }
    }
}

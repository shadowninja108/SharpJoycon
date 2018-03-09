using HidSharp;
using System;
using System.Linq;
using System.Threading;

namespace SharpJoycon.Interfaces
{
    public class HIDInterface : AbstractInterface
    {
        private HidDevice hid;
        private HidStream stream;

        public HIDInterface(NintendoController controller) : base(controller)
        {
            hid = controller.GetHid();
            hid.TryOpen(out stream);
        }

        public void Write(byte[] bytes)
        {
            // can't use regular write operations?
            // this works so don't argue
            stream.WriteAsync(bytes, 0, bytes.Length);

            //i can't even do it syncronously???
            //prob a race condition but whatever
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
            stream.ReadTimeout = Timeout.Infinite; // eh why not
            return stream.Read();
        }

        public String GetSerialNumber()
        {
            return hid.GetSerialNumber();
        }

        public string GetProductString()
        {
            return hid.GetProductName();
        }

        public string GetManufacturerString()
        {
            return hid.GetManufacturer();
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

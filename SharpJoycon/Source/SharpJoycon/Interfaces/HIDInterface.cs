using HidSharp;
using System;
using System.Diagnostics;
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
            hid = controller.GetRawHID();
            hid.TryOpen(out stream);
            // not the best but at least it doesn't crash when it decides to take its time
            stream.ReadTimeout = Timeout.Infinite;
            stream.WriteTimeout = Timeout.Infinite;
        }

        public void Write(byte[] bytes)
        {
            // truncate to fit into the max output report length
            stream.Write(bytes.Take(hid.GetMaxOutputReportLength()).ToArray());
        }

        public PacketData ReadPacket()
        {
            return new PacketData(ReadData());
        }

        public byte[] ReadData()
        {
            byte[] buffer = new byte[hid.GetMaxInputReportLength()];
           stream.Read(buffer, 0, buffer.Length);
            return buffer;
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

        public override void Poll(PacketData data)
        {
            // nothing to read
        }

        public class PacketData
        {

            public readonly byte[] rawData;

            public byte[] Header => rawData.Take(15).ToArray();
            public byte[] Data => rawData.Skip(15).ToArray();

            public PacketData(byte[] packet)
            {
                rawData = packet;
            }

        }
    }
}

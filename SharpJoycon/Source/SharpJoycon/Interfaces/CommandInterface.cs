using SharpJoycon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces
{
    public class CommandInterface : AbstractInterface
    {
        private static readonly Func<PacketData, bool> AlwaysTrue = _ => true;

        private byte packetNumber = 0;
        private HIDInterface hid;

        public CommandInterface(NintendoController controller) : base(controller)
        {
            hid = controller.GetHID();
        }

        /* Will truncate if data is more than 0x40 bytes */
        public PacketData SendCommand(byte command, byte[] data = null)
        {
            data = data ?? new byte[0];
            List<byte> output = new List<byte>();
            output.Add(command);
            output.AddAll(data.Take(0x40));
            switch (controller.connectionType)
            {
                case NintendoController.ConnectionType.USB:
                    if (command != 0x80) // 0x80 indicates raw UART command, so it should be left alone. (hopefully a 0x80 command isn't found)
                    {
                        List<byte> header = new List<byte>();
                        header.AddAll(new byte[] { 0x80, 0x92 });
                        header.AddAll(BitConverter.GetBytes(Math.Min(0x40, data.Length)).Skip(2));
                        header.Fill<byte>(0, 4);
                        //output.InsertAll(0, header);
                        //encode into USB UART command
                        //0x8092 + size of packet(16-bit) + padding + packet
                    }
                    break;
            }
            hid.Write(output.ToArray());
            return hid.ReadPacket();
        }

        public PacketData SendSubcommand(byte command, byte subCommand, byte[] data = null)
        {
            return SendSubcommandAsync(command, subCommand, data).Result;
        }

        public async Task<PacketData> SendSubcommandAsync(byte command, byte subCommand, byte[] data = null, Func<PacketData, bool> verify = null)
        {
            data = data ?? new byte[0];
            verify = verify ?? AlwaysTrue;
            List<byte> output = new List<byte>();
            output.Add(packetNumber);
            output.Fill<byte>(0, 8); // rumble data (unimplemented)
            output.Add(subCommand);
            output.AddAll(data);

            return await Task.Run(() =>
            {
                PacketData packet;
                while (true)
                {
                    packet = SendCommand(command, output.ToArray());
                    int id = packet.rawData[14];
                    if (id == subCommand)
                        if(verify(packet))
                            break;
                }
                IncreasePacketNumber();
                return packet;
            });
        }

        private void IncreasePacketNumber()
        {
            packetNumber++;
            if (packetNumber > 0xF)
                packetNumber = 0;
        }


        public override void Poll(PacketData data)
        {
            // nothing to read
        }
    }
}

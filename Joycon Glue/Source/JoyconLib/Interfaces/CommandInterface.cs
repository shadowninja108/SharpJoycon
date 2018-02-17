using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HIDInterface;

namespace Joycon_Glue.Source.Joystick.Controllers.Interfaces
{
    public class CommandInterface : AbstractInterface
    {
        private byte packetNumber = 0;
        private HIDInterface hid;

        public CommandInterface(NintendoController controller) : base(controller)
        {
            hid = controller.GetHID();
        }

        /* Will truncate if data is more than 0x39 bytes */
        public PacketData SendCommand(byte command, byte[] data, int len)
        {
            data = data ?? new byte[0];
            byte[] output = new byte[0x40];
            output[0] = command;
            len = Math.Min(0x39, len);
            Array.Copy(data, 0, output, 1, len);
            hid.Write(output);
            return hid.ReadPacket();
        }

        public PacketData SendSubcommand(byte command, byte subCommand, byte[] data)
        {
            data = data ?? new byte[0];
            byte[] output = new byte[0x40];
            output[0] = packetNumber;
            // rumble data (unimplemented)
            output[9] = subCommand;
            Array.Copy(data, 0, output, 10, data.Length);

            PacketData packet =  SendCommand(command, output, 10 + data.Length);
            IncreasePacketNumber();
            return packet;
        }


        private void IncreasePacketNumber()
        {
            packetNumber++;
            if (packetNumber < 0xF)
                packetNumber = 0;
        }


        public override void Poll(PacketData data)
        {
            // nothing to read
        }
    }
}

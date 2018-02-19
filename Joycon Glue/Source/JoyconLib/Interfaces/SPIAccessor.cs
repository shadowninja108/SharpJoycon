using Joycon_Glue.Source.Joystick.Controllers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HIDInterface;

namespace Joycon_Glue.Source.JoyconLib.Interfaces
{
    public class SPIAccessor : RandomAccess
    {

        static int readLimit = 0x1D;

        private NintendoController controller;
        private CommandInterface command;
        private HIDInterface hid;

        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override int Size => 0x10000 + 0x70000;
        public override bool IsOpen => true;

        public SPIAccessor(NintendoController controller)
        {
            this.controller = controller;
            command = controller.GetCommands();
            hid = controller.GetHID();
        }

        // should support arbitrary read lengths
        public override byte[] Read(int address, int length)
        {
            decimal reads = Math.Ceiling(((decimal)length / readLimit));
            byte[] data = new byte[length];
            byte reportMode = controller.GetHardware().GetReportMode();
            HardwareInterface hardware = controller.GetHardware();
            hardware.SetReportMode(0x3F); // calm down the packets down to speed up read
            hardware.SetIMU(false);
            for (int i = 0; i < reads; i++)
            {
                int readOffset = i * readLimit;
                int readAddress = address + readOffset;
                int readLength = Math.Min(length - readOffset, readLimit);
                List<byte> outputBytes = new List<byte>(BitConverter.GetBytes(readAddress));
                outputBytes.Add((byte) length);
                byte[] output = outputBytes.ToArray();
                PacketData packet;
                while(true)
                {
                    // spam because why not?
                    packet = command.SendSubcommand(0x1, 0x10, output);
                    if (output.SequenceEqual(packet.data.Take(output.Length)))
                        break;
                }
                Array.Copy(packet.data.Skip(5).ToArray(), 0, data, readOffset, readLength);
            }
            hardware.SetIMU(true);
            hardware.SetReportMode(reportMode); // returned to whatever mode was before read
            return data;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

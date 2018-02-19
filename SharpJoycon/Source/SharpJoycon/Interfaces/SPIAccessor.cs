using System;
using System.Collections.Generic;
using System.Linq;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces.SPI
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
            for (int i = 0; i < reads; i++)
            {
                int readOffset = i * readLimit;
                int readAddress = address + readOffset;
                int readLength = Math.Min(length - readOffset, readLimit);
                List<byte> outputBytes = new List<byte>(BitConverter.GetBytes(readAddress));
                outputBytes.Add((byte) length);
                byte[] output = outputBytes.ToArray();
                PacketData packet;
                Console.WriteLine("Attempting SPI read...");
                int attempts = 0;
                while(true)
                {
                    // spam because why not?
                    attempts++;
                    packet = command.SendSubcommand(0x1, 0x10, output);
                    if (output.SequenceEqual(packet.data.Take(output.Length)))
                        break;
                }
                Console.WriteLine($"SPI read took {attempts} attempts");
                Array.Copy(packet.data.Skip(5).ToArray(), 0, data, readOffset, readLength);
            }
            return data;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

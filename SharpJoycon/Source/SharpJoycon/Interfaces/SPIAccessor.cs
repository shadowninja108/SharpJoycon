using System;
using System.Collections.Generic;
using System.IO;
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
        public override byte[] Read(int address, int length, string file = null)
        {
            decimal reads = Math.Ceiling(((decimal)length / readLimit));
            byte[] data = null;
            FileStream stream = null;
            if (file != null)
                stream = new FileStream(file, FileMode.Open);
            else
                data = new byte[length];
            for (int i = 0; i < reads; i++)
            {
                int readOffset = i * readLimit;
                int readAddress = address + readOffset;
                int readLength = Math.Min(length - readOffset, readLimit);
                List<byte> outputBytes = new List<byte>(BitConverter.GetBytes(readAddress));
                outputBytes.Add((byte) readLength);
                byte[] output = outputBytes.ToArray();
                PacketData packet;
                Console.WriteLine($"Attempting SPI read ({i+1}/{reads})...");
                int attempts = 0;
                while(true)
                {
                    // spam cause the input reports clutter everything
                    // switching modes also takes time meaning switching it would cost more time
                    attempts++;
                    packet = command.SendSubcommand(0x1, 0x10, output);
                    if (output.SequenceEqual(packet.data.Take(output.Length)))
                        break;
                }
                Console.WriteLine($"SPI read took {attempts} attempt{(attempts==1 ? "" : "s")}"); // lol grammar
                byte[] readData = packet.data.Skip(5).ToArray();
                if (stream == null)
                    Array.Copy(readData, 0, data, readOffset, readLength);
                else
                {
                    stream.Seek(readOffset, SeekOrigin.Begin);
                    stream.Write(readData, 0, readData.Length);
                }
            }
            if (stream != null)
                stream.Close();
            return data;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

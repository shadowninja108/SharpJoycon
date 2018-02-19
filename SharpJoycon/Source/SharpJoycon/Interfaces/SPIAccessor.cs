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
            BinaryWriter writer = null;
            if (file != null)
                writer = new BinaryWriter(new FileStream(file, FileMode.Open));
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
                Console.WriteLine($"Attempting SPI read ({i}/{reads})...");
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
                byte[] readData = packet.data.Skip(5).ToArray();
                if (writer == null)
                    Array.Copy(readData, 0, data, readOffset, readLength);
                else
                {
                    writer.Seek(readOffset, SeekOrigin.Begin);
                    writer.Write(readData, 0, readData.Length);
                }
            }
            if (writer != null)
                writer.Close();
            return data;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

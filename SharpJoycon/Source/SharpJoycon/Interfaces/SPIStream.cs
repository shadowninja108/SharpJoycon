using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces.SPI
{
    public class SPIStream : Stream
    {

        static readonly int readLimit = 0x1D;

        private NintendoController controller;
        private CommandInterface command;
        private HIDInterface hid;
        private long pos;

        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanSeek => true;
        public override long Length => 0x10000 + 0x70000;
        // could this be simplified?
        public override long Position { get => pos; set => pos = value; }

        public SPIStream(NintendoController controller)
        {
            this.controller = controller;
            command = controller.GetCommands();
            hid = controller.GetHID();
        }

        public override void Flush()
        {
            //shouldn't be needed
            throw new NotImplementedException();
        }

        public byte[] Read(int offset, int count)
        {
            byte[] buffer = new byte[count];
            int bytes = Read(buffer, offset, count);
            return buffer;
        }

        public async Task ReadAsync(int count, IProgress<byte[]> progress)
        {
            decimal reads = Math.Ceiling(((decimal)count / readLimit));
            byte[] data;
            await Task.Run(() =>
            {
                for (int i = 0; i < reads; i++)
                {
                    int readOffset = i * readLimit;
                    int readAddress = (int)Position + readOffset;
                    int readLength = Math.Min(count - readOffset, readLimit);
                    data = new byte[readLength];
                    List<byte> outputBytes = new List<byte>(BitConverter.GetBytes(readAddress));
                    outputBytes.Add((byte)readLength);
                    byte[] output = outputBytes.ToArray();
                    PacketData packet;
                    Console.WriteLine($"Attempting SPI read ({i + 1}/{reads})...");
                    int attempts = 0;
                    while (true)
                    {
                        // spam because why not?
                        attempts++;
                        packet = command.SendSubcommand(0x1, 0x10, output);
                        if (output.SequenceEqual(packet.Data.Take(output.Length)))
                            break;
                    }
                    Console.WriteLine($"SPI read took {attempts} attempt{(attempts == 1 ? "" : "s")}"); // lol grammar
                    data = packet.Data.Skip(5).Take(readLength).ToArray();
                    progress.Report(data);
                }
            });
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            MemoryStream stream = new MemoryStream(buffer);
            Progress<byte[]> progress = new Progress<byte[]>();
            stream.Seek(offset, SeekOrigin.Begin);
            progress.ProgressChanged += (d,data) =>
            {
                stream.Write(data, 0, data.Length);
            };
            ReadAsync(count, progress).Wait(); // wait for the buffer to be filled
            Seek(stream.Length, SeekOrigin.Current);
            return (int) stream.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public new void CopyTo(Stream stream)
        {
            Progress<byte[]> progress = new Progress<byte[]>();
            progress.ProgressChanged += (d, data) =>
            {
                stream.Write(data, 0, data.Length);
            };
            ReadAsync((int) Position, (int) Length, progress).Wait();
        }

        public override void SetLength(long value)
        {
            // not how this works buddy
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}

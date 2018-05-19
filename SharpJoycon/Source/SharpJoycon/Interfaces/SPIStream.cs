using SharpJoycon.Utilities;
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

        static readonly int ioLimit = 0x1D;

        private NintendoController controller;
        private CommandInterface command;
        private HIDInterface hid;

        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => true;
        public override long Length => 0x10000 + 0x70000;
        // could this be simplified?
        public override long Position { get; set; }

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

        public byte[] Read(int count)
        {
            byte[] buffer = new byte[count];
            int bytes = Read(buffer, 0, count);
            return buffer;
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public async Task ReadAsync(int count, IProgress<byte[]> progress)
        {
            decimal reads = Math.Ceiling(((decimal)count / ioLimit));
            byte[] data;
            await Task.Run(async () =>
            {
                for (int i = 0; i < reads; i++)
                {
                    int readOffset = i * ioLimit;
                    int readAddress = (int)Position + readOffset;
                    int readLength = Math.Min(count - readOffset, ioLimit);
                    List<byte> outputBytes = new List<byte>();

                    outputBytes.AddAll(BitConverter.GetBytes(readAddress));
                    outputBytes.Add((byte) readLength);

                    byte[] output = outputBytes.ToArray();
                    Console.WriteLine($"SPI read ({i + 1}/{reads})...");
                    PacketData packet = await command.SendSubcommandAsync(0x1, 0x10, output, (p) =>
                         p.Data.Take(output.Length).SequenceEqual(output) // check that read is the one requested
                    );
                    data = packet.Data.Skip(output.Length).Take(readLength).ToArray();
                    progress.Report(data);
                }
            });
        }

        public async Task WriteAsync(byte[] data, int offset, int count, IProgress<int> progress)
        {
            decimal writes = Math.Ceiling(((decimal)count / ioLimit));
            await Task.Run(async () => {
                for (int i = 0; i < writes; i++)
                {
                    int writeOffset = i * ioLimit;
                    int writeLength = Math.Min(data.Length - writeOffset, ioLimit);
                    List<byte> outputBytes = new List<byte>();

                    outputBytes.AddAll(BitConverter.GetBytes(Position));
                    outputBytes.Add((byte)writeLength);
                    outputBytes.AddAll(data.Skip(writeOffset + offset).Take(writeLength));

                    byte[] output = outputBytes.ToArray();
                    Console.WriteLine($"SPI write ({i + 1}/{writes})...");
                    await command.SendSubcommandAsync(0x1, 0x11, output);
                    progress.Report(writeOffset + writeLength);
                    Seek(writeLength, SeekOrigin.Current);
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count, new Progress<int>()).Wait();
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
            ReadAsync((int)Length, progress).Wait();
        }


        public override void SetLength(long value)
        {
            // not how this works buddy
            throw new InvalidOperationException();
        }

    }
}

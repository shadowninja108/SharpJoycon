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
        private long pos;

        public override bool CanRead => true;
        public override bool CanWrite => true;
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
            decimal reads = Math.Ceiling(((decimal)count / ioLimit));
            byte[] data;
            await Task.Run(() =>
            {
                for (int i = 0; i < reads; i++)
                {
                    int readOffset = i * ioLimit;
                    int readAddress = (int)Position + readOffset;
                    int readLength = Math.Min(count - readOffset, ioLimit);
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

        public async Task WriteAsync(int offset, byte[] data, IProgress<int> progress)
        {
            decimal writes = Math.Ceiling(((decimal)data.Length / ioLimit));
            await Task.Run(() => {
                for (int i = 0; i < writes; i++)
                {
                    int writeOffset = i * ioLimit;
                    int writeAddress = (int)Position + writeOffset;
                    int writeLength = Math.Min(data.Length - writeOffset, ioLimit);
                    List<byte> outputBytes = new List<byte>();
                    outputBytes.AddAll(BitConverter.GetBytes(writeAddress));
                    outputBytes.Add((byte)writeLength);
                    outputBytes.AddAll(data.Skip(writeOffset).Take(writeLength));
                    byte[] output = outputBytes.ToArray();
                    Console.WriteLine($"Write payload: {BitConverter.ToString(output)}");
                    int result = 0;
                    int attempts = 0;
                    Console.WriteLine($"Attempting SPI write ({i + 1}/{writes})...");
                    while (result != 0x1180) {
                        PacketData packet = command.SendSubcommand(0x1, 0x11, output);
                        result = BitConverter.ToUInt16(packet.rawData.Skip(0xD).Take(2).ToArray(), 0);
                        attempts++;
                    }
                    Console.WriteLine($"SPI write took {attempts} attempt{(attempts == 1 ? "" : "s")}"); // lol grammar
                    progress.Report(writeOffset + writeLength);
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

        public void Write(byte[] data, int offset)
        {
            WriteAsync(offset, data, new Progress<int>()).Wait();
            Seek(data.Length, SeekOrigin.Current);
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

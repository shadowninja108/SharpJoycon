﻿using SharpJoycon.Utilities;
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
            await Task.Run(() =>
            {
                for (int i = 0; i < reads; i++)
                {
                    int readOffset = i * ioLimit;
                    int readAddress = (int)Position + readOffset;
                    int readLength = Math.Min(count - readOffset, ioLimit);
                    data = new byte[readLength];
                    List<byte> outputBytes = new List<byte>();

                    outputBytes.AddAll(BitConverter.GetBytes(readAddress));
                    outputBytes.Add((byte) readLength);

                    byte[] output = outputBytes.ToArray();
                    PacketData packet;
                    int attempts = 0;
                    Console.WriteLine($"Attempting SPI read ({i + 1}/{reads})...");
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

        public async Task WriteAsync(byte[] data, int offset, int count, IProgress<int> progress)
        {
            decimal writes = Math.Ceiling(((decimal)count / ioLimit));
            await Task.Run(() => {
                for (int i = 0; i < writes; i++)
                {
                    int writeOffset = i * ioLimit;
                    int writeAddress = (int) Position + writeOffset;
                    int writeLength = Math.Min(data.Length - writeOffset, ioLimit);
                    List<byte> outputBytes = new List<byte>();

                    outputBytes.AddAll(BitConverter.GetBytes(writeAddress));
                    outputBytes.Add((byte)writeLength);
                    outputBytes.AddAll(data.Skip(writeOffset + offset).Take(writeLength));

                    byte[] output = outputBytes.ToArray();
                    int result = 0;
                    int attempts = 0;
                    Console.WriteLine($"Attempting SPI write ({i + 1}/{writes})...");
                    while (result != 0x1180) { // magic number found in Joy-Con Toolkit
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count, new Progress<int>()).Wait();
            Seek(count, SeekOrigin.Current);
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
            ReadAsync(0, progress).Wait();
        }

        public override void SetLength(long value)
        {
            // not how this works buddy
            throw new InvalidOperationException();
        }

    }
}

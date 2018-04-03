using System;
using System.IO;
using SharpJoycon.Utilities;
using static SharpJoycon.Interfaces.HardwareInterface;

namespace SharpJoycon.Interfaces
{
    public class HomeLEDInterface : AbstractInterface
    {
        CommandInterface command;

        public HomeLEDInterface(NintendoController controller) : base(controller)
        {
            command = controller.GetCommands();
        }

        public void SendPattern(Pattern pattern)
        {
            command.SendSubcommand(0x1, 0x38, ConvertPattern(pattern));
        }

        public override void Poll(HIDInterface.PacketData data)
        {
            // nothing to poll
        }

        public static byte[] ConvertPattern(Pattern pattern)
        {
            byte[] bytes = new byte[25];
            MemoryStream stream = new MemoryStream(bytes);
            stream.WriteByte(Nibble.Combine((byte) Math.Min(15,pattern.miniCycles.Length), pattern.miniCycleDuration));
            stream.WriteByte(Nibble.Combine(pattern.startIntensity, pattern.cycleCount));
            MiniCycle[] miniCycles = pattern.miniCycles;
            // code will ignore more than 15 mini cycles
            for (int i = 0; i < Math.Min(14, miniCycles.Length) / 2; i+=2)
            {
                MiniCycle mc1 = miniCycles[i];
                MiniCycle mc2 = miniCycles[i + 1];
                stream.WriteByte(Nibble.Combine(mc1.intensity, mc2.intensity));
                stream.WriteByte(mc1.durationMultiplier);
                stream.WriteByte(mc2.durationMultiplier);
            }
            if (miniCycles.Length > 14)
            {
                stream.WriteByte((byte) (miniCycles[14].intensity >> 4));
                stream.WriteByte(miniCycles[14].durationMultiplier);
            }
            return stream.ToArray();
        }

        public static Pattern GetHeartbeatPattern()
        {
            // 12 minicycles (0-15)
            // 1 = duration multiplier, the duration in the minicycle will be multiplied by this (0-15)
            // 0 = start intensity of led (0-15)
            // 0 = how many cycles/repeats, zero means repeat forever (0-15)
            Pattern pattern = new Pattern(12, 1, 0, 0);
            MiniCycle[] miniCycles = pattern.miniCycles;
            
            //15 = intensity of led (0-15)
            //0xF0 * duration multiplier = length of minicycle (0-255)
            //"bum"
            miniCycles[0] = new MiniCycle(15, 0xF0);
            //wait
            miniCycles[1] = new MiniCycle(0, 0xF0);
            //"bum"
            miniCycles[2] = new MiniCycle(15, 0xF0);
            
            //wait for the next cycle
            miniCycles[3] = new MiniCycle(0, 0xF0);
            miniCycles[4] = new MiniCycle(0, 0xFF);
            miniCycles[5] = new MiniCycle(0, 0xFF);
            miniCycles[6] = new MiniCycle(0, 0xFF);
            miniCycles[7] = new MiniCycle(0, 0xFF);
            miniCycles[8] = new MiniCycle(0, 0xFF);
            miniCycles[9] = new MiniCycle(0, 0xFF);
            miniCycles[10] = new MiniCycle(0, 0xFF);
            miniCycles[11] = new MiniCycle(0, 0xFF);

            return pattern;
        }

        public struct Pattern
        {
            public Pattern(byte miniCycleCount, byte miniCycleDuration, byte startIntensity, byte cycleCount)
            {
                this.miniCycleDuration = miniCycleDuration;
                this.startIntensity = startIntensity;
                this.cycleCount = cycleCount;
                miniCycles = new MiniCycle[miniCycleCount];
            }

            public byte miniCycleDuration, startIntensity, cycleCount;
            public MiniCycle[] miniCycles;
        }

        public struct MiniCycle
        {
            public MiniCycle(byte intensity, byte durationMultiplier)
            {
                this.intensity = intensity;
                this.durationMultiplier = durationMultiplier;
            }

            public byte intensity, durationMultiplier;
        }
    }
}

using System;
using System.Linq;

namespace SharpJoycon.Utilities
{
    public static class Extensions
    {
        public static int[] ToInt16(this byte[] input)
        {
            int[] output = new int[input.Length/2];
            for (int i = 0; i < output.Length; i++)
            {
                byte[] singular = input.Skip(i * 2).Take(2).ToArray();
                output[i] = BitConverter.ToInt16(singular, 0);
            }
            return output;
        }
    }
}

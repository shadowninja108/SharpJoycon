using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static void AddAll<T>(this List<T> list, IEnumerable<T> elements)
        {
            foreach(T element in elements){
                list.Add(element);
            }
        }
    }
}

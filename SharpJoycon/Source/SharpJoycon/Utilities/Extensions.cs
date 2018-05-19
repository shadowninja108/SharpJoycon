using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        public static void InsertAll<T>(this List<T> list, int index, IEnumerable<T> elements)
        {
            foreach(T t in elements.Reverse()){
                list.Insert(index, t);
            }
        }

        public static void Fill<T>(this List<T> list, T element, int count)
        {
            for(int i = 0; i < count; i++)
            {
                list.Add(element);
            }
        }

        public static byte[] Take(this MemoryStream stream, int count)
        {
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, count);
            return buffer;
        }

        public static byte Take(this MemoryStream stream) => (byte)stream.ReadByte();

        public static void Skip(this MemoryStream stream, int count) => stream.Position += count;

    }
}

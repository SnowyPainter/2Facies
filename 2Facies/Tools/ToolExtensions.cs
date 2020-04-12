
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Facies
{
    public static class ToolExtensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public static byte[] ToBitConvertByte(this string bitconverted)
        {
            string[] splited = bitconverted.Split('-');
            byte[] reversed = new byte[splited.Length];
            for (int i = 0; i < splited.Length; i++)
            {
                reversed[i] = Convert.ToByte(splited[i], 16);
            }
            return reversed;
        }

        public static byte[] Combine(this byte[] first, byte[] second)
        {
            var ms = new MemoryStream();
            ms.Write(first, 0, first.Length);
            ms.Write(second, 0, second.Length);
            return ms.ToArray();
        }

        public static string ToStringValue(this Socket.Headers header)
        {
            return ((int)header).ToString();
        }
    }
}

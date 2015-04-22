using CodeEndeavors.ServiceHost.Common;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Extensions
{
    public static class ConversionExtensions
    {
        public static ZipPayload ToCompress(string s)
        {
            return ConversionExtensions.ToCompress(Encoding.UTF8.GetBytes(s));
        }
        public static ZipPayload ToCompress(byte[] bytes)
        {
            ZipPayload ret = new ZipPayload(bytes.Length);
            MemoryStream memory = new MemoryStream();
            DeflaterOutputStream stream = new DeflaterOutputStream(memory, new Deflater(9), 131072);
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            ret.Complete(memory.ToArray());
            return ret;
        }
        public static ZipPayload ToDecompress(Stream inputStream)
        {
            byte[] bytes = ConversionExtensions.ToBytes(inputStream);
            return ConversionExtensions.ToDecompress(bytes);
        }
        public static ZipPayload ToDecompress(byte[] bytes)
        {
            ZipPayload ret = new ZipPayload(bytes.Length);
            InflaterInputStream stream = new InflaterInputStream(new MemoryStream(bytes));
            MemoryStream memory = new MemoryStream();
            byte[] writeData = new byte[4097];
            while (true)
            {
                int size = stream.Read(writeData, 0, writeData.Length);
                bool flag = size > 0;
                if (!flag)
                {
                    break;
                }
                memory.Write(writeData, 0, size);
            }
            stream.Close();
            ret.Complete(memory.ToArray());
            return ret;
        }
        public static Stream ToStream(string s)
        {
            return new MemoryStream(Encoding.Default.GetBytes(s));
        }
        public static string ToString(byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }
        public static byte[] ToBytes(Stream inputStream)
        {
            int initialLength = 32768;
            checked
            {
                byte[] buffer = new byte[initialLength + 1];
                int read = 0;
                byte[] ToBytes;
                while (true)
                {
                    int chunk = inputStream.Read(buffer, read, buffer.Length - read);
                    bool flag = chunk == 0;
                    if (flag)
                    {
                        break;
                    }
                    read += chunk;
                    flag = (read == buffer.Length);
                    if (flag)
                    {
                        int nextByte = inputStream.ReadByte();
                        flag = (nextByte == -1);
                        if (flag)
                        {
                            ToBytes = buffer;
                            return ToBytes;
                        }
                        byte[] newBuffer = new byte[buffer.Length * 2 + 1];
                        Array.Copy(buffer, newBuffer, buffer.Length);
                        newBuffer[read] = (byte)nextByte;
                        buffer = newBuffer;
                        read++;
                    }
                }
                byte[] ret = new byte[read + 1];
                Array.Copy(buffer, ret, read);
                ToBytes = ret;
                return ToBytes;
            }
        }
        public static string ToString(Stream inputStream)
        {
            TextReader stream = new StreamReader(inputStream);
            string contents;
            try
            {
                contents = stream.ReadToEnd();
            }
            finally
            {
                bool flag = stream != null;
                if (flag)
                {
                    ((IDisposable)stream).Dispose();
                }
            }
            return contents;
        }
    }
}

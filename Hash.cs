using System;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hasher
{
    class Hash
    {
        public string HashString { get; private set; }

        public Hash(string filePath)
        {
            HashString = CalculateHash(filePath);
        }

        private String CalculateHash(string filePath)
        {
            byte[] buffer;
            int bytesRead;
            long size;
            long totalBytesRead = 0;

            using (Stream file = File.OpenRead(filePath))
            {
                size = file.Length;

                using (HashAlgorithm hasher = MD5.Create())
                {
                    do
                    {
                        buffer = new byte[4096];

                        bytesRead = file.Read(buffer, 0, buffer.Length);

                        totalBytesRead += bytesRead;

                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);

                        Console.Write("\r{0}%   ",(int)((double)totalBytesRead / size * 100));

                    } while (bytesRead != 0);

                    hasher.TransformFinalBlock(buffer, 0, 0);

                    Console.WriteLine();

                    return MakeHashString(hasher.Hash);

                }
            }
        }

        private static String MakeHashString(byte[] hashBytes)
        {
            StringBuilder hash = new StringBuilder(32);

            foreach (byte b in hashBytes) {
                hash.Append(b.ToString("X2").ToLower());
            }

            return hash.ToString();
        }
    }
}
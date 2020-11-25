using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    class Coder
    {
        public static void Encryprt(string file, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            string s;
            using(StreamReader sr = new StreamReader(file))
            {
                s = sr.ReadToEnd();
            }
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(s);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            using(StreamWriter sw = new StreamWriter(file))
            {
                foreach(byte b in encrypted)
                {
                    sw.WriteLine(b);
                }
            }
        }

        public static void Decryprt(string file, byte[] Key, byte[] IV)
        {
            List<byte> bytes = new List<byte>();
            string s;
            using (StreamReader sr = new StreamReader(file))
            {
                while(!sr.EndOfStream)
                {
                    bytes.Add(Convert.ToByte(sr.ReadLine()));
                }
            }
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(bytes.ToArray()))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            s = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(s);
            }
        }
    }
}

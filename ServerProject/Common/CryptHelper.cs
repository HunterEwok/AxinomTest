using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ServerProject.Common
{
    // helper for decrypt data from client
    public static class CryptHelper
    {
        private static string _secretKey;

        public static void Init(string secretKey)
        {
            _secretKey = secretKey;
        }

        // simple string decryption
        public static string Decrypt(string text)
        {
            byte[] src = Convert.FromBase64String(text);
            RijndaelManaged aes = new RijndaelManaged();
            byte[] key = Encoding.ASCII.GetBytes(_secretKey);
            aes.KeySize = 128;
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.ECB;

            using (ICryptoTransform decrypt = aes.CreateDecryptor(key, null))
            {
                byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                decrypt.Dispose();
                return Encoding.UTF8.GetString(dest);
            }
        }

        // decription whole JSON
        public static string DecryptAll(string text)
        {
            int startPos = 0;
            Dictionary<string, string> replaces = new Dictionary<string, string>();

            while (text.IndexOf("\"", startPos) > 0)
            {
                string subText = text.Substring(text.IndexOf('\"', startPos) + 1, 
                    text.IndexOf("\":", startPos) - text.IndexOf('\"', startPos) - 1);

                if (!replaces.ContainsKey("\"" + subText + "\":"))
                    replaces.Add("\"" + subText + "\":", "\"" + Decrypt(subText) + "\":");

                startPos = text.IndexOf("\":", startPos) + 3;
            }

            foreach (KeyValuePair<string, string> replace in replaces)
                text = text.Replace(replace.Key, replace.Value);

            return text;
        }
    }
}

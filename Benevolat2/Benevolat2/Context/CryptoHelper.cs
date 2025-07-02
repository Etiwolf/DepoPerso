using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Aristopattes.Context
{
    internal class CryptoHelper
    {
        private AristopattesContext Context = new AristopattesContext();
        public string EncryptString(string plainText)
        {
            string encryptedkey = Context.GetSetting("EncryptionKey");

            // Génère une clé AES 256 bits depuis la clé texte (via SHA256)
            using var sha256 = SHA256.Create();
            byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptedkey));

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();

            // Stocker IV au début
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        public string DecryptString(string cipherText)
        {
            string encryptedkey = Context.GetSetting("EncryptionKey");

            if (string.IsNullOrWhiteSpace(encryptedkey))
                throw new InvalidOperationException("EncryptionKey is not set in configuration.");

            if (string.IsNullOrWhiteSpace(cipherText))
                throw new ArgumentException("cipherText is null or empty.");

            byte[] fullBytes;
            try
            {
                fullBytes = Convert.FromBase64String(cipherText);
            }
            catch (FormatException)
            {
                throw new ArgumentException("cipherText is not a valid Base64 string.");
            }

            if (fullBytes.Length < 16)
                throw new ArgumentException("cipherText is too short to contain an IV.");

            byte[] iv = new byte[16];
            Array.Copy(fullBytes, 0, iv, 0, iv.Length);

            byte[] cipherBytes = new byte[fullBytes.Length - iv.Length];
            Array.Copy(fullBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var sha256 = SHA256.Create();
            byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptedkey));

            try
            {
                using var aes = Aes.Create();
                aes.Key = keyBytes;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(cipherBytes);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (CryptographicException ex)
            {
                throw new InvalidOperationException("Decryption failed. Possibly due to wrong key or corrupt data.", ex);
            }
        }
    }    
}

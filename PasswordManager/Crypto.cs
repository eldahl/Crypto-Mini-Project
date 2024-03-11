using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    class Crypto
    {
        public void AES_Print()
        {
            byte[] key = RandomNumberGenerator.GetBytes(32);
            byte[] iv = RandomNumberGenerator.GetBytes(12);
            byte[] data = UTF8Encoding.UTF8.GetBytes("Hello, World!");

            byte[] auth_tag_out = new byte[256];
            int auth_tag_length = 0;

            // Encrypt the data and get the authentication tag
            byte[] encrypted = AES_GCM_Encrypt(data, key, iv, out auth_tag_out, out auth_tag_length);

            // Extract the authentication tag from the output buffer
            byte[] auth_tag = new byte[auth_tag_length];
            Buffer.BlockCopy(auth_tag_out, 0, auth_tag, 0, auth_tag_length);

            // Decrypt the data using the authentication tag, key, and IV
            byte[] decrypted = AES_GCM_Decrypt(encrypted, key, iv, auth_tag);

            Console.WriteLine("Original: {0} {1}", BitConverter.ToString(data), UTF8Encoding.UTF8.GetString(data));
            Console.WriteLine("Encrypted: {0}", BitConverter.ToString(encrypted));
            Console.WriteLine("Decrypted: {0} {1}", BitConverter.ToString(decrypted), UTF8Encoding.UTF8.GetString(decrypted));
        }

        public byte[] AES_GCM_Encrypt(byte[] data, byte[] key, byte[] iv, out byte[] auth_tag, out int auth_tag_length)
        {
            using (AesGcm aes_gcm = new AesGcm(key))
            {

                // Output buffer for the encrypted data & the authentication tag
                byte[] encrypted = new byte[data.Length];
                byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];

                // Encrypt the data and get the authentication tag
                aes_gcm.Encrypt(iv, data, encrypted, tag);

                // Return the encrypted data and the authentication tag
                auth_tag = tag;
                auth_tag_length = tag.Length;
                return encrypted;
            }
        }

        public byte[] AES_GCM_Decrypt(byte[] data, byte[] key, byte[] iv, byte[] auth_tag)
        {
            using (AesGcm aes_gcm = new AesGcm(key))
            {
                // Output buffer for the decrypted data
                byte[] decrypted = new byte[data.Length];

                // Decrypt the data using the authentication tag, key, and IV
                aes_gcm.Decrypt(iv, data, auth_tag, decrypted);
                return decrypted;
            }
        }

        public byte[] PBKDF2_HMAC_SHA512(string password, byte[] salt, int iterations, int outputBytes)
        {
            return new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512).GetBytes(outputBytes);
        }

        // Implement SCrypt using CryptSharp
        public byte[] SCrypt(byte[] password, byte[] salt, int cost, int blockSize, int parallel, int outputBytes)
        {
            return CryptSharp.Utility.SCrypt.ComputeDerivedKey(password, salt, cost, blockSize, parallel, null, outputBytes);
        }
    }
}

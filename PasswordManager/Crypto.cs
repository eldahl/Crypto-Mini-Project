using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PasswordManager.Model;
using PasswordManager.ViewModel;

namespace PasswordManager
{
    static class Crypto
    {
        public static byte[] AES_GCM_Encrypt(byte[] data, byte[] key, byte[] iv, out byte[] auth_tag, out int auth_tag_length)
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

        public static byte[] AES_GCM_Decrypt(byte[] data, byte[] key, byte[] iv, byte[] auth_tag)
        {
            using (AesGcm aes_gcm = new AesGcm(key))
            {
                // Output buffer for the decrypted data
                byte[] decrypted = new byte[data.Length];

                // Decrypt the data using the authentication tag, key, and IV
                try {
                    aes_gcm.Decrypt(iv, data, auth_tag, decrypted);
                }
                catch (CryptographicException e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    return null;
                }
                return decrypted;
            }
        }

        public static byte[] PBKDF2_HMAC_SHA512(string password, byte[] salt, int iterations, int outputBytes)
        {
            return new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512).GetBytes(outputBytes);
        }

        // Implement SCrypt using CryptSharp
        public static byte[] SCrypt(byte[] password, byte[] salt, int cost, int blockSize, int parallel, int outputBytes)
        {
            return CryptSharp.Utility.SCrypt.ComputeDerivedKey(password, salt, cost, blockSize, parallel, null, outputBytes);
        }

        public static PasswordDecryptResult DecryptVerificationTag(string password)
        {
            // Get the encrypted data from the database
            VerificationTag verifyData = SQLiteDatabaseContext.instance.GetVerificationTag();

            int iterations = verifyData.pbkdf2_iterations;
            byte[] salt = verifyData.pbkdf2_salt;

            // First generate the key using RFC2898
            byte[] hmac_key = PBKDF2_HMAC_SHA512(password, salt, iterations, 256);

            byte[] scrypt_salt = verifyData.scrypt_salt;
            int cost = verifyData.scrypt_cost;
            int blockSize = verifyData.scrypt_blockSize;
            int parallel = verifyData.scrypt_parallel;
            int outputBytes = verifyData.scrypt_outputBytes;

            // Then hash the key through Scrypt
            byte[] scrypt_key = SCrypt(hmac_key, salt, cost, blockSize, parallel, outputBytes);

            // Finally, use the key to decrypt the data
            byte[] decryptedData = AES_GCM_Decrypt(verifyData.aes_verification_tag, scrypt_key, verifyData.aes_iv, verifyData.aes_auth_tag);

            if(decryptedData is null) {
                return PasswordDecryptResult.Error;
            }

            // Check if the decrypted data is the secret key
            if (Encoding.UTF8.GetString(decryptedData) == "Secret Key")
            {
                return PasswordDecryptResult.Success;
            }

            return PasswordDecryptResult.Error;
        }

        public static void EncryptVerificationTag(string password) {
            int iterations = 10000;
            byte[] salt = RandomNumberGenerator.GetBytes(32);

            // First generate the key using RFC2898
            byte[] hmac_key = PBKDF2_HMAC_SHA512(password, salt, iterations, 256);

            byte[] scrypt_salt = RandomNumberGenerator.GetBytes(32);
            int cost = 16384;
            int blockSize = 8;
            int parallel = 1;
            int outputBytes = 32;

            // Then hash the key through Scrypt
            byte[] scrypt_key = SCrypt(hmac_key, salt, cost, blockSize, parallel, outputBytes);
            byte[] aes_iv = RandomNumberGenerator.GetBytes(12);

            // Encrypt the data using the key
            byte[] encrypted_data = AES_GCM_Encrypt(Encoding.UTF8.GetBytes("Secret Key"), scrypt_key, aes_iv, out byte[] auth_tag, out int auth_tag_length);

            // Store the encrypted data in the database
            SQLiteDatabaseContext.instance.SetVerificationTag(new VerificationTag()
            {
                pbkdf2_salt = salt,
                pbkdf2_iterations = iterations,

                scrypt_salt = scrypt_salt,
                scrypt_cost = cost,
                scrypt_blockSize = blockSize,
                scrypt_parallel = parallel,
                scrypt_outputBytes = outputBytes,
                
                aes_verification_tag = encrypted_data,
                aes_iv = aes_iv,
                aes_auth_tag = auth_tag
            });

            Debug.WriteLine("Encrypted Verification Tag");
            Debug.WriteLine("Key: " + BitConverter.ToString(scrypt_key));
            Debug.WriteLine("IV: " + BitConverter.ToString(aes_iv));
            Debug.WriteLine("Auth Tag: " + BitConverter.ToString(auth_tag));
        }

        // Write a method for generating a random password of variable lengths consisting of letters, numbers, and special characters
        public static string GenerateRandomPassword(int length, bool use_special_characters)
        {
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            if(use_special_characters)
            {
                chars += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
            }
            
            char[] password = new char[length];
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }

        // Write a method for generating random passphrases based on words from a dictionary
        public static string GenerateRandomPassphrase(int wordCount)
        {
            string[] words = new string[] { "apple", "banana", "cherry", "date", "elderberry", "fig", "grape", "honeydew", "kiwi", "lemon", "mango", "nectarine", "orange", "papaya", "quince", "raspberry", "strawberry", "tangerine", "watermelon" };

            string passphrase = "";
            Random random = new Random();

            for (int i = 0; i < wordCount; i++)
            {
                passphrase += words[random.Next(words.Length)];
            }

            return passphrase;
        }

        public static void AddNewCredentials(string name, string username, string password, string master_password)
        {
            int iterations = 10000;
            byte[] salt = RandomNumberGenerator.GetBytes(32);

            // First generate the key using RFC2898
            byte[] hmac_key = PBKDF2_HMAC_SHA512(master_password, salt, iterations, 256);

            byte[] scrypt_salt = RandomNumberGenerator.GetBytes(32);
            int cost = 16384;
            int blockSize = 8;
            int parallel = 1;
            int outputBytes = 32;

            // Then hash the key through Scrypt
            byte[] scrypt_key = SCrypt(hmac_key, scrypt_salt, cost, blockSize, parallel, outputBytes);

            // Write: SCrypt is called with the following parameters:
            //Debug.WriteLine("SCrypt Parameters");
            //Debug.WriteLine("HMAC: " + BitConverter.ToString(hmac_key));
            //Debug.WriteLine("Salt: " + BitConverter.ToString(salt));
            //Debug.WriteLine("Cost: " + cost);
            //Debug.WriteLine("BlockSize: " + blockSize);
            //Debug.WriteLine("Parallel: " + parallel);
            //Debug.WriteLine("OutputBytes: " + outputBytes);
            //Debug.WriteLine("");


            byte[] aes_iv = RandomNumberGenerator.GetBytes(12);

            // Encrypt the data using the key
            byte[] encrypted_password = AES_GCM_Encrypt(Encoding.UTF8.GetBytes(password), scrypt_key, aes_iv, out byte[] auth_tag, out int auth_tag_length);

            DBPasswordEntry passwordEntry = new DBPasswordEntry()
            {
                name = Encoding.UTF8.GetBytes(name),
                username = Encoding.UTF8.GetBytes(username),
                encryptedPassword = encrypted_password,

                pbkdf2_salt = salt,
                pbkdf2_iterations = iterations,

                scrypt_salt = scrypt_salt,
                scrypt_cost = cost,
                scrypt_blockSize = blockSize,
                scrypt_parallel = parallel,
                scrypt_outputBytes = outputBytes,

                aes_iv = aes_iv,
                aes_auth_tag = auth_tag
            };

            SQLiteDatabaseContext.instance.InsertPassword(passwordEntry);

            MainWindowViewModel.instance.Passwords.Add(new PasswordListViewItem() { ServiceName = name, Username = username, PasswordData = passwordEntry });

            // Log all variables
            /*
            Debug.WriteLine("Encrypted Password");
            Debug.WriteLine("Name: " + name);
            Debug.WriteLine("Username: " + username);
            Debug.WriteLine("Password: " + password);
            Debug.WriteLine("hmac Key: " + BitConverter.ToString(hmac_key));
            Debug.WriteLine("Scrypt Key: " + BitConverter.ToString(scrypt_key));
            Debug.WriteLine("IV: " + BitConverter.ToString(aes_iv));
            Debug.WriteLine("Auth Tag: " + BitConverter.ToString(auth_tag));

            Debug.WriteLine("pbkdf2_salt: " + BitConverter.ToString(salt));
            Debug.WriteLine("pbkdf2_iterations: " + iterations);

            Debug.WriteLine("scrypt_salt: " + BitConverter.ToString(scrypt_salt));
            Debug.WriteLine("scrypt_cost: " + cost);
            Debug.WriteLine("scrypt_blockSize: " + blockSize);
            Debug.WriteLine("scrypt_parallel: " + parallel);
            Debug.WriteLine("scrypt_outputBytes: " + outputBytes);
            */
        }

        public static string DecryptPassword(DBPasswordEntry passwordData, string password)
        {
            Debug.WriteLine(password);

            byte[] hmac_key = PBKDF2_HMAC_SHA512(password, passwordData.pbkdf2_salt, passwordData.pbkdf2_iterations, 256);

            byte[] scrypt_key = SCrypt(hmac_key, passwordData.scrypt_salt, passwordData.scrypt_cost, passwordData.scrypt_blockSize, passwordData.scrypt_parallel, passwordData.scrypt_outputBytes);

            byte[] decrypted_password = AES_GCM_Decrypt(passwordData.encryptedPassword, scrypt_key, passwordData.aes_iv, passwordData.aes_auth_tag);

            if (decrypted_password is null)
            {
                // Error messagebox
                MessageBox.Show("Error: Could not decrypt password");
                return "";
            }

            return Encoding.UTF8.GetString(decrypted_password);
        }
    }
}

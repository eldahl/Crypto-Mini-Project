using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Model
{
    internal class VerificationTag
    {
        // PBKDF2-HMAC-SHA256
        public byte[] pbkdf2_salt { get; set; }
        public int pbkdf2_iterations { get; set; }

        // Scrypt
        public byte[] scrypt_salt { get; set; }
        public int scrypt_cost { get; set; }
        public int scrypt_blockSize { get; set; }
        public int scrypt_parallel { get; set; }
        public int scrypt_outputBytes { get; set; }

        // AES-GCM
        public byte[] aes_verification_tag { get; set; }
        public byte[] aes_iv { get; set; }
        public byte[] aes_auth_tag { get; set; }
    }
}

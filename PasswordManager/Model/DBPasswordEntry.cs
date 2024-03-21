using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Model
{
    public class DBPasswordEntry
    {
        public byte[] name { get; set; }
        public byte[] username { get; set; }
        public byte[] encryptedPassword { get; set; }
        public byte[] pbkdf2_salt { get; set; }
        public int pbkdf2_iterations { get; set; }
        public byte[] scrypt_salt { get; set; }
        public int scrypt_cost { get; set; }
        public int scrypt_blockSize { get; set; }
        public int scrypt_parallel { get; set; }
        public int scrypt_outputBytes { get; set; }
        public byte[] aes_iv { get; set; }
        public byte[] aes_auth_tag { get; set; }
    }
}
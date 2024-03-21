using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using PasswordManager.Model;
using System.Diagnostics;
using OpenQA.Selenium.DevTools.V120.CacheStorage;
using System.Buffers.Text;

namespace PasswordManager
{
    internal class SQLiteDatabaseContext
    {
        public static SQLiteDatabaseContext instance;
        SQLiteConnection connection;

        public string DatabaseFilePath {
            get { 
                return _databaseFilePath; 
            }
            set {
                if (value == "")
                    return;
                _databaseFilePath = value;
                CreateDatabase();
            }
        }
        private string _databaseFilePath = "PasswordManager.db";

        public SQLiteDatabaseContext() {

            if (instance is null)
                instance = this;
            else { 
                throw new Exception("SQLiteDatabaseContext instance already exists");
            }

            CreateDatabase();
        }

        public void CreateDatabase()
        {
            connection = new SQLiteConnection("Data Source=" + DatabaseFilePath + ";Version=3;");
            connection.Open();

            // Add the table if it doesn't exist using System.Data.SQLite.Linq;
            using (SQLiteCommand command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS Passwords (name BLOB, username BLOB, password BLOB, pbkdf2_salt BLOB, pbkdf2_iterations INTEGER," +
                "scrypt_salt BLOB, scrypt_cost INTEGER, scrypt_blockSize INTEGER, scrypt_parallel INTEGER, scrypt_outputBytes INTEGER, " +
                "aes_iv BLOB, aes_auth_tag BLOB);" +

                "CREATE TABLE IF NOT EXISTS VerificationTag (pbkdf2_salt BLOB, pbkdf2_iterations INTEGER, " +
                "scrypt_salt BLOB, scrypt_cost INTEGER, scrypt_blockSize INTEGER, scrypt_parallel INTEGER, scrypt_outputBytes INTEGER, " +
                "aes_verification_tag BLOB, aes_iv BLOB, aes_auth_tag BLOB);",
                connection
            ))
            { command.ExecuteNonQuery(); }
        }

        public bool IsNewDatabase()
        {
            using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM VerificationTag;", connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return !reader.Read();
                }
            }
        }

        public IEnumerable<DBPasswordEntry> GetAllPasswordEntries() { 
            // Query the database for all password entries
            using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM Passwords", connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new DBPasswordEntry
                        {
                            name = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["name"])),
                            username = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["username"])),
                            encryptedPassword = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["password"])),
                            
                            pbkdf2_salt = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["pbkdf2_salt"])),
                            pbkdf2_iterations = (int)(long)reader["pbkdf2_iterations"],

                            scrypt_salt = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["scrypt_salt"])),
                            scrypt_cost = (int)(long)reader["scrypt_cost"],
                            scrypt_blockSize = (int)(long)reader["scrypt_blockSize"],
                            scrypt_parallel = (int)(long)reader["scrypt_parallel"],
                            scrypt_outputBytes = (int)(long)reader["scrypt_outputBytes"],

                            aes_iv = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["aes_iv"])),
                            aes_auth_tag = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["aes_auth_tag"]))
                        };
                    }
                }
            }
        }

        public void InsertPassword(DBPasswordEntry encryptedPasswordData) {

            string sql = String.Format("INSERT INTO Passwords VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}')",
                Convert.ToBase64String(encryptedPasswordData.name),
                Convert.ToBase64String(encryptedPasswordData.username),
                Convert.ToBase64String(encryptedPasswordData.encryptedPassword),
                Convert.ToBase64String(encryptedPasswordData.pbkdf2_salt),
                encryptedPasswordData.pbkdf2_iterations,
                Convert.ToBase64String(encryptedPasswordData.scrypt_salt),
                encryptedPasswordData.scrypt_cost,
                encryptedPasswordData.scrypt_blockSize,
                encryptedPasswordData.scrypt_parallel,
                encryptedPasswordData.scrypt_outputBytes,
                Convert.ToBase64String(encryptedPasswordData.aes_iv),
                Convert.ToBase64String(encryptedPasswordData.aes_auth_tag));
            Debug.WriteLine(sql);

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            { 
                command.ExecuteNonQuery(); 
            }
        }


        public VerificationTag GetVerificationTag() {
            string sql = "SELECT * FROM VerificationTag";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new VerificationTag() {
                            pbkdf2_salt = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["pbkdf2_salt"])),
                            pbkdf2_iterations = (int)(long)reader["pbkdf2_iterations"],

                            scrypt_salt = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["scrypt_salt"])),
                            scrypt_cost = (int)(long)reader["scrypt_cost"],
                            scrypt_blockSize = (int)(long)reader["scrypt_blockSize"],
                            scrypt_parallel = (int)(long)reader["scrypt_parallel"],
                            scrypt_outputBytes = (int)(long)reader["scrypt_outputBytes"],
                            
                            // Convert to byte array from base64 string
                            // Byte[] -> Base64String -> Byte[]
                            aes_verification_tag = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["aes_verification_tag"])),
                            aes_iv = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["aes_iv"])),
                            aes_auth_tag = Convert.FromBase64String(Encoding.UTF8.GetString((byte[])reader["aes_auth_tag"]))
                        };
                    }
                }
            }
            return null;
        }

        public void SetVerificationTag(VerificationTag tag) {

            string sql = String.Format("INSERT INTO VerificationTag VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')", 
                Convert.ToBase64String(tag.pbkdf2_salt),
                tag.pbkdf2_iterations,
                Convert.ToBase64String(tag.scrypt_salt),
                tag.scrypt_cost,
                tag.scrypt_blockSize,
                tag.scrypt_parallel,
                tag.scrypt_outputBytes,
                Convert.ToBase64String(tag.aes_verification_tag),
                Convert.ToBase64String(tag.aes_iv),
                Convert.ToBase64String(tag.aes_auth_tag));
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}

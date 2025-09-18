using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RobotGame.Security
{
    /// <summary>
    /// Provides symmetric encryption (AES‑256‑CBC) helpers for
    /// secure local storage. Generates a random IV for every call.
    /// </summary>
    public static class CryptoUtility
    {
        /// <summary>
        /// Encrypts plain text with AES‑256‑CBC and returns Base64 string (IV + Cipher).
        /// </summary>
        public static string Encrypt(string plainText, byte[] key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Concatenate IV + cipher text
            byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);
            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Decrypts a Base64 string (IV + Cipher) produced by <see cref="Encrypt"/>.
        /// </summary>
        public static string Decrypt(string cipherText, byte[] key)
        {
            byte[] combined = Convert.FromBase64String(cipherText);
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV
            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipherBytes = new byte[combined.Length - iv.Length];
            Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(combined, iv.Length, cipherBytes, 0, cipherBytes.Length);
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Derives a 256‑bit key from a device‑unique seed plus a private salt.
        /// </summary>
        public static byte[] DeriveKey(string salt)
        {
            string seed = SystemInfo.deviceUniqueIdentifier + salt;
            using SHA256 sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
        }
    }
}

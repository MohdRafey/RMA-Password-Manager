using Konscious.Security.Cryptography;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RMA.Windows.Data
{
  public class CryptoService
  {
    // Parameters for Argon2id - These MUST stay the same to unlock the vault later
    // 64MB Memory, 3 Iterations, 4 Parallel threads
    private const int DegreeOfParallelism = 4;
    private const int Iterations = 3;
    private const int MemorySize = 65536;

    /// <summary>
    /// Converts the User's PIN + Salt into a 256-bit (32-byte) AES Key.
    /// </summary>
    public byte[] DeriveKey(string pin, byte[] salt)
    {
      using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(pin)))
      {
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = DegreeOfParallelism;
        argon2.Iterations = Iterations;
        argon2.MemorySize = MemorySize;

        return argon2.GetBytes(32); // 32 bytes = 256 bits
      }
    }

    /// <summary>
    /// Encrypts a plain text string using AES-256.
    /// Returns a Base64 string containing [IV (16 bytes) + Ciphertext].
    /// </summary>
    public string Encrypt(string plainText, byte[] key)
    {
      if (string.IsNullOrEmpty(plainText)) return string.Empty;

      using (Aes aes = Aes.Create())
      {
        aes.KeySize = 256;
        aes.Key = key;
        aes.GenerateIV(); // Unique IV for every single password

        using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
        using (var ms = new MemoryStream())
        {
          // We write the IV at the start of the stream so we can find it later for decryption
          ms.Write(aes.IV, 0, aes.IV.Length);

          using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
          using (var sw = new StreamWriter(cs))
          {
            sw.Write(plainText);
          }

          return Convert.ToBase64String(ms.ToArray());
        }
      }
    }

    /// <summary>
    /// Decrypts a Base64 string that was encrypted with the Encrypt method.
    /// </summary>
    public string Decrypt(string cipherTextWithIv, byte[] key)
    {
      if (string.IsNullOrEmpty(cipherTextWithIv)) return string.Empty;

      byte[] fullCipher = Convert.FromBase64String(cipherTextWithIv);

      using (Aes aes = Aes.Create())
      {
        aes.KeySize = 256;
        aes.Key = key;

        // Extract the first 16 bytes as the IV
        byte[] iv = new byte[aes.BlockSize / 8];
        byte[] cipherText = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

        aes.IV = iv;

        using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
        using (var ms = new MemoryStream(cipherText))
        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (var sr = new StreamReader(cs))
        {
          return sr.ReadToEnd();
        }
      }
    }

    /// <summary>
    /// Generates a unique 16-byte random salt for the user.
    /// </summary>
    public byte[] GenerateSalt()
    {
      var salt = new byte[16];
      RandomNumberGenerator.Fill(salt);
      return salt;
    }
  }
}
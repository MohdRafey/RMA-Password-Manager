using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace RMA.Windows.Core
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
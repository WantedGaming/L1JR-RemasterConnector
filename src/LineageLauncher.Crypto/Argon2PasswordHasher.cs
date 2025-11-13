using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using LineageLauncher.Core.Interfaces;

namespace LineageLauncher.Crypto;

/// <summary>
/// Implements password hashing using Argon2id algorithm.
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 4;
    private const int MemorySize = 65536; // 64 MB
    private const int DegreeOfParallelism = 2;

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = HashPasswordInternal(password, salt);

        // Format: salt.hash (both base64 encoded)
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);

        var parts = hashedPassword.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = Convert.FromBase64String(parts[1]);
            var computedHash = HashPasswordInternal(password, salt);

            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] HashPasswordInternal(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(HashSize);
    }
}

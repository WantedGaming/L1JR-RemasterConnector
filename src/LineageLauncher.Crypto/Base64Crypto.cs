using System.Security.Cryptography;
using System.Text;

namespace LineageLauncher.Crypto;

/// <summary>
/// Implements Base64 + AES encryption/decryption for connector API responses.
/// Matches the Java server's implementation: AES-128-CBC with PKCS5 padding.
/// </summary>
public static class Base64Crypto
{
    /// <summary>
    /// Decrypts a Base64-encoded and AES-encrypted string.
    /// Matches Java implementation: AES/CBC/PKCS5Padding with first 16 bytes of key as both key and IV.
    /// </summary>
    /// <param name="encryptedBase64">The Base64-encoded encrypted data.</param>
    /// <param name="base64Key">The Base64-encoded encryption key.</param>
    /// <returns>The decrypted plaintext string.</returns>
    public static string DecryptFromBase64(string encryptedBase64, string base64Key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedBase64);
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Key);

        try
        {
            // Decode Base64 encrypted data
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);

            // Java takes the key string as UTF-8 bytes (NOT base64-decoded)
            // Then takes first 16 bytes for AES-128
            byte[] fullKeyBytes = Encoding.UTF8.GetBytes(base64Key);
            byte[] keyBytes = new byte[16];
            int keyLength = Math.Min(fullKeyBytes.Length, 16);
            Array.Copy(fullKeyBytes, 0, keyBytes, 0, keyLength);

            // Java uses the same 16 bytes for both key and IV
            byte[] ivBytes = new byte[16];
            Array.Copy(keyBytes, 0, ivBytes, 0, 16);

            // Decrypt using AES-128-CBC
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7; // .NET equivalent of PKCS5

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid Base64 format.", nameof(encryptedBase64), ex);
        }
        catch (CryptographicException ex)
        {
            throw new ArgumentException("Decryption failed. Invalid key or corrupted data.", nameof(encryptedBase64), ex);
        }
    }

    /// <summary>
    /// Encrypts a string using AES and returns Base64-encoded result.
    /// Matches Java implementation: AES/CBC/PKCS5Padding with first 16 bytes of key as both key and IV.
    /// </summary>
    /// <param name="plaintext">The plaintext to encrypt.</param>
    /// <param name="base64Key">The Base64-encoded encryption key.</param>
    /// <returns>The Base64-encoded encrypted data.</returns>
    public static string EncryptToBase64(string plaintext, string base64Key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Key);

        try
        {
            // Decode Base64 key and take first 16 bytes (AES-128)
            byte[] fullKeyBytes = Convert.FromBase64String(base64Key);
            byte[] keyBytes = new byte[16];
            int keyLength = Math.Min(fullKeyBytes.Length, 16);
            Array.Copy(fullKeyBytes, 0, keyBytes, 0, keyLength);

            // Java uses the same 16 bytes for both key and IV
            byte[] ivBytes = new byte[16];
            Array.Copy(keyBytes, 0, ivBytes, 0, 16);

            // Encrypt using AES-128-CBC
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7; // .NET equivalent of PKCS5

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid Base64 key format.", nameof(base64Key), ex);
        }
    }

    /// <summary>
    /// Decrypts an integer value from Base64-encoded AES-encrypted string.
    /// </summary>
    public static int DecryptInt(string encryptedBase64, string base64Key)
    {
        string decrypted = DecryptFromBase64(encryptedBase64, base64Key);
        if (int.TryParse(decrypted, out int result))
        {
            return result;
        }
        throw new FormatException($"Decrypted value '{decrypted}' is not a valid integer.");
    }
}

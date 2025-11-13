namespace LineageLauncher.Crypto;

/// <summary>
/// Implements XOR encryption/decryption for Lin.bin parameter encryption.
/// </summary>
public static class XorEncryption
{
    /// <summary>
    /// Encrypts or decrypts data using XOR with the provided key.
    /// XOR is symmetric: encryption and decryption use the same operation.
    /// </summary>
    /// <param name="data">The data to encrypt or decrypt.</param>
    /// <param name="key">The XOR key.</param>
    /// <returns>The encrypted or decrypted data.</returns>
    public static byte[] Transform(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (data.IsEmpty)
        {
            return Array.Empty<byte>();
        }

        if (key.IsEmpty)
        {
            throw new ArgumentException("Key cannot be empty.", nameof(key));
        }

        var result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ key[i % key.Length]);
        }

        return result;
    }

    /// <summary>
    /// Encrypts or decrypts a string using XOR encryption.
    /// </summary>
    public static string TransformString(string input, ReadOnlySpan<byte> key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var transformedBytes = Transform(inputBytes, key);
        return Convert.ToBase64String(transformedBytes);
    }
}

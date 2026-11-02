using System.Security.Cryptography;
using System.Text;

namespace NetCoreTemplate.Infrastructure.Security;

public static class EncryptionHelper
{
    private const int Keysize = 256;
    private const int DerivationIterations = 1000;

    public static string Encrypt(string plainText, string passPhrase)
    {
        var saltStringBytes = RandomNumberGenerator.GetBytes(Keysize / 8);
        var ivStringBytes = RandomNumberGenerator.GetBytes(Keysize / 8);

        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
            passPhrase,
            saltStringBytes,
            DerivationIterations,
            HashAlgorithmName.SHA256,
            Keysize / 8);

        using var symmetricKey = Aes.Create();
        symmetricKey.BlockSize = 128;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;

        using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        cryptoStream.FlushFinalBlock();

        var cipherTextBytes = saltStringBytes.Concat(ivStringBytes).ToArray().Concat(memoryStream.ToArray()).ToArray();
        return Convert.ToBase64String(cipherTextBytes);
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8 * 2).ToArray();

        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
            passPhrase,
            saltStringBytes,
            DerivationIterations,
            HashAlgorithmName.SHA256,
            Keysize / 8);

        using var symmetricKey = Aes.Create();
        symmetricKey.BlockSize = 128;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;

        using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
        using var memoryStream = new MemoryStream(cipherTextBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);

        return streamReader.ReadToEnd();
    }
}

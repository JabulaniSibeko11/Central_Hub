using System.Security.Cryptography;
using System.Text;

namespace Central_Hub.Services.Security
{
    /// <summary>
    /// AES-256-CBC + HMAC-SHA256 authenticated encryption.
    /// Key loaded from appsettings — never hardcoded.
    /// Output layout: Base64( IV[16] + HMAC[32] + Cipher[n] )
    /// </summary>
    public interface IAesEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        bool IsEncrypted(string value);   // starts with "ENC:"
    }

    public class AesEncryptionService : IAesEncryptionService
    {
        private const string Prefix = "ENC:";
        private readonly byte[] _aesKey;
        private readonly byte[] _hmacKey;

        public AesEncryptionService(IConfiguration config)
        {
            var aes = config["Encryption:AesKey"]
                ?? throw new InvalidOperationException("Encryption:AesKey missing");
            var hmac = config["Encryption:HmacKey"]
                ?? throw new InvalidOperationException("Encryption:HmacKey missing");

            _aesKey = Convert.FromBase64String(aes);
            _hmacKey = Convert.FromBase64String(hmac);

            if (_aesKey.Length != 32)
                throw new InvalidOperationException("AesKey must be 32 bytes (256-bit)");
            if (_hmacKey.Length < 32)
                throw new InvalidOperationException("HmacKey must be at least 32 bytes");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            if (IsEncrypted(plainText)) return plainText; // already encrypted

            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var enc = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var cipher = enc.TransformFinalBlock(bytes, 0, bytes.Length);

            var payload = aes.IV.Concat(cipher).ToArray();
            var mac = HMACSHA256(payload);

            // IV + MAC + Cipher
            var result = aes.IV.Concat(mac).Concat(cipher).ToArray();
            return Prefix + Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            if (!IsEncrypted(cipherText)) return cipherText; // plain text — pass through

            var raw = Convert.FromBase64String(cipherText[Prefix.Length..]);
            if (raw.Length < 48) throw new CryptographicException("Invalid ciphertext length.");

            var iv = raw[..16];
            var mac = raw[16..48];
            var cipher = raw[48..];

            var expected = HMACSHA256(iv.Concat(cipher).ToArray());
            if (!CryptographicOperations.FixedTimeEquals(mac, expected))
                throw new CryptographicException("Integrity check failed — data may have been tampered.");

            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var dec = aes.CreateDecryptor();
            var plain = dec.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }

        public bool IsEncrypted(string value) =>
            !string.IsNullOrEmpty(value) && value.StartsWith(Prefix);

        private byte[] HMACSHA256(byte[] data)
        {
            using var hmac = new HMACSHA256(_hmacKey);
            return hmac.ComputeHash(data);
        }
    }
}

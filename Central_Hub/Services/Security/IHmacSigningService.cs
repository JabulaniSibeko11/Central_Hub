using System.Security.Cryptography;
using System.Text;

namespace Central_Hub.Services.Security
{
    /// <summary>
    /// Per-request HMAC-SHA256 signing.
    ///
    /// Client sends:
    ///   X-Company-Code : {companyId}
    ///   X-Timestamp    : Unix UTC seconds  (±5 min tolerance)
    ///   X-Nonce        : random GUID
    ///   X-Signature    : Base64( HMAC-SHA256( "{companyId}|{ts}|{nonce}|{METHOD}|{/path}" ) )
    ///
    /// Signing key = PBKDF2-SHA256( licenseKey, "DECLARIFY_HUB_{companyId}", 50000, 32 )
    /// </summary>
    public interface IHmacSigningService
    {
        bool ValidateRequest(int companyId, string licenseKey,
                               string timestamp, string nonce, string signature,
                               string httpMethod, string path);

        string GenerateSignature(int companyId, string licenseKey,
                                 string timestamp, string nonce,
                                 string httpMethod, string path);
    }

    public class HmacSigningService : IHmacSigningService
    {
        private const int ToleranceSeconds = 300; // 5 minutes

        public bool ValidateRequest(int companyId, string licenseKey,
                                    string timestamp, string nonce, string signature,
                                    string httpMethod, string path)
        {
            // 1 — timestamp drift
            if (!long.TryParse(timestamp, out long ts)) return false;
            var drift = Math.Abs((DateTime.UtcNow - DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime).TotalSeconds);
            if (drift > ToleranceSeconds) return false;

            // 2 — nonce present
            if (string.IsNullOrWhiteSpace(nonce)) return false;

            // 3 — constant-time compare
            var expected = GenerateSignature(companyId, licenseKey, timestamp, nonce, httpMethod, path);
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(signature)) return false;

            try
            {
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(expected),
                    Convert.FromBase64String(signature));
            }
            catch { return false; }
        }

        public string GenerateSignature(int companyId, string licenseKey,
                                        string timestamp, string nonce,
                                        string httpMethod, string path)
        {
            var message = $"{companyId}|{timestamp}|{nonce}|{httpMethod.ToUpper()}|{path.ToLowerInvariant()}";
            var salt = Encoding.UTF8.GetBytes($"DECLARIFY_HUB_{companyId}");
            var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(licenseKey ?? string.Empty),
                salt, 50_000, HashAlgorithmName.SHA256, 32);

            using var hmac = new HMACSHA256(keyBytes);
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
        }
    }
}

using Central_Hub.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Central_Hub.Services
{
    public interface ILicenseService
    {

        string GenerateLicenseKey(ClientCompany company);
        bool validateLicenseKey(string LIcenseKey,string emailDomain);
        DateTime CalculateLicenseExpiryDate();
        DateTime CalculateCreditExpiryDate();

        LicenseStatus CalculateLicenseStatus(DateTime ExpiryDate);
    }

    public class LicenseService : ILicenseService
    {
        private const string SECRET_SALT = "DECLARIFY_2026_SECURE_KEY";

        public string GenerateLicenseKey(ClientCompany company)
        {
            string uniqueString = $"{company.CompanyName}{company.RegistrationNumber}{company.EmailDomain}{DateTime.UtcNow.Ticks}";

            using (var sha256 = SHA256.Create()) 
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(uniqueString + SECRET_SALT));
                string hash = Convert.ToBase64String(hashBytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "")
                    .ToUpper();

                string key = $"DECL-{hash.Substring(0, 4)}-{hash.Substring(4, 4)}-{hash.Substring(8, 4)}-{hash.Substring(12, 4)}";
                return key;
            }
           

        }

        public bool validateLicenseKey(string LicenseKey, string emailDomain)
        {
           if(string.IsNullOrEmpty(LicenseKey)) return false;
           var parts = LicenseKey.Split('-');

            if(parts.Length != 5 || parts[0] != "DECL")
            {
                return false;
            }

            for (int i =1; i < parts.Length; i++)
            {
                if(parts[i].Length != 4)
                {
                    return false;
                }
            }
            return true;
        }



        public DateTime CalculateLicenseExpiryDate()
        {
            DateTime today = DateTime.UtcNow;

            int targetYear = today.Month == 12 ? today.Year + 1 : today.Year;
             return new DateTime(targetYear, 2,1,23,59,59,DateTimeKind.Utc);
        }

        public DateTime CalculateCreditExpiryDate()
        {
            return DateTime.UtcNow.AddMonths(12);
        }

        public LicenseStatus CalculateLicenseStatus(DateTime ExpiryDate)
        {
            DateTime today = DateTime.UtcNow;
         int daysUntilExpiry = (ExpiryDate - today).Days;
          
            if (daysUntilExpiry < 0)
            {
                return LicenseStatus.Expired;
            }

            if (daysUntilExpiry <= 30)
            {
                return LicenseStatus.ExpiringSoon;
            }

            return LicenseStatus.Active;


        }
    }
}

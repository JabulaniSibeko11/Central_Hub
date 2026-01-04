namespace Central_Hub.Models.DTO
{
    public class CompanyInformationResponse
    {
        public string CompanyName { get; set; }
        public string CompanyRegistration { get; set; }
        public string? Domain { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}

namespace Central_Hub.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct);

    }
}

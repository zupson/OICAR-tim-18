namespace SpendlyWebAPI.Dal.Repo
{
    public interface IEmailService
    {
        Task SendInviteAsync(string toEmail, string invitationToken);
    }
}
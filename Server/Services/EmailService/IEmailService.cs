namespace Server.Services.EmailService;

public interface IEmailService
{
    public Task SendRecoveryEmail(string address);
    public Task SendEmailConfirmationEMail();
    public Task SendNewsEMail(Guid newsGuid);
}
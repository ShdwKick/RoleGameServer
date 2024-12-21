using Server.Data;

namespace GraphQLServer.Services.RecoveryService;

public interface IEmailService
{
    Task<bool> SendRecoveryEmail(string address);
    Task<bool> SendEmailConfirmationEMail();
}
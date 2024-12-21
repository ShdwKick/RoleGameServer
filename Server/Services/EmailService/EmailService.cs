using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;
using Server.Data.Helpers;

namespace GraphQLServer.Services.RecoveryService;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    private readonly DataBaseConnection _dataBaseConnection;

    public EmailService(IHttpContextAccessor httpContextAccessor, IMemoryCache cache,
        IHttpClientFactory httpClientFactory, DataBaseConnection dataBaseConnection)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
        _dataBaseConnection = dataBaseConnection;
    }

    public async Task<bool> SendRecoveryEmail(string address)
    {
        try
        {
            var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_email == address);

            if (user == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            int code = Helpers.GenerateCode();

            await _dataBaseConnection.RecoveryCodes.AddAsync(new RecoveryCodes()
            {
                c_email = address,
                n_code = code,
                id = Guid.NewGuid(),
                d_expiration_time = DateTime.UtcNow.AddMinutes(5),
            });

            string url = $"{ServerSecretData.GetBaseUrl()}:7111/EmailSender/SendRecoveryMail";


            HttpResponseMessage response =
                await _httpClient.PostAsync(url, Helpers.GenerateEmailCodeJson(address, code));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Email sent successfully.");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to send email. Status code: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while sending email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendEmailConfirmationEMail()
    {
        try
        {
            var user = await Helpers.GetUserFromHeader(_dataBaseConnection, _httpContextAccessor);

            if (user == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            int code = Helpers.GenerateCode();

            await _dataBaseConnection.RecoveryCodes.AddAsync(new RecoveryCodes()
            {
                c_email = user.c_email,
                n_code = code,
                id = Guid.NewGuid(),
                d_expiration_time = DateTime.UtcNow.AddMinutes(5),
            });

            string url = $"{ServerSecretData.GetBaseUrl()}:7111/EmailSender/SendConfirmationMail";

            HttpResponseMessage response =
                await _httpClient.PostAsync(url, Helpers.GenerateEmailCodeJson(user.c_email, code));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Email sent successfully.");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to send email. Status code: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while sending email: {ex.Message}");
            return false;
        }
    }
}
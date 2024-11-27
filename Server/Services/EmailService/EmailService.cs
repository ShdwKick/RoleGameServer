using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Helpers;

namespace Server.Services.EmailService;

public class EmailService : IEmailService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly DataBaseConnection _dataBaseConnection;
    public EmailService(IHttpContextAccessor httpContextAccessor,DataBaseConnection dataBaseConnection, HttpClient httpClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _dataBaseConnection = dataBaseConnection;
        _httpClient = httpClient;
    }
    
    
    public async Task SendRecoveryEmail(string address)
    {
        try
        {
            var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_email == address);

            if (user == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            Random random = new Random();
            int code = random.Next(100000, 999999);

            await _dataBaseConnection.RecoveryCodes.AddAsync(new RecoveryCodes()
            {
                c_email = address,
                n_code = code,
                id = Guid.NewGuid(),
                d_expiration_time = DateTime.UtcNow.AddMinutes(5),
            });

            string url = $"{ServerSecretData.GetBaseUrl()}:7111/EmailSender/SendRecoveryMail";

            // Создаем объект RecoveryMessage и сериализуем его в JSON
            var recoveryMessage = new
            {
                address = address,
                code = code.ToString()
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(recoveryMessage),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await _httpClient.PostAsync(url, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Email sent successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to send email. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while sending email: {ex.Message}");
        }
    }

    public async Task SendEmailConfirmationEMail()
    {
        try
        {
            var user = await Helpers.GetUserFromHeader(_dataBaseConnection, _httpContextAccessor);

            if (user == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            Random random = new Random();
            int code = random.Next(100000, 999999);

            await _dataBaseConnection.RecoveryCodes.AddAsync(new RecoveryCodes()
            {
                c_email = user.c_email,
                n_code = code,
                id = Guid.NewGuid(),
                d_expiration_time = DateTime.UtcNow.AddMinutes(5),
            });

            string url = $"{ServerSecretData.GetBaseUrl()}:7111/EmailSender/SendConfirmationMail";

            // Подготовка содержимого запроса с адресом электронной почты в теле
            var emailContent = new StringContent(
                JsonSerializer.Serialize(user.c_email),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await _httpClient.PostAsync(url, emailContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Email sent successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to send email. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while sending email: {ex.Message}");
        }
    }
    
    public async Task SendNewsEMail(Guid newsGuid)
    {
        try
        {
            var user = await Helpers.GetUserFromHeader(_dataBaseConnection, _httpContextAccessor);

            if (user == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            Random random = new Random();
            int code = random.Next(100000, 1000000);

            await _dataBaseConnection.RecoveryCodes.AddAsync(new RecoveryCodes()
            {
                c_email = user.c_email,
                n_code = code,
                id = Guid.NewGuid(),
                d_expiration_time = DateTime.UtcNow.AddMinutes(5),
            });

            string url = $"{ServerSecretData.GetBaseUrl()}:7111/EmailSender/SendConfirmationMail";

            // Подготовка содержимого запроса с адресом электронной почты в теле
            var emailContent = new StringContent(
                JsonSerializer.Serialize(user.c_email),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await _httpClient.PostAsync(url, emailContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Email sent successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to send email. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while sending email: {ex.Message}");
        }
    }
}
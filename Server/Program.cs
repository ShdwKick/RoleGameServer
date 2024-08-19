using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebSockets;
using Server.Data;
using Microsoft.AspNetCore.Hosting;

namespace GraphQLServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Получаем ключ безопасности из ServerSecretData
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ServerSecretData.GetSecurityKey()));

            // Настройка аутентификации JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = securityKey,
                        ValidateIssuer = true,
                        ValidIssuer = ServerSecretData.GetIssuer(),
                        ValidateAudience = true,
                        ValidAudience = ServerSecretData.GetAudience(),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            return Task.FromResult("AUTH_FAILED_PROBLEM");
                        }
                    };
                });

            // Настройка авторизации
            builder.Services.AddAuthorization();

            // Добавление GraphQL
            builder.Services.AddScoped<Query>();
            builder.Services.AddScoped<Mutation>();
            builder.Services.AddScoped<Subsription>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();
            builder.Services.AddWebSockets(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(120);
            });

            builder.Services.AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddSubscriptionType<Subsription>()
                .AddInMemorySubscriptions()
                .AddAuthorization();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5000);
                options.ListenAnyIP(5001, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWebSockets();

            app.MapGraphQL();
            app.Run();
        }
    }
}

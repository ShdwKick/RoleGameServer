﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GraphQLServer.Services.ChatService;
using GraphQLServer.Services.RecoveryService;
using GraphQLServer.Services.RoleService;
using GraphQLServer.Services.RoomService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebSockets;
using Server.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Server.Services;

namespace GraphQLServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            // Настройка авторизации
            builder.Services.AddAuthorization();
            
            builder.Services.AddScoped<Query>();
            builder.Services.AddScoped<Mutation>();
            builder.Services.AddScoped<Subsription>();
            builder.Services.AddScoped<DataBaseConnection>(); 
            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

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
            
            
            
            
            
            //var key = builder.Configuration["AppSettings:ServerKey"];
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
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

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5000);
                options.ListenAnyIP(5001, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            
            var app = builder.Build();

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWebSockets();

            app.MapGraphQL();
            app.Run();
        }
    }
}

﻿using BLL.Abstraction;
using BLL.Implementation;
using BLL.Jwt;
using Domain.Models;
using DAL;
using DAL.Abstraction;
using DAL.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Reflection;

namespace BLL.DI
{
    public static class DependencyContainer
    {
        public static void RegisterDependency(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<DatabaseSettings>(configuration.GetSection("MindTrackerDatabase"));
            services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));

            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                IServiceProvider serviceProvider = services.BuildServiceProvider();

                JwtOptions jwtOptions = serviceProvider.GetService<IOptions<JwtOptions>>()!.Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = jwtOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                };
            });

            services.AddSingleton<IMongoClient, MongoClient>((serviceProvider) =>
            {
                DatabaseSettings dbSettings = serviceProvider.GetService<IOptions<DatabaseSettings>>()!.Value;

                return new MongoClient(dbSettings.ConnectionString);
            });

            services.AddScoped<IMongoDatabase>((serviceProvider) =>
            {
                DatabaseSettings dbSettings = serviceProvider.GetService<IOptions<DatabaseSettings>>()!.Value;
                IMongoClient mongoClient = serviceProvider.GetService<IMongoClient>()!;

                return mongoClient.GetDatabase(dbSettings.DatabaseName);
            });

            services.AddScoped<IMongoCollection<Account>>((serviceProvider) =>
            {
                IMongoDatabase mongoDatabase = serviceProvider.GetService<IMongoDatabase>()!;

                return mongoDatabase.GetCollection<Account>("Accounts");

            });
            services.AddScoped<IMongoCollection<MoodMark>>((serviceProvider) =>
            {
                IMongoDatabase mongoDatabase = serviceProvider.GetService<IMongoDatabase>()!;

                return mongoDatabase.GetCollection<MoodMark>("MoodMarks");

            });

            services.AddTransient<IMoodMarksRepository, MoodMarksRepository>();
            services.AddTransient<IAccountRepository, AccountRepository>();

            services.AddTransient<IMoodMarksService, MoodMarksService>();
            services.AddTransient<IAccountService, AccountService>();

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }
    }
}

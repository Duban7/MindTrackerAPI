using BLL.Abstraction;
using BLL.Implementation;
using BLL.Jwt;
using Domain.Models;
using DAL;
using DAL.Abstraction;
using DAL.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using FluentValidation;
using MindTrackerServer.Validators;
using System.Reflection;
using BLL.Gmail;

namespace BLL.DI
{
    /// <summary>
    /// 
    /// </summary>
    public static class DependencyContainer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void RegisterDependency(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://localhost",
                                        "capacitor://localhost",
                                        "ionic://localhost",
                                        "http://localhost",
                                        "http://localhost:8080",
                                        "http://localhost:8100")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                });
            });

            services.Configure<DatabaseSettings>(configuration.GetSection("MindTrackerCloudDatabase"));
            services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
            services.Configure<CloudinaryDotNet.Account>(configuration.GetSection("CloudinaryAccount"));
            services.Configure<GmailOptions>(configuration.GetSection("GmailOptions"));

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
            services.AddScoped<IMongoCollection<MoodActivity>>((serviceProvider) =>
            {
                IMongoDatabase mongoDatabase = serviceProvider.GetService<IMongoDatabase>()!;

                return mongoDatabase.GetCollection<MoodActivity>("MoodActivities");

            });
            services.AddScoped<IMongoCollection<MoodGroup>>((serviceProvider) =>
            {
                IMongoDatabase mongoDatabase = serviceProvider.GetService<IMongoDatabase>()!;

                return mongoDatabase.GetCollection<MoodGroup>("MoodGroups");

            });

            services.AddTransient<IMoodMarksRepository, MoodMarksRepository>();
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<IMoodGroupRepository, MoodGroupRepository>();
            services.AddTransient<IMoodActivityRepository, MoodActivityRepository>();

            services.AddScoped<IValidator<Account>, AccountValidator>();

            services.AddTransient<IMoodMarksService, MoodMarksService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IGroupSchemaService, GroupSchemaService>();
            services.AddTransient<ICloudinaryService, CloudinaryService>();

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MindTrackerAPI", Version = "v1" });
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
        }
    }
}

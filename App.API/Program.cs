using System.Globalization;
using System.Text;
using App.Application.Extensions;
using App.Application.Services.LocalizationService;
using App.Application.Services.ResponseService;
using App.Application.Services.SeederService;
using App.Application.Services.TokenService;
using App.Infrastructure.DbContext;
using App.Infrastructure.Repositories;
using Core.Entities;
using Core.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

namespace App.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

        builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        var Configuration = builder.Configuration;
        builder.Services.AddApplicationServices();

        #region Add CORS services

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        #endregion Add CORS services
       
        #region Configure Swagger

        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });

        #endregion
        
        #region Configure JWT
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters(){
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });
        #endregion
        
        #region injecting interfaces and their implementations
        
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.AddScoped<IResponseService, ResponseService>();
        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddHttpContextAccessor();
        
        #endregion
        
        #region Add Identity password seeting
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;            // No digit required
                options.Password.RequiredLength = 6;              // Minimum length of 6
                options.Password.RequireNonAlphanumeric = false;  // No special character required
                options.Password.RequireUppercase = false;        // No uppercase letter required
                options.Password.RequireLowercase = false;        // No lowercase letter required
                options.Password.RequiredUniqueChars = 1;
            })
            .AddEntityFrameworkStores<AppDbContext>()  
            .AddDefaultTokenProviders();  
        #endregion
        
        #region DataBase Config
        
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options
                .UseSqlServer(connectionString)
                .LogTo(Console.WriteLine, LogLevel.Information);
        });

        #endregion
        
        #region Localization

        builder.Services.AddLocalization();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSingleton<IStringLocalizerFactory, JSonStringLocalizerFactory>();
        builder.Services.AddMvc()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(JSonStringLocalizerFactory));
            });

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("ar-EG"),
                new CultureInfo("en-US")
            };
            options.DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0]);
            options.SupportedCultures = supportedCultures;
        });

        #endregion

        
        var app = builder.Build();

        #region RolesSeeder

        using var scope = app.Services.CreateScope();
        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await DefaultRoles.SeedAsync(roleManager);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
        #endregion
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        var supportedCultures = new[] { "ar-EG", "en-US" };
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures);

        app.UseCors("AllowAll");
        app.UseHttpsRedirection();

        
        app.UseRequestLocalization(localizationOptions);
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
       
        app.Run();
    }
}
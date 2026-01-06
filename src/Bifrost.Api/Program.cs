using System.Runtime.CompilerServices;
using System.Text;
using Bifrost.Api.Endpoints;
using Bifrost.Core.Services;
using Bifrost.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

[assembly: InternalsVisibleTo("Bifrost.Integration.Tests")]

namespace Bifrost.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        // Configure JWT Authentication for Supabase
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer();

        // Configure JWT options post-construction to allow test overrides
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, configuration) =>
            {
                var jwtIssuer = configuration["Jwt:Issuer"]
                    ?? throw new InvalidOperationException("JWT Issuer is not configured.");
                var jwtAudience = configuration["Jwt:Audience"]
                    ?? throw new InvalidOperationException("JWT Audience is not configured.");
                var jwtKey = configuration["Jwt:Key"];

                // Check if we're using a symmetric key or should use Supabase's JWKS
                if (!string.IsNullOrEmpty(jwtKey) && !jwtKey.StartsWith("eyJ"))
                {
                    // Use symmetric key (for testing or custom JWT)
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                }
                else
                {
                    // Use Supabase JWKS endpoint for token validation
                    options.Authority = jwtIssuer;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        ClockSkew = TimeSpan.Zero
                    };
                }
            });

        builder.Services.AddAuthorization();

        // Configure CORS to allow frontend requests
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Register Infrastructure Services and DbContext
        builder.Services.AddInfrastructure(builder.Configuration);

        // Register Core Services
        builder.Services.AddScoped<IJobService, JobService>();
        builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
        builder.Services.AddScoped<IApplicationNoteService, ApplicationNoteService>();
        builder.Services.AddScoped<IPreferencesService, PreferencesService>();

        var app = builder.Build();


        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/docs", options =>
            {
                options.WithTitle("Bifrost API - Job Application Tracker")
                    .WithTheme(ScalarTheme.Purple)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        /// Configure the HTTP request pipeline.
        app.UseHttpsRedirection();

        // Enable CORS before authorization
        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        // Map API endpoints
        app.MapJobEndpoints();
        app.MapJobApplicationEndpoints();
        app.MapApplicationNoteEndpoints();
        app.MapPreferencesEndpoints();

        app.Run();
    }
}

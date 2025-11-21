using System.Runtime.CompilerServices;
using Bifrost.Api.Endpoints;
using Bifrost.Core.Services;
using Bifrost.Infrastructure;
using Scalar.AspNetCore;

[assembly: InternalsVisibleTo("Bifrost.Integration.Tests")]

namespace Bifrost.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
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

        app.UseAuthorization();

        // Map API endpoints
        app.MapJobEndpoints();
        app.MapJobApplicationEndpoints();
        app.MapApplicationNoteEndpoints();
        app.MapPreferencesEndpoints();

        app.Run();
    }
}

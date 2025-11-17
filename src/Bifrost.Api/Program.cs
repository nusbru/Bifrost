using Bifrost.Api.Endpoints;
using Bifrost.Core.Services;
using Bifrost.Infrastructure;
using Scalar.AspNetCore;

namespace Bifrost.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

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

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options
                .WithTitle("Bifrost - Job Application Tracker API")
                .WithTheme(ScalarTheme.BluePlanet));
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        // Map API endpoints
        app.MapJobEndpoints();
        app.MapJobApplicationEndpoints();
        app.MapApplicationNoteEndpoints();
        app.MapPreferencesEndpoints();

        app.Run();
    }
}

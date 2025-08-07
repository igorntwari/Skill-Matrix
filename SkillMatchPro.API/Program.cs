using Microsoft.EntityFrameworkCore;
using SkillMatchPro.API.GraphQL;
using SkillMatchPro.Infrastructure.Data;
using DotNetEnv;
using SkillMatchPro.API.Services;

namespace SkillMatchPro.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Load environment variables FIRST thing inside Main
        // Load .env from parent directory
        Env.Load("../.env");

        var builder = WebApplication.CreateBuilder(args);
        // Get connection string from environment or config
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
            ?? builder.Configuration.GetConnectionString("DefaultConnection");


        // Override JWT settings from environment
        builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? builder.Configuration["Jwt:Key"];
        builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? builder.Configuration["Jwt:Issuer"];
        builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? builder.Configuration["Jwt:Audience"];

        // Add services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<JwtService>();


        // Add GraphQL
        builder.Services
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutations>();

        // Add PostgreSQL with environment variable
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        var app = builder.Build();

        // Configure pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapGraphQL();
        app.MapControllers();

        app.Run();
    }
}
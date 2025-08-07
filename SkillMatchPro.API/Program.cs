using Microsoft.EntityFrameworkCore;
using SkillMatchPro.API.GraphQL;
using SkillMatchPro.Infrastructure.Data;
using DotNetEnv;
using SkillMatchPro.API.Services;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;  
using Microsoft.IdentityModel.Tokens;                 
using System.Text;

namespace SkillMatchPro.API;

public class Program
{
    public static void Main(string[] args)
    {
       
        Env.Load("../.env");

        var builder = WebApplication.CreateBuilder(args);
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
            ?? builder.Configuration.GetConnectionString("DefaultConnection");


        builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? builder.Configuration["Jwt:Key"];
        builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? builder.Configuration["Jwt:Issuer"];
        builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? builder.Configuration["Jwt:Audience"];

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<JwtService>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")))
    };
});
        builder.Services.AddAuthorization();

        builder.Services
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutations>()
            .AddAuthorization()
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

        // Add PostgreSQL with environment variable
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            var user = context.User;
            Console.WriteLine($"User authenticated: {user.Identity?.IsAuthenticated}");
            Console.WriteLine($"User name: {user.Identity?.Name}");
            await next();
        });

        // Configure pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication(); 
        app.UseAuthorization();
        app.MapGraphQL();
        app.MapControllers();

        app.Run();
    }
}
using Microsoft.EntityFrameworkCore;
using SkillMatchPro.API.GraphQL;
using SkillMatchPro.Infrastructure.Data;
using DotNetEnv;
using SkillMatchPro.API.Services;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Events;
using SkillMatchPro.Application.Services;
using SkillMatchPro.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using SkillMatchPro.API.Hubs;
namespace SkillMatchPro.API;

public class Program
{
    public static void Main(string[] args)
    {

        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/skillmatchpro-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
        try
        {
            Log.Information("Starting SkillMatch Pro API");
            Env.Load("../.env");

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpContextAccessor();

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
            builder.Services.AddScoped<IMatchingService, MatchingService>();
            builder.Services.AddScoped<IAdvancedMatchingService, AdvancedMatchingService>();
            builder.Services.AddScoped<ITeamOptimizationService, TeamOptimizationService>();
            builder.Services.AddScoped<INotificationService>(provider =>
            {
                var hubContext = provider.GetRequiredService<IHubContext<ProjectNotificationHub>>();
                var logger = provider.GetRequiredService<ILogger<SignalRNotificationService>>();

                return new SignalRNotificationService(hubContext.Clients, logger) as INotificationService;
            });

            // Register application services
            builder.Services.AddScoped<IAllocationService, AllocationService>();
            // We'll implement NotificationService later when needed
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
            builder.Services.AddAuthorization(options =>
            {
                // Policy for Admins only
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                // Policy for Managers and above
                options.AddPolicy("ManagerOrAbove", policy =>
                    policy.RequireRole("Admin", "Manager"));

                // Policy for authenticated employees
                options.AddPolicy("EmployeeOrAbove", policy =>
                    policy.RequireAuthenticatedUser());
            });

            builder.Services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutations>()
                .AddAuthorization()
                .AddErrorFilter<ErrorFilter>()
                .AddFiltering()        
                .AddSorting()        
                .AddProjections()
                .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

            // Add PostgreSQL with environment variable
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddSignalR();

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
            app.MapHub<ProjectNotificationHub>("/hubs/notifications");
            app.MapGraphQL();
            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
        }
        finally
        {
            Log.CloseAndFlush();
        }


    }
}
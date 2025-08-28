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
using HotChocolate.AspNetCore;

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
            builder.Services.AddSignalR();
            builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
            builder.Services.AddScoped<IHubClients>(provider =>
                provider.GetRequiredService<IHubContext<ProjectNotificationHub>>().Clients);

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

        // Add this for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
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



            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500"); // Your test client origin
                });
            });

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
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<ProjectNotificationHub>("/hubs/notifications");
            app.MapGraphQL()
   .WithOptions(new GraphQLServerOptions
   {
       Tool = {
           Enable = app.Environment.IsDevelopment()
       }
   });
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
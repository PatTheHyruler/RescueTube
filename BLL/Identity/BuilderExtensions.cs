using System.Text;
using BLL.Identity.Options;
using BLL.Identity.Services;
using ConfigDefaults;
using DAL.EF.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Validation;
using JwtBearerOptions = BLL.Identity.Options.JwtBearerOptions;

namespace BLL.Identity;

public static class BuilderExtensions
{
    public static IServiceCollection AddCustomIdentity(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var jwtSection = configuration.GetRequiredSection(JwtBearerOptions.Section);
        services
            .AddOptionsFull<JwtBearerOptions>(jwtSection)
            .AddOptionsFull<RegistrationOptions>(configuration.GetSection(RegistrationOptions.Section))
            .AddOptions<IdentityOptions>()
            .Configure(options => options.Password.RequiredLength = IdentityDefaults.PasswordMinLength)
            .Bind(configuration.GetSection("Identity"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        var jwtOptions = jwtSection.Get<JwtBearerOptions>()
                         ?? throw new OptionsValidationException(JwtBearerOptions.Section, typeof(JwtBearerOptions),
                             new[] { "Failed to read JWT options" });

        services.AddHttpContextAccessor(); // This is added in .AddIdentity(), but not in .AddIdentityCore(), so adding it manually just in case it doesn't get registered elsewhere.
        services.AddIdentityCore<Domain.Identity.User>()
            .AddRoles<Domain.Identity.Role>()
            .AddEntityFrameworkStores<AbstractAppDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<SignInManager<Domain.Identity.User>>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ClockSkew = TimeSpan.FromSeconds(5),
            };
        });

        services.AddIdentityUowAndServices();

        return services;
    }

    private static void AddIdentityUowAndServices(this IServiceCollection services)
    {
        services.AddIdentityServices();
    }

    private static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddScoped<UserService>()
            .AddScoped<TokenService>();
    }
}
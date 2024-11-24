using ApiGateway.Attributes;
using ApiGateway.DependencyInjections.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiGateway.DependencyInjections.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddAuthenticationApiGateway(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var jwtOptions = new JwtOptions();
                configuration.GetSection(nameof(JwtOptions)).Bind(jwtOptions);

                options.SaveToken = true;
                var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    // Custom here by Attributes and after add EventsType
                    //OnTokenValidated = (context) =>
                    //{
                    //    return Task.CompletedTask;
                    //},
                    OnAuthenticationFailed = (context) =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.TryAdd("IS-TOKEN-EXPIRED", "true");
                        }

                        return Task.CompletedTask;
                    },
                };

                options.EventsType = typeof(CustomJwtBearEvents);
            });

            services.AddAuthorization(policy =>
            {
                policy.AddPolicy("DefaultAuthentication", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddScoped<CustomJwtBearEvents>();
            return services;
        }
    }
}
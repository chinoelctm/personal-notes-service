using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PersonalNotes.Api.Exceptions;
using PersonalNotes.Api.Options;

namespace PersonalNotes.Api.Extensions
{
    public static class PresentationExtensions
    {
        public const string FrontCorsPolicy = "FrontCorsPolicy";

        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // Enums como texto en request/response (ej. "High" en vez de 3)
            services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            services.AddCors(options =>
                options.AddPolicy(FrontCorsPolicy, policy => policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("Location")));

            services.AddCookieJwtAuthentication();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title       = "PersonalNotes API",
                    Version     = "v1",
                    Description = "API de notas y documentación personal: árbol de carpetas, documentos con prioridad y respaldos. Requiere la cookie auth_token de personal-notes-auth-services."
                });
            });

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }

        /// <summary>
        /// Toda la API exige sesión (política por defecto); solo /health es anónimo. El access token llega en la
        /// cookie HttpOnly auth_token y se valida con la clave pública del servicio de auth (RS256, 15 min).
        /// </summary>
        private static IServiceCollection AddCookieJwtAuthentication(this IServiceCollection services)
        {
            services.AddOptions<JwtValidationOptions>()
                .BindConfiguration(JwtValidationOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtValidationOptions>>((options, jwt) =>
                {
                    var rsa = RSA.Create();
                    rsa.ImportFromPem(jwt.Value.PublicKeyPem.Replace("\\n", "\n"));

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwt.Value.Issuer,
                        ValidAudience = jwt.Value.Audience,
                        IssuerSigningKey = new RsaSecurityKey(rsa),
                        ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                    var cookieName = jwt.Value.AccessTokenCookie;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies[cookieName];
                            return Task.CompletedTask;
                        },
                        // El front ante este 401 llama a /api/v1/auth/refresh y reintenta
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsJsonAsync(new ProblemDetails
                            {
                                Status = StatusCodes.Status401Unauthorized,
                                Title  = "No autorizado",
                                Detail = "La sesión expiró o no es válida."
                            });
                        }
                    };
                });

            return services;
        }
    }
}

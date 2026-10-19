using System.ComponentModel.DataAnnotations;

namespace PersonalNotes.Api.Options
{
    /// <summary>
    /// Validación de los access tokens que emite personal-notes-auth-services. Esta API solo tiene la clave
    /// pública: puede verificar tokens, pero no emitirlos.
    /// </summary>
    public class JwtValidationOptions
    {
        public const string SectionName = "JwtSettings";

        [Required(ErrorMessage = "Falta JwtSettings:PublicKeyPem (clave pública de personal-notes-auth-services).")]
        public string PublicKeyPem { get; set; } = null!;

        [Required] public string Issuer { get; set; } = null!;
        [Required] public string Audience { get; set; } = null!;

        /// <summary>Cookie HttpOnly donde llega el access token (la escribe el servicio de auth).</summary>
        [Required] public string AccessTokenCookie { get; set; } = "auth_token";
    }
}

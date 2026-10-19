using System.Net;
using System.Text.RegularExpressions;

namespace PersonalNotes.Application.Helpers
{
    /// <summary>Texto visible del HTML del editor, para enviarlo a la IA sin etiquetas ni imágenes en Base64.</summary>
    public static partial class HtmlText
    {
        public static string ToPlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            // Imágenes (data URI enormes), scripts y estilos no aportan texto
            var texto = NonTextBlocks().Replace(html, " ");
            // Fin de bloque y celdas: un espacio para no pegar palabras de párrafos distintos
            texto = BlockBreaks().Replace(texto, " ");
            texto = Tags().Replace(texto, string.Empty);
            texto = WebUtility.HtmlDecode(texto);
            return Spaces().Replace(texto, " ").Trim();
        }

        [GeneratedRegex(@"<(script|style)\b[\s\S]*?</\1\s*>|<img\b[^>]*>", RegexOptions.IgnoreCase)]
        private static partial Regex NonTextBlocks();

        [GeneratedRegex(@"<\s*(br|/p|/h[1-6]|/li|/tr|/td|/th|/pre|/div|/blockquote)[^>]*>", RegexOptions.IgnoreCase)]
        private static partial Regex BlockBreaks();

        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex Tags();

        [GeneratedRegex(@"\s+")]
        private static partial Regex Spaces();
    }
}

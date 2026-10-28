using System.Text.RegularExpressions;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Helpers
{
    /// <summary>
    /// Texto de un documento Markdown para la IA. La sintaxis (#, **, listas, tablas) se conserva porque la IA la
    /// entiende; se quitan las imágenes (pueden ser data URI enormes) y de los enlaces queda solo su texto.
    /// </summary>
    public static partial class MarkdownText
    {
        public static string ToPlainText(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;

            var texto = Images().Replace(markdown, "$1");
            texto = Links().Replace(texto, "$1");
            // HTML incrustado en el Markdown: mismo tratamiento que las notas
            return HtmlText.ToPlainText(texto);
        }

        /// <summary>Texto visible de un documento según su tipo.</summary>
        public static string ToPlainText(string? content, DocumentType type)
            => type == DocumentType.Markdown ? ToPlainText(content) : HtmlText.ToPlainText(content);

        [GeneratedRegex(@"!\[([^\]]*)\]\([^)]*\)")]
        private static partial Regex Images();

        [GeneratedRegex(@"\[([^\]]+)\]\([^)]*\)")]
        private static partial Regex Links();
    }
}

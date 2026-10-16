namespace PersonalNotes.Domain.ValueObjects
{
    /// <summary>
    /// Proyección liviana de un documento (sin título ni contenido) para calcular estadísticas sin
    /// traer el HTML, que puede incluir imágenes en Base64.
    /// </summary>
    public record DocumentStamp(string Id, string FolderId, Priority Priority, DateTime UpdatedOn);
}

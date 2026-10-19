using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Api.Models.Document.AddDocument
{
    /// <param name="FolderId">Carpeta donde se publica el documento.</param>
    /// <param name="Title">Título; vacío se publica como "Sin título".</param>
    /// <param name="Content">HTML del editor (imágenes en Base64 incluidas).</param>
    /// <param name="Priority">None, Low, Medium o High.</param>
    /// <param name="Type">Rich (nota, por defecto) o Html (página HTML que se muestra en un iframe aislado).</param>
    public record AddDocumentRequest(
        string FolderId,
        string? Title,
        string? Content,
        Priority Priority,
        DocumentType? Type);
}

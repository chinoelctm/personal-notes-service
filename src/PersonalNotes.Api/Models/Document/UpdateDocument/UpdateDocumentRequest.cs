using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Api.Models.Document.UpdateDocument
{
    /// <param name="FolderId">Carpeta del documento; otra distinta lo mueve.</param>
    /// <param name="Title">Título; vacío se publica como "Sin título".</param>
    /// <param name="Content">HTML del editor (imágenes en Base64 incluidas).</param>
    /// <param name="Priority">None, Low, Medium o High.</param>
    public record UpdateDocumentRequest(
        string FolderId,
        string? Title,
        string? Content,
        Priority Priority);
}

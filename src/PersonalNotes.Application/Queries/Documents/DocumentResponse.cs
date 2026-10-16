using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Queries.Documents
{
    /// <param name="Content">HTML del editor (puede incluir imágenes en Base64).</param>
    /// <param name="CreatedOn">UTC.</param>
    /// <param name="UpdatedOn">UTC; ordena los listados por última edición.</param>
    public record DocumentResponse(
        string Id,
        string FolderId,
        string Title,
        string Content,
        Priority Priority,
        DateTime CreatedOn,
        DateTime UpdatedOn)
    {
        public static DocumentResponse From(Document document) => new(
            document.Id,
            document.FolderId,
            document.Title,
            document.Content,
            document.Priority,
            document.CreatedOn,
            document.UpdatedOn);
    }
}

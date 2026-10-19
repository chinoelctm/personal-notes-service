using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Queries.Documents
{
    /// <param name="Content">HTML del editor (Rich) o código de la página (Html).</param>
    /// <param name="Type">Rich o Html.</param>
    /// <param name="CreatedOn">UTC.</param>
    /// <param name="UpdatedOn">UTC; ordena los listados por última edición.</param>
    public record DocumentResponse(
        string Id,
        string FolderId,
        string Title,
        string Content,
        Priority Priority,
        DocumentType Type,
        DateTime CreatedOn,
        DateTime UpdatedOn)
    {
        public static DocumentResponse From(Document document) => new(
            document.Id,
            document.FolderId,
            document.Title,
            document.Content,
            document.Priority,
            document.Type,
            document.CreatedOn,
            document.UpdatedOn);
    }
}

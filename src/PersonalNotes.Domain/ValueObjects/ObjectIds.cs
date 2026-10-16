using MongoDB.Bson;

namespace PersonalNotes.Domain.ValueObjects
{
    /// <summary>Los Id de carpetas y documentos son ObjectId de Mongo y viajan como texto de 24 caracteres.</summary>
    public static class ObjectIds
    {
        public static string New() => ObjectId.GenerateNewId().ToString();

        public static bool IsValid(string? id) => id is not null && ObjectId.TryParse(id, out _);
    }
}

using System.ComponentModel.DataAnnotations;
using PersonalNotes.Domain.Options;

namespace PersonalNotes.Infrastructure.Options
{
    public class NotesOptions : INotesOptions
    {
        public const string SectionName = "Notes";

        // El árbol debe soportar al menos 3 niveles (carpeta, subcarpeta, sub-subcarpeta)
        [Range(3, 20)]
        public int MaxFolderDepth { get; set; } = 5;

        [Range(1, 24 * 30)]
        public int RecentWindowHours { get; set; } = 24;
    }
}

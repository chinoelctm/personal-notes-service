namespace PersonalNotes.Domain.Options
{
    /// <summary>Reglas del árbol de notas configurables en la sección "Notes" de appsettings.json.</summary>
    public interface INotesOptions
    {
        /// <summary>Niveles máximos del árbol (una carpeta raíz es el nivel 1). Mínimo 3.</summary>
        int MaxFolderDepth { get; }

        /// <summary>Ventana (en horas) para contar los documentos "recientes" del dashboard.</summary>
        int RecentWindowHours { get; }
    }
}

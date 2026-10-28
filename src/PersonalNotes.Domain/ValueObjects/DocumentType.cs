namespace PersonalNotes.Domain.ValueObjects
{
    /// <summary>
    /// Rich: nota del editor enriquecido. Html: página HTML propia (con su CSS y JavaScript) que el front muestra
    /// en un iframe aislado. Markdown: texto en formato Markdown que el front muestra como documento. Los
    /// documentos guardados antes de existir el campo se leen como Rich.
    /// </summary>
    public enum DocumentType
    {
        Rich,
        Html,
        Markdown
    }
}

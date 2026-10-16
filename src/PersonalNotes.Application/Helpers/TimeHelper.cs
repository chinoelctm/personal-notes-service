namespace PersonalNotes.Application.Helpers
{
    public class TimeHelper
    {
        /// <summary>
        /// Instante actual en UTC, para auditoría (createdOn/updatedOn). Mongo guarda las fechas en UTC y trata
        /// un DateTime sin Kind como hora local del servidor, que en un contenedor no es la del usuario. El
        /// front convierte a la hora local del navegador al mostrar.
        /// </summary>
        public virtual DateTime GetUtcNow() => DateTime.UtcNow;
    }
}

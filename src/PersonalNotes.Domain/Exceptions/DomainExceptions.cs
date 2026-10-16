namespace PersonalNotes.Domain.Exceptions
{
    public class DomainException(string message) : Exception(message);
    public class NotFoundException(string message) : DomainException(message);
    public class ConflictException(string message) : DomainException(message);

    /// <summary>La operación es válida en forma, pero rompe una regla del árbol (profundidad, ciclos…).</summary>
    public class BusinessRuleException(string message) : DomainException(message);

    /// <summary>El asistente de IA no tiene API Key configurada en el servidor.</summary>
    public class AssistantNotConfiguredException(string message) : Exception(message);

    /// <summary>El proveedor de IA rechazó la consulta por límite de uso.</summary>
    public class AssistantQuotaExceededException(string message) : Exception(message);

    /// <summary>El proveedor de IA falló, rechazó la clave o no respondió a tiempo.</summary>
    public class AssistantUnavailableException(string message) : Exception(message);
}

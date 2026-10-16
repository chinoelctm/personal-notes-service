using FluentValidation;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Helpers
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> MustBeObjectId<T>(this IRuleBuilder<T, string> rule, string field)
        {
            return rule
                .Must(ObjectIds.IsValid)
                .WithMessage($"{field} no es válido (se espera un ObjectId de 24 caracteres hexadecimales).");
        }

        /// <summary>Vacío/null se acepta (por ejemplo, una carpeta raíz no tiene carpeta padre).</summary>
        public static IRuleBuilderOptions<T, string?> MustBeOptionalObjectId<T>(this IRuleBuilder<T, string?> rule,
            string field)
        {
            return rule
                .Must(id => string.IsNullOrWhiteSpace(id) || ObjectIds.IsValid(id.Trim()))
                .WithMessage($"{field} no es válido (se espera un ObjectId de 24 caracteres hexadecimales).");
        }

        public static IRuleBuilderOptions<T, Priority> MustBeValidPriority<T>(this IRuleBuilder<T, Priority> rule)
        {
            return rule
                .IsInEnum()
                .WithMessage("La prioridad debe ser None, Low, Medium o High.");
        }
    }
}

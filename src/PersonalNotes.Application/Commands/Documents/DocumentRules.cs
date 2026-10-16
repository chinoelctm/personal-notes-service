using FluentValidation;
using PersonalNotes.Domain.Entities;

namespace PersonalNotes.Application.Commands.Documents
{
    public static class DocumentRules
    {
        /// <summary>El título puede venir vacío: se publica como "Sin título".</summary>
        public static IRuleBuilderOptions<T, string?> MustBeValidTitle<T>(this IRuleBuilder<T, string?> rule)
        {
            return rule
                .Must(title => title is null || title.Trim().Length <= Document.MaxTitleLength)
                .WithMessage($"El título no puede superar los {Document.MaxTitleLength} caracteres.");
        }

        public static IRuleBuilderOptions<T, string?> MustBeValidContent<T>(this IRuleBuilder<T, string?> rule)
        {
            return rule
                .Must(content => content is null || content.Length <= Document.MaxContentLength)
                .WithMessage("El contenido es demasiado grande; reduce el tamaño de las imágenes.");
        }
    }
}

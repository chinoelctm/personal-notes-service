using FluentValidation;
using PersonalNotes.Application.Services.Assistant;

namespace PersonalNotes.Application.Commands.Assistant.Ask
{
    public class AskAssistantCommandValidator : AbstractValidator<AskAssistantCommand>
    {
        public AskAssistantCommandValidator()
        {
            RuleFor(x => x.Question)
                .NotEmpty().WithMessage("La pregunta es obligatoria.")
                .MaximumLength(AssistantPromptBuilder.MaxQuestionLength)
                .WithMessage($"La pregunta no puede superar los {AssistantPromptBuilder.MaxQuestionLength} caracteres.");

            RuleForEach(x => x.History).ChildRules(message =>
                message.RuleFor(m => m.Role).IsInEnum().WithMessage("El rol del mensaje debe ser User o Assistant."));
        }
    }
}

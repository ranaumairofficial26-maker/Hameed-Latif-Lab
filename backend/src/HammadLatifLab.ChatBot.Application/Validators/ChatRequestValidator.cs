using FluentValidation;
using HammadLatifLab.ChatBot.Application.DTOs;

namespace HammadLatifLab.ChatBot.Application.Validators;

public class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters.");

        RuleFor(x => x.SessionId)
            .MaximumLength(100).WithMessage("SessionId cannot exceed 100 characters.");
    }
}

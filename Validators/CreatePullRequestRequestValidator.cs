using FluentValidation;
using GithubCloudConnector.DTOs;

namespace GithubCloudConnector.Validators;

public class CreatePullRequestRequestValidator : AbstractValidator<CreatePullRequestRequest>
{
    public CreatePullRequestRequestValidator()
    {
        RuleFor(x => x.Owner)
            .NotEmpty().WithMessage("Owner is required.")
            .MaximumLength(100).WithMessage("Owner must not exceed 100 characters.");

        RuleFor(x => x.Repo)
            .NotEmpty().WithMessage("Repo is required.")
            .MaximumLength(100).WithMessage("Repo must not exceed 100 characters.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(256).WithMessage("Title must not exceed 256 characters.");

        RuleFor(x => x.Head)
            .NotEmpty().WithMessage("Head branch is required.");

        RuleFor(x => x.Base)
            .NotEmpty().WithMessage("Base branch is required.");
    }
}

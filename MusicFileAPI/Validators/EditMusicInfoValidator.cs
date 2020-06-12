using FluentValidation;
using FluentValidation.Results;
using MusicFileAPI.Model;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicFileAPI.Validators
{
    public class EditMusicInfoValidator : AbstractValidator<MusicStream>
    {
        public EditMusicInfoValidator()
        {
            RuleFor(r => r.artist)
                .NotEmpty()
                .WithErrorCode("ArtistMissing")
                .WithMessage("{PropertyName} is missing.");

            RuleFor(r => r.title)
                .NotEmpty()
                .WithErrorCode("TitleMissing")
                .WithMessage("{PropertyName} is missing.");

            RuleFor(r => r.uri)
                .NotEmpty()
                .WithErrorCode("MusicFileMissing")
                .WithMessage("{PropertyName} is missing.");
        }

        public override ValidationResult Validate(ValidationContext<MusicStream> context)
        {
            if (context.InstanceToValidate == null)
            {
                var f = new ValidationFailure("Model", "Request object is mandatory");
                return new ValidationResult(new[] { f });
            }

            return base.Validate(context);
        }
    }
}

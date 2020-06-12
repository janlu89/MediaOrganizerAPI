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
    public class DeleteFileValidator : AbstractValidator<string>
    {
        public override ValidationResult Validate(ValidationContext<string> context)
        {
            if (context.InstanceToValidate == null)
            {
                var f = new ValidationFailure("Model", "Request filename is mandatory");
                return new ValidationResult(new[] { f });
            }

            return base.Validate(context);
        }
    }
}

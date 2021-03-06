﻿using FluentValidation;
using FluentValidation.Results;
using MusicFileAPI.Model;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicFileAPI.Validators
{
    public class UploadFileValidator : AbstractValidator<UploadMusicFileRequest>
    {
        public string allowedExtentions = ".mp3, .mp4, .m4a, .m4p, .m4b, .m4r, .m4v, .3gp, .wav, .wave, .aac, .ogg, .ogv, .oga, .ogx, .ogm, .spx, .opus, .webm, .flac";
        public UploadFileValidator()
        {
            RuleFor(r => r.artist)
                .NotEmpty()
                .WithErrorCode("ArtistMissing")
                .WithMessage("{PropertyName} is missing.");

            RuleFor(r => r.title)
                .NotEmpty()
                .WithErrorCode("TitleMissing")
                .WithMessage("{PropertyName} is missing.");

            RuleFor(r => r.musicFile)
                .Must(x => x != null)
                .WithErrorCode("MusicFileMissing")
                .WithMessage("{PropertyName} is missing.")
                .Must(x => x!= null && this.allowedExtentions.IndexOf(Path.GetExtension(x.FileName)) != -1)
                .WithErrorCode("BadFileExtension")
                .WithMessage("{PropertyName} has wrong file exention.");
        }

        public override ValidationResult Validate(ValidationContext<UploadMusicFileRequest> context)
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

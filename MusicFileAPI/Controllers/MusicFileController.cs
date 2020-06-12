using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicFileAPI.Extention;
using MusicFileAPI.Interfaces;
using MusicFileAPI.Model;

namespace MusicFileAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/MusicFile")]
    public class MusicFileController : Controller
    {
        private readonly ILogger<MusicFileController> _logger;
        private readonly ICloudStorage _cloudStorage;
        private readonly IValidator<UploadMusicFileRequest> _uploadMusicFileValidator;
        private readonly IValidator<MusicStream> _editMusicInfoValidator;
        private readonly IValidator<string> _deleteMusicFileValidator;

        public MusicFileController(
            ILogger<MusicFileController> logger, 
            ICloudStorage cloudStorage, 
            IValidator<UploadMusicFileRequest> uploadMusicFileValidator, 
            IValidator<MusicStream> editMusicInfoValidator,
            IValidator<string> deleteMusicFileValidator)
        {
            _logger = logger;
            _cloudStorage = cloudStorage;
            _uploadMusicFileValidator = uploadMusicFileValidator;
            _editMusicInfoValidator = editMusicInfoValidator;
            _deleteMusicFileValidator = deleteMusicFileValidator;
        }
        
        [HttpGet()]
        [ProducesResponseType(typeof(List<MusicStream>),200)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 400)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 500)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var files = await _cloudStorage.GetAll();
                return Ok(files);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to get all the music");
                return this.ErrorResult(HttpStatusCode.InternalServerError, "InternalServerError", "An error occured while processing this request.");
            }
        }

        [HttpPost()]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 400)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 500)]
        public async Task<IActionResult> Upload([FromForm] UploadMusicFileRequest request)
        {
            try
            {
                var validation = _uploadMusicFileValidator.Validate(request);

                if (!validation.IsValid)
                    return this.ErrorResult(HttpStatusCode.BadRequest, validation.Errors);

                await _cloudStorage.UploadAsync(request);
                return Ok("File saved successfully");
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to upload the music");
                return this.ErrorResult(HttpStatusCode.InternalServerError, "InternalServerError", "An error occured while processing this request.");
            }
        }

        [HttpPut()]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 400)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 500)]
        public async Task<IActionResult> Edit([FromForm] MusicStream request)
        {
            try
            {
                var validation = _editMusicInfoValidator.Validate(request);

                if (!validation.IsValid)
                    return this.ErrorResult(HttpStatusCode.BadRequest, validation.Errors);

                await _cloudStorage.EditMusicInfo(request);
                return Ok("Data updated successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update the data");
                return this.ErrorResult(HttpStatusCode.InternalServerError, "InternalServerError", "An error occured while processing this request.");
            }
        }

        [HttpDelete()]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 400)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 500)]
        public async Task<IActionResult> Delete([FromForm]string fileName)
        {
            try
            {
                var validation = _deleteMusicFileValidator.Validate(fileName);

                if (!validation.IsValid)
                    return this.ErrorResult(HttpStatusCode.BadRequest, validation.Errors);

                await _cloudStorage.DeleteFile(fileName);
                return Ok("File deleted successfully");
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to delete the music file");
                return this.ErrorResult(HttpStatusCode.InternalServerError, "InternalServerError", "An error occured while processing this request.");
            }
        }

        [HttpDelete("all")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 400)]
        [ProducesResponseType(typeof(List<ApiErrorResponse>), 500)]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                await _cloudStorage.DeleteAll();
                return Ok("Files deleted successfully");
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to delete all the music files");
                return this.ErrorResult(HttpStatusCode.InternalServerError, "InternalServerError", "An error occured while processing this request.");
            }
        }
    }
}

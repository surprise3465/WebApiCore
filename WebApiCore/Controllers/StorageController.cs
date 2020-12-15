using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnnxObjectDetection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApiCore.Entities;
using WebApiCore.Helpers;
using WebApiCore.Models;
using WebApiCore.Services;

namespace WebApiCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IFileService _storageService;
        private readonly IObjectDetectionService _objectDetectionService;
        private readonly ILogger<StorageController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public StorageController(IFileService storageService, IWebHostEnvironment hostingEnvironment,
            ILogger<StorageController> logger, IObjectDetectionService objectDetectionService)
        {
            _logger = logger;
            _storageService = storageService;
            _objectDetectionService = objectDetectionService;
            _hostingEnvironment = hostingEnvironment;
        }

        //Example from https://dottutorials.net/dotnet-core-web-api-multipart-form-data-upload-file/
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile([FromForm] UploadImageCommand imageCommand)
        {
            var file = imageCommand.File;
            if (file.Length > 0)
            {
                var details = new AppFile
                {
                    Name = file.FileName,
                    AddedDate = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ContentType = file.ContentType,
                    Description = imageCommand.Description,
                    Tags = imageCommand.Tags
                };

                MemoryStream imageMemoryStream = new MemoryStream();
                file.CopyTo(imageMemoryStream);

                Image image = Image.FromStream(imageMemoryStream);

                //Convert to Bitmap
                Bitmap bitmapImage = (Bitmap)image;

                _logger.LogInformation($"Start processing image...");
                var watch = System.Diagnostics.Stopwatch.StartNew();
                ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };
                var imgbyte = DetectAndPaintImage(imageInputData, image);
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");
                imageMemoryStream.Close();

                Stream mem = new MemoryStream(imgbyte);
                details.Size = mem.Length;
                AppFile res = await _storageService.UploadFileAsync(mem, details);
                return Ok(res);                
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownLoadFile(string id)
        {
            var (content, details) = await _storageService.DownloadFileAsync(id);
            this.Response.ContentLength = details.Size;
            this.Response.Headers.Add("Accept-Ranges", "bytes");
            this.Response.Headers.Add("Content-Range", "bytes 0-" + details.Size);
            return File(content, details.ContentType, details.Name);
        }

        [HttpGet("{id}/view")]
        public async Task<FileStreamResult> DownloadView(string id)
        {
            var (stream, details) = await _storageService.DownloadFileAsync(id);
            this.Response.ContentLength = details.Size;
            this.Response.Headers.Add("Accept-Ranges", "bytes");
            this.Response.Headers.Add("Content-Range", "bytes 0-" + details.Size);
            return new FileStreamResult(stream, details.ContentType);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppFile()
        {
            return Ok(await _storageService.GetAllAppFileAsync());
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetAppFile(string id)
        {
            return Ok(await _storageService.GetAppFileAsync(id));
        }

        [HttpPut("details/{id}")]
        public async Task<IActionResult> UpdateAppFile(AppFile details, string id)
        {
            details.Id = id;
            return Ok(await _storageService.UpdateAppFileAsync(details));
        }

        [HttpGet("details/tags/{tag}")]
        public async Task<IActionResult> GetAppFileByTag(string tag)
        {
            return Ok(await _storageService.GetAppFileByTagAsync(tag));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFileAsync(string id)
        {
            string deletedId = await _storageService.DeleteFileAsync(id);
            return base.Ok($"Deleted {deletedId} successfully");
        }

        private byte[] DetectAndPaintImage(ImageInputData imageInputData, Image imageFilePath)
        {
            //Predict the objects in the image
            _objectDetectionService.DetectObjectsUsingModel(imageInputData);
            var img = _objectDetectionService.DrawBoundingBox(imageFilePath);

            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, img.RawFormat);
                byte[] imageBytes = m.ToArray();
                return imageBytes;
            }
        }
    }
}

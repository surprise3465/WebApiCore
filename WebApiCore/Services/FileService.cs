using System;
using System.Collections.Generic;
using System.Linq;
using WebApiCore.Entities;
using WebApiCore.DbContexts;
using WebApiCore.Helpers;
using System.Threading.Tasks;
using System.IO;
using WebApiCore.Entities;
using WebApiCore.DbContexts;
using WebApiCore.Helpers;
using LiteDB;
using Microsoft.AspNetCore.Hosting;

namespace WebApiCore.Services
{
    public interface IFileService : IDisposable
    {
        Task<AppFile> UploadFileAsync(Stream fileStream, AppFile AppFile);
        Task<(Stream, AppFile)> DownloadFileAsync(string id);
        Task<AppFile> UpdateAppFileAsync(AppFile details);
        Task<AppFile> GetAppFileAsync(string id);
        Task<IEnumerable<AppFile>> GetAllAppFileAsync();
        Task<IEnumerable<AppFile>> GetAppFileByTagAsync(string tag);
        Task<string> DeleteFileAsync(string id);
    }

    public sealed class FilesService : IFileService
    {
        private readonly ILiteDatabase _liteDatabase;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public FilesService(ISettingsService settingsService, IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            string path = _hostingEnvironment.ContentRootPath + settingsService.GetLiteDBAppSettings();
            Console.WriteLine(path);
            _liteDatabase = new LiteDatabase(path);
        }

        public void Dispose()
        {
            _liteDatabase.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<string> DeleteFileAsync(string id)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<AppFile>("AppFile");
                var success = collection.Delete(id);
                return (success ? id : string.Empty);
            });
        }

        public async Task<(Stream, AppFile)> DownloadFileAsync(string id)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<AppFile>("AppFile");
                var AppFile = collection.FindById(id);
                var stream = _liteDatabase.FileStorage.OpenRead(id);
                return (stream, AppFile);
            });
        }

        public async Task<IEnumerable<AppFile>> GetAllAppFileAsync()
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<AppFile>("AppFile");
                return collection.Query().ToList();
            });
        }

        public async Task<AppFile> GetAppFileAsync(string id)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<AppFile>("AppFile");
                return collection.FindById(id);
            });
        }

        public async Task<IEnumerable<AppFile>> GetAppFileByTagAsync(string tag)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<AppFile>("AppFile");
                return collection.Query().Where(a => a.Tags.Contains(tag.Trim())).ToList();
            });
        }

        public async Task<AppFile> UpdateAppFileAsync(AppFile details)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<AppFile>("AppFile");
                var AppFile = collection.FindById(details.Id);
                AppFile.Description = details.Description;
                AppFile.LastModified = DateTime.UtcNow;
                AppFile.Name = details.Name;
                AppFile.Tags = details.Tags;
                AppFile.NumberOfDownloads = details.NumberOfDownloads;
                var success = collection.Update(AppFile);
                return (success ? AppFile : throw new Exception("Error while updating"));
            });
        }

        public async Task<AppFile> UploadFileAsync(Stream fileStream, AppFile AppFile)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<AppFile>("AppFile");
                AppFile.Id = ObjectId.NewObjectId().ToString();
                collection.Insert(AppFile.Id, AppFile);
                var obj = _liteDatabase.FileStorage.Upload(AppFile.Id, AppFile.Name, fileStream);
                return AppFile;
            });
        }
    }
}
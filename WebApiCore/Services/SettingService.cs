using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiCore.Helpers;
using WebApiCore.Models;

namespace WebApiCore.Services
{
    public interface ISettingsService
    {
        public string GetSecret();
        public string GetLiteDBAppSettings();
    }

    public class SettingsService : ISettingsService
    {
        private readonly AppSettings appSettings;
        public SettingsService(IOptions<AppSettings> AppSettings)
        {
            appSettings = AppSettings.Value;
        }

        public string GetSecret()
        {
            return appSettings.Secret;
        }

        public string GetLiteDBAppSettings()
        {
            return appSettings.LiteDB;
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiCore.Services;
using WebApiCore.Entities;

namespace WebApiCore.Middlewares
{
	// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
	public class DownloadsCountMiddleware
	{
		private readonly RequestDelegate next;

		public DownloadsCountMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context, IFileService storageService /* other dependencies */)
		{
			//TODO: Identify unique views
			try
			{
				var header = context.Request.Headers["DownloadsCount"];
				if (header.ToString() == "AppFile")
				{
					var reqlist = context.Request.Path.Value.Split("/")[3];
					var details = await storageService.GetAppFileAsync(reqlist);
					if (details.NumberOfDownloads.HasValue)
					{
						details.NumberOfDownloads++;
					}
					else
					{
						details.NumberOfDownloads = 1;
					}
					await storageService.UpdateAppFileAsync(details);
				}
			}
			finally
			{
				await next(context);
			}
		}
	}

	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseDownloadsCount(
			this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<DownloadsCountMiddleware>();
		}		
	}
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiCore.Models
{
	public class UploadImageCommand
	{
		public IFormFile File { get; set; }
		public string Description { get; set; }
		public IEnumerable<string> Tags { get; set; }
	}
}

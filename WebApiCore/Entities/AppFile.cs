using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApiCore.Entities
{
    public class AppFile
    {
        public string Id { get; set; }

        [Display(Name = "File Name")]
        public string Name { get; set; }

        [Display(Name = "Size (bytes)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public long Size { get; set; }

        [Display(Name = "Uploaded (UTC)")]
        [DisplayFormat(DataFormatString = "{0:G}")]
        public DateTime AddedDate { get; set; }

        public string AddedBy { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public IEnumerable<string> Tags { get; set; }

        [Display(Name = "Last Modified (UTC)")]
        [DisplayFormat(DataFormatString = "{0:G}")]
        public DateTime LastModified { get; set; }
        public string ContentType { get; set; }
        public int? NumberOfDownloads { get; set; }
    }
}

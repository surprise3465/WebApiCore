using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiCore.Models
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string LiteDB { get; set; }
    }
}

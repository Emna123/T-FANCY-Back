using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class UploadFrontHomeFiles
    {
        public string animated_text { get; set; }
        public string historical_text { get; set; }
        public IFormFile hommeCollection_image { get; set; }
        public IFormFile femmeCollection_image { get; set; }
    }
}

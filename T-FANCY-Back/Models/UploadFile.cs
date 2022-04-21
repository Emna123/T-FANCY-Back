using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class UploadFile
    {
        public IFormFile file { get; set; }
        public String name { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class UploadFiles
    {
        public List<IFormFile> files { get; set; }
        public int idProduct { get; set; }

    }
}

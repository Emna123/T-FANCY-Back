using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class Prod_image
    {
        public int id { get; set; }
        public string name { get; set; }
        public virtual Product product { get; set; }
    }
}

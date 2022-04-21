using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class Brand
    {
        public int id { get; set; }
        public string brand_name { get; set; }
        public string brand_image{ get; set; }
        public virtual ICollection<Product> products { get; set; }


    }
}

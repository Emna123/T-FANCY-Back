using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class Product
    {
        public int id { get; set; }
        public string productName { get; set; }
       public string category { get; set; }
       public string description { get; set; }
       public string gender { get; set; }
       public string type { get; set; }
       public string [] size { get; set; }
       public int quantity { get; set; }
       public int qnt_sold { get; set; }
       public double price { get; set; }
       public double old_price { get; set; }
       public Boolean best_prod { get; set; }
       public Boolean new_prod { get; set; }
       public Boolean Bestsell { get; set; }
       public  Nullable<DateTime> exp_date { get; set; }
       public virtual ICollection<Prod_image> prod_image { get; set; }
       public virtual Brand brand { get; set; }
       public int brandid { get; set; }





    }
}

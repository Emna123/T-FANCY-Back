using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class Client:User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string Birth { get; set; }
        public Boolean Inscri_news  { get; set; }



    }
}

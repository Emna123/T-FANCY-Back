using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class ResetPassword
    {
        public string email { get; set; }
        public string token { get; set; }
        public string newpassword { get; set; }

    }
}

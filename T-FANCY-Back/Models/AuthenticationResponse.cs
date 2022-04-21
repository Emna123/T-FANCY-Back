using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class AuthenticationResponse
    {
        public string token { get; set; }
        public string refreshtoken { get; set; }
        public bool IsSuccess { get; set; }
        public string Reason { get; set; }

    }
}

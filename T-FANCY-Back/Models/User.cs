using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{
    public class User : IdentityUser
    {
        override
        public string Email { get; set; }

        public string Password { get; set; }

        public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; }


}
}

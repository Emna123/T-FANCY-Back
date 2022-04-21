using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace T_FANCY_Back.Models
{

    [Table("UserRefreshToken")]
    public class UserRefreshToken
    {
        [Key]
        public int UserRefreshTokenId { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        [NotMapped]
        public bool ISActive
        {
            get
            {
                return ExpirationDate > DateTime.UtcNow;
            }
        }
            public string IpAddress { get; set; }
        public bool IsInvalidated { get; set; }
        public virtual User user { get; set; }

    }
}

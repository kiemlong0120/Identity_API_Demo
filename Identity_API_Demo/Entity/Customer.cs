using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity_API_Demo.Entity
{
    public class Customer : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(50)")]
        public string Name { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(50)")]
        public string Address { get; set; }

        public string RoleName { get; set; }
    }
}

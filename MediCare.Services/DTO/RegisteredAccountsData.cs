using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTO
{
    public class RegisteredAccountsData
    {
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string Role { get; set; } = null!;
        public DateTime? Created { get; set; }
    }
}

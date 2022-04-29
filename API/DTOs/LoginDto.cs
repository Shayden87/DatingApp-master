using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    // Summary:
    // Creates data transfer object for user data for allowing user logins. 
    public class LoginDto
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
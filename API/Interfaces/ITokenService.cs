using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;

namespace API.Interfaces
{
    // Summary: 
    // Interface that provides functionality for creating Json web tokens.
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
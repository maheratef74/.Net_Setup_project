using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class ApplicationUser :  IdentityUser
{
    public string  Name { get; set; }
}
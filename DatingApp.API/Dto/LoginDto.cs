using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dto
{
    public class LoginDto
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.WebAPI.CrossClient
{
    public interface ITokenUser
    {
        int ID { get; set; }
        string Username { get; set; }
        string Email { get; set; }
        DateTime CreatedOn { get; set; }
        string Token { get; set; }

        List<string> AuthenticationStatus { get; set; }        
    }
}

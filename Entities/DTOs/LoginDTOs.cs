using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class LoginRequest
    {
        public string Usuario { get; set; }
        public string Contraseña { get; set; }
    }

    public class LoginResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public DateTime expires_in { get; set; }
    }

    public class RefreshRequest
    {
        public string refresh_token { get; set; }
    }

    public class ValidateTokenRequest
    {
        public string token { get; set; }
    }
}

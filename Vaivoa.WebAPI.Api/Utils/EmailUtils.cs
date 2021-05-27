using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vaivoa.WebAPI.Api.Utils
{
    public static class EmailUtils
    {
        //se um dia preciar mudar regra de email valido, muda só uma vez aqui
        public static bool EhEmailValido(string email)
        {
            return (new EmailAddressAttribute().IsValid(email));
        }
    }
}

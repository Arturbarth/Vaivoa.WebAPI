using System;
using System.IO;
using CreditCardValidator;
using Microsoft.AspNetCore.Http;

namespace Vaivoa.CartoesController.Modelos
{
    public static class CartoesExtension
    {
     
        public static Cartao GerarCartao(this CartaoEmail model)
        {
            Random rnd = new Random();
            return new Cartao
            {
                Email = model.Email,
                Titular = model.Email.Split('@')[0],
                Numero = CreditCardFactory.RandomCardNumber(CardIssuer.MasterCard),
                CodSeguranca = rnd.Next(1,999).ToString().PadLeft(3, '0'),
                MesValido = rnd.Next(1, 13).ToString().PadLeft(2, '0'),
                AnoValido = (DateTime.Now.Year+5).ToString()
            };
        }

        public static CartaoApi ToApi(this Cartao cartao)
        {
            return new CartaoApi
            {
                Id = cartao.Id,
                Email = cartao.Email,
                Titular = cartao.Titular,
                Numero = cartao.Numero,
                CodSeguranca = cartao.CodSeguranca,
                MesValido = cartao.MesValido,
                AnoValido = cartao.AnoValido
            };
        }

    }
}

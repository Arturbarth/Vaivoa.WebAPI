using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vaivoa.CartoesController.Modelos
{
    public class Cartao
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        public string Titular { get; set; }
        public string Numero { get; set; }
        public string CodSeguranca { get; set; }
        public string MesValido { get; set; }
        public string AnoValido { get; set; }

    }
    
    public class CartaoApi: Cartao
    {
        //herda de Cartão, havia implementado métodos especificas mas removi
    }

    public class CartaoEmail
    {
        [Required]
        public string Email { get; set; }
    }
}

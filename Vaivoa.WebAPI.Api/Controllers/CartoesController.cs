using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vaivoa.CartoesController.Modelos;
using Vaivoa.CartoesController.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.ComponentModel.DataAnnotations;
using Vaivoa.WebAPI.Api.Utils;

namespace Vaivoa.CartoesController.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartoesController : ControllerBase
    {
        private readonly IRepository<Cartao> _repo;

        public CartoesController(IRepository<Cartao> repository)
        {
            _repo = repository;
        }

        [HttpGet]
        [ProducesResponseType(statusCode: 200, Type = typeof(List<CartaoApi>))]
        public IActionResult ListaDeCartoes()
        {
            var lista = _repo.All.Select(l => l.ToApi()).ToList();
            return Ok(lista);
        }

        [HttpGet("GetById/{id}")]
        [ProducesResponseType(statusCode: 200, Type = typeof(CartaoApi))]
        public IActionResult Buscar(int id)
        {
            if (id <= 0) return BadRequest("Infome um ID!");

            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model.ToApi());
        }

        [HttpGet("{email}")]
        [ProducesResponseType(statusCode: 200, Type = typeof(List<CartaoApi>))]
        public IActionResult BuscarPorEmail(string email)
        {
            if (!EmailUtils.EhEmailValido(email)) return BadRequest("Infome um Email válido!");

            var model = _repo.All.Where(x => x.Email.Equals(email)).Select(l => l.ToApi()).ToList();
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        [HttpPost]
        public IActionResult Incluir([FromBody] CartaoEmail model)
        {
            if (ModelState.IsValid)
            {
                if (!EmailUtils.EhEmailValido(model.Email)) return BadRequest("Infome um Email válido!");

                var cartao = model.GerarCartao();
                _repo.Incluir(cartao);
                var uri = Url.Action("Buscar", new { id = cartao.Id });
                return Created(uri, cartao); 
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public IActionResult Excluir(int id)
        {
            if (id <= 0) BadRequest("Infome um ID!");

            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            _repo.Excluir(model);
            return NoContent(); 
        }
    }
}
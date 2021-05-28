Primeiro, vamos pensar no problema:
Precisamos que a API receba um email e gere um cartão, neste caso, não vamos complicar e vamos manter o escopo bem definido.
A estrutura idela de banco de dados é Pessoa (Email) » Cartões da Pessoa. Mas se abrangermos uma estrutura de Microsservisos, teriamos uma API especifica para pessoas, correto?
Neste caso, manteremos apenas uma entidade, a entidade Cartões com as seguintes propriedades:

public int Id;
public string Email;
public string Titular; (Em um cenário onde vamos ter Pessoa » Cartões haverá uma propriedade public Pessoa Titular; ) 
public string Numero;
public string CodSeguranca;
public string MesValido;
public string AnoValido;

Como o escopo solicitado é gerar apenas o número do cartão, sem levar em consideração demais pontos que possam estar envolvidos como limite, 
cartão adicional, se é internacional ou não, tipo Crédito, Débito ou ambos, Beneficíos Bandeira (Gold, Platinum etc), anuidade (eca) entre outros, 
resolvi manter uma entidade básica. 

Pensando em escalabilidade, precisamos tratar cada entidade como única e independente no nosso projeto, 
imagine que semana que vem teremos que criar uma API de investimentos.

Pensando em microsserviços, criaremos as classlibs independentes e com objetivos únicos, dessa forma, podemos reutilizar o código em quaisquer outras APIs. 
Ainda pensando em reutilizar código, porque não criar uma única API de autenticação? Dessa forma, podemos centralizar a autenticação de várias APIs em um único serviço, onde teremos o trabalho de escrever o código uma única vez.
Com isso em mente, criaremos uma DAL adicional de usuários e um projeto "Segurança" responsável por centralizar as credenciais.

Obs: Não vou detalhar a utilização e instalação dos pacotes NuGet exceto o os pacotes JWT e CreditCardValidator.

Com o seguinte comando criamos a Solution:
```C#
dotnet new sln -n Vaivoa.WEBAPI
```

Agora vamos criar as bibliotecas de classes
```C#
dotnet new classlib -o Vaivoa.WebAPI.DAL.Cartoes
```
```C#
dotnet new classlib -o Vaivoa.WebAPI.DAL.Usuarios
```
```C#
dotnet new classlib -o Vaivoa.WebAPI.Model
```
```C#
dotnet new classlib -o Vaivoa.WebAPI.Seguranca
```
Já que decidimos criar duas APIs, uma para criar e retornar os cartões e outra para a autenticação, precisamos criar os projetos:
```C#
dotnet new webapi -o Vaivoa.WebAPI.Api
```
```C#
dotnet new webapi -o Vaivoa.WebAPI.AuthProvider
```
Finalmente, vamos vincular todos os projetos criados à Solution.
```C#
dotnet sln Vaivoa.WEBAPI.sln add Vaivoa.WebAPI.DAL.Cartoes\Vaivoa.WebAPI.DAL.Cartoes.csproj
```
```C#
dotnet sln Vaivoa.WEBAPI.sln add Vaivoa.WebAPI.DAL.Usuarios\Vaivoa.WebAPI.DAL.Usuarios.csproj
```
```C#
dotnet sln Vaivoa.WEBAPI.sln add Vaivoa.WebAPI.Model\Vaivoa.WebAPI.Model.csproj
```
```C#
dotnet sln Vaivoa.WEBAPI.sln add Vaivoa.WebAPI.Seguranca\Vaivoa.WebAPI.Seguranca.csproj
```
```C#
dotnet sln Vaivoa.WEBAPI.sln add Vaivoa.WebAPI.Api\Vaivoa.WebAPI.Api.csproj
```
```C#
dotnet sln Vaivoa.WEBAPI.sln add Vaivoa.WebAPI.AuthProvider\Vaivoa.WebAPI.AuthProvider.csproj
```


Obs: Eu não executei exatamente nessa ordem, mas ao executar os comandos acioma o resultado será o mesmo.

Agora vamos começar a codificação de baixo para cima, primeiro precisamos criar as entidades que pensamos anteriormente. 
No projeto Vaivoa.WebAPI.Model criamos a entidade Cartoes.cs

```C#
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
```

Mais pra frente vamos indentificar que precisamos criar uma segunda entidade para receber um email. 
Criada a entidade, vamos prepara-lá para funcionar com EF Core adicionando algumas anotations. 
É chegada a hora de criarmos a DAL. 
Primeiro, criamos a entidade CartaoContext e de quebra já vamos utilizar uma boa prática e um Design Pattern muito conhecido chamado RepositoryPattern 
Você pode ler sobre esse Pattern neste artigo: https://medium.com/@martinstm/repository-pattern-net-core-78d0646b6045

```C#
using Vaivoa.CartoesController.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Vaivoa.CartoesController.Persistencia
{
    public class CartaoContext : DbContext
    {
        public DbSet<Cartao> Cartoes { get; set; }

        public CartaoContext(DbContextOptions<CartaoContext> options) 
            : base(options)
        {
            //irá criar o banco e a estrutura de tabelas necessárias
            this.Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration<Cartao>(new CartaoConfiguration());
        }
    }
}
```

```C#
using System.Linq;

namespace Vaivoa.CartoesController.Persistencia
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> All { get; }
        TEntity Find(int key);
        void Incluir(params TEntity[] obj);
        void Alterar(params TEntity[] obj);
        void Excluir(params TEntity[] obj);
    }
}
```

Agora que está tudo preparado, vamos criar o controller de cartões.
Criamos então uma nova pasta chamada Controllers no projeto Vaivoa.WebAPI.Api e nessa nova pasta criada, criamos a CartoesController herdando de ControllerBase.

```C#
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
```


Como pode ser visto acima, nessa etapa é que vimos a necessidade de implementar a geração do cartão. Para isso, utilizei o pacote NuGet CreditCardValidator 
Para manter tudo padronizado, vamos adiciona-lo ao projeto Model, basta navegar pelo terminal até a pasta Vaivoa.WebAPI.Model: 
dotnet add package CreditCardValidator 

Para não adicionar lógica no modelo, criaremos uma Extension da classe cartão, que tratará a geração do cartão.
Note que CartaoApi herda de Cartão mas mesmo assim implementei um método "cast" ToApi para deixar o sistema já preparado para adicionar objetos pais ou filhos à classe cartão. 

```C#
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
```


Agora vamos aos itens importantes no arquivo Startup.cs
Precisamos configurar alguns middlewares como o Swagger, SQL Server, Controllers e Routings.
O código abaixo já está com o MiddleWare JWT, para instalar é fácil:

```C#
dotnet add package Swashbuckle.AspNetCore


```C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vaivoa.CartoesController.Modelos;
using Vaivoa.CartoesController.Persistencia;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Vaivoa.WebAPI.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //por algum motivo que não sei explicar o JWT só funcionou com CORS,
            //ao passar o token por parametro apresentava erro 401 não autorizado mesmo gerando o Token uma requisição anterior
            //acredito que isso se deve pois estou subindo 2 serviços, 1 de tokens e outro que é a API propriamente dita
            services.AddCors();

            services.AddDbContext<CartaoContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Vaivoa"));
            });

            //Serviço com Lifecicle Transitório, necessário para instanciar conexão com a base somente quando necessário por requisição
            services.AddTransient<IRepository<Cartao>, RepositorioBaseEF<Cartao>>();

            services.AddControllers();


            //atenticação JWT
            var key = System.Text.Encoding.UTF8.GetBytes("vaivoa-webapi-authentication-valid");
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            //gera o SwaggerDOC para documentação da API
            /*services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VaiVoaWebApi", Version = "v1" });
            });*/


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Autorização JWT utilizando Schema Bearer. \r\n\r\n Acesse a API de autenticação atravéz deste link: e informe o Bearer abaixo.\r\n\r\n Exemplo: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                   {
                     new OpenApiSecurityScheme
                     {
                       Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                      new string[] { }
                    }
                  });
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //configura para gerar SwaggerDOC para documentação da API
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VaiVoaWebApi v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            //por algum motivo que não sei explicar o JWT só funcionou com CORS,
            //ao passar o token por parametro apresentava erro 401 não autorizado mesmo gerando o Token uma requisição anterior
            //acredito que isso se deve pois estou subindo 2 serviços, 1 de tokens e outro que é a API propriamente dita
            app.UseCors(x => x
             .AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader());

            //Necessário para autorização e autenticação JWT
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
```



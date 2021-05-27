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

            services.AddDbContext<CartaoContext>(options => {
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VaiVoaWebApi", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //configura para gerar SwaggerDOC para documentação da API
            //if (env.IsDevelopment())
            //{
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VaiVoaWebApi v1"));
            //}

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

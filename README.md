# Vaivoa.WebAPI
Escreva um artigo, em formato de blog post sobre um projeto C# com .NET Core. Você deverá descrever o passo-a-passo para a criação de uma API REST que fornece um sistema de geração de número de cartão de crédito virtual.

A API deverá gerar números aleatórios para o pedido de novo cartão. Cada cartão gerado deve estar associado a um email para identificar a pessoa que está utilizando.

Essencialmente são 2 endpoints. Um receberá o email da pessoa e retornará um objeto de resposta com o número do cartão de crédito. E o outro endpoint deverá listar, em ordem de criação, todos os cartões de crédito virtuais de um solicitante (passando seu email como parâmetro).

A implementação deverá ser escrita utilizando C# com .Net Core e Entity Framework Core.

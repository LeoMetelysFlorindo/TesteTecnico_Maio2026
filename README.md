# Atividade Prática

### Informações de configuração para uso da aplicação

IDE´s utilzadas: 
  - Visual Studio 2026
  - Visual Studio Code

Linguagens de Programação
  - Backend (API) - C#.NET

Banco de dados
  - Microsofit SQL Server 22 Comunity

Ferramentas de Apoio
  - SQL Managemente Studio 22 Free

# Testes no Terminal ou PowerShell
# Check installed .NET version
dotnet --version
# Expected: 8.0.x

# Criação do Projeto API
dotnet new webapi -n PedidoApi -o PedidoApi
cd PedidoApi

# Add essential packages
Após a criação do projeto na pasta onde o mesmo foi criado abriu-se o Windows PowerShell como administrador e foram executados estes comandos:

- dotnet add package Microsoft.EntityFrameworkCore.SqlServer
- dotnet add package Microsoft.EntityFrameworkCore.Design
- dotnet add package FluentValidation.AspNetCore
- dotnet add package Swashbuckle.AspNetCore
- dotnet add package Serilog.AspNetCore --version 8.0.0
- dotnet add package Serilog.Sinks.Console --version 6.0.0
- dotnet add package Serilog.Sinks.File --version 6.0.0
- dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.8


# Inserir as tabelas no banco de dados
Os comandos abaixo fizeram a criação das tabelas dentrodo SQL Server local instalado
- dotnet ef migrations add AddCascadeDeletePedidoItem
- dotnet ef database update

# Sobre o projeto

Baixe os fontes em uma pagina apropriada do Git. Valide a verificação dos packages informados acima. Valide o arquivo em PDF dos testes de execução para seguir conforme mostrado nas imagens de testes.

Alguns Testes Unitários foram criados e inseridos. 
Os logs de execução aparecem enquanto o mesmo é executado informando a execução das tarefas selecionadas. 


# JSON DE USO PARA INSERÇÃO:

````JSON
{
  "clienteNome": "Karol Silva",
  "itensPedido": [
    {
      "nomeProduto": "Notebook Lenovo",
      "quantidade": 1,
      "valorUnitario": 2750.00
    }
  ]
}
````



Por
<br/>
Manoel Leonardo Metelis Florindo | Desenvolvedor Full-stack

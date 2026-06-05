# Lumiere — Guia de Arquitetura e Convenções

## Visão Geral

Projeto **Lumiere** desenvolvido em **.NET 9** seguindo os princípios de **Clean Architecture**, com separação rígida de responsabilidades entre camadas.

**Repositório:** `git@github.com:ViniEugenio/Lumiere.git`

---

## Configuração por Ambiente

A API lê as configurações do `appsettings.json` combinado com o arquivo do ambiente ativo, determinado pela variável `ASPNETCORE_ENVIRONMENT`.

| Ambiente | Arquivo carregado |
|---|---|
| `Development` (local) | `appsettings.json` + `appsettings.Development.json` |
| `Production` | `appsettings.json` + `appsettings.Production.json` |

**Regras:**
- `appsettings.json` contém apenas a estrutura das chaves, sem valores sensíveis
- `appsettings.Development.json` e `appsettings.Production.json` contém os valores reais e estão no `.claudeignore` e `.gitignore`
- A connection string deve ser lida via `configuration.GetConnectionString("Default")`
- Em produção, os valores podem ser sobrescritos por variáveis de ambiente (ex: `ConnectionStrings__Default`)

**Configuração no `Program.cs`** — o .NET já faz isso automaticamente:
```csharp
// O builder já carrega appsettings.json + appsettings.{Environment}.json por padrão
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default");
```

**Nos testes**, o `Testcontainers` cria o banco dinamicamente e injeta a connection string diretamente, sem depender do `appsettings`.

---

## Estrutura da Solução

```
Lumiere/
└── src/
    ├── Lumiere.API
    ├── Lumiere.Application
    ├── Lumiere.Domain
    ├── Lumiere.Infra
    └── Lumiere.Tests
```

---

## Lumiere.API

Responsável exclusivamente pela exposição da API. Não contém regras de negócio.

**Regras:**
- Utilizar **Minimal APIs**
- Todas as rotas organizadas em endpoints separados por funcionalidade
- Apenas receber requisições, validar entradas e encaminhar para a camada Application
- Configurações de DI registradas nesta camada
- Configurações de autenticação e autorização ficam nesta camada

**Estrutura:**
```
Lumiere.API/
├── Endpoints/
├── Extensions/
├── Middlewares/
├── Configurations/
└── Program.cs
```

---

## Convenções de Minimal APIs

### EndpointBase

Todos os endpoints devem herdar de `EndpointBase` (localizado em `Lumiere.API/Endpoints/EndpointBase.cs`).

`EndpointBase` provê o método `HandleResult<T>(ResultDto<T> result)` para padronizar as respostas:
- Sucesso → `Results.Ok(result.Data)`
- Falha → `Results.BadRequest(result.Errors)`

```csharp
public abstract class EndpointBase
{
    protected static IResult HandleResult<T>(ResultDto<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Data);
        }

        return Results.BadRequest(result.Errors);
    }
}
```

Como `EndpointBase` é uma classe não-estática, as classes de endpoints também são não-estáticas e herdam dela. O método de mapeamento é estático mas **não é extension method** — o registro no `Program.cs` usa chamada direta: `UserEndpoints.MapUserEndpoints(apiRoutes)`.

### Estrutura dos endpoints

Cada funcionalidade deve ter sua própria classe dentro de `Endpoints/`, herdando de `EndpointBase`:

```csharp
public class UserEndpoints : EndpointBase
{
    public static IEndpointRouteBuilder MapUserEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("users")
            .RequireAuthorization();

        group.MapGet("", GetAllUsers);
        group.MapGet("{id}", GetUserById);
        group.MapPost("", CreateUser);

        return endpoints;
    }
}
```

### Registro no `Program.cs`

Todos os grupos de endpoints são registrados sob o prefixo `api/`:

```csharp
var apiRoutes = app.MapGroup("api/");
UserEndpoints.MapUserEndpoints(apiRoutes);
```

### Handlers (delegates)

Os handlers são métodos estáticos privados dentro da mesma classe do grupo. Parâmetros são resolvidos pelo model binding do ASP.NET Core:

```csharp
private static async Task<IResult> CreateUser(
    [FromServices] ISender sender,
    [FromBody] CreateUserCommand command,
    CancellationToken ct)
{
    var result = await sender.Send(command, ct);
    return HandleResult(result);
}
```

**Origens de parâmetros:**
- `[FromServices]` — serviços injetados via DI
- `[FromRoute]` — parâmetros de rota
- `[FromQuery]` — parâmetros de query string
- `[FromBody]` — corpo da requisição
- `CancellationToken` — fornecido automaticamente pelo pipeline

### Documentação OpenAPI

Usar `[Tags]` para agrupar endpoints no Swagger e `.Produces()` para documentar os possíveis retornos:

```csharp
group.MapPost("", CreateUser)
    .WithTags("Users")
    .Produces(StatusCodes.Status200OK, typeof(UserDto))
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status403Forbidden);
```

**Regras:**
- `.Produces()` é apenas documentação — não valida nem força o retorno em tempo de execução
- Endpoints com `RequireAuthorization()` devem documentar `401` e `403`
- A tag deve corresponder ao nome do recurso (ex: `"Users"`, `"Products"`)

---

## Lumiere.Domain

Responsável pelas regras centrais do domínio.

**Regras:**
- **Não depende de nenhuma outra camada**
- Contém apenas conceitos de domínio
- Todas as entidades ficam nesta camada
- Todas as interfaces dos repositórios ficam nesta camada
- Value Objects, Enums e Exceptions de domínio ficam nesta camada

**Estrutura:**
```
Lumiere.Domain/
├── Entities/
├── Interfaces/
├── ValueObjects/
├── Enums/
└── Exceptions/
```

---

## Lumiere.Application

Responsável pelos casos de uso da aplicação.

**Regras:**
- Implementar o padrão **CQRS**
- Commands e Queries separados
- Utilizar **MediatR** para processamento dos requests
- Toda regra de aplicação fica nesta camada
- **CommandHandlers não devem acessar repositórios diretamente** — o acesso a repositórios deve ocorrer exclusivamente pelos Application Services
- **Services são responsáveis pela orquestração** das regras de negócio e conversão de resultados
- **Repositórios são responsáveis apenas pela persistência** — não convertem erros em DTOs
- Commands e Queries devem retornar `ResultDto<T>` quando precisam comunicar status de sucesso ou falha
- DTOs usados para comunicação entre Application e API ficam em `Lumiere.Application/DTOs` — Features não devem criar subpastas `DTOs/` próprias sem justificativa forte
- **Services devem receber Commands, Queries ou DTOs como parâmetro** — nunca uma lista de primitivos; ver regra de parâmetros abaixo
- **Depende apenas do Domain**

**Estrutura:**
```
Lumiere.Application/
├── DTOs/
├── Features/
│   └── Users/
│       ├── Commands/
│       ├── Queries/
│       └── Handlers/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Behaviors/
├── Validators/
└── DependencyInjection/
```

---

## Lumiere.Infra

Responsável pela persistência e integrações externas.

**Regras:**
- Utilizar **Entity Framework Core**
- Conter DbContext
- Conter Migrations
- Mapeamentos das entidades via `IEntityTypeConfiguration`
- Implementar todos os repositórios definidos no Domain
- Responsável por integrações com serviços externos

**Estrutura:**
```
Lumiere.Infra/
├── Context/
├── Mappings/
├── Repositories/
├── Migrations/
└── DependencyInjection/
```

---

## Lumiere.Tests

Responsável pelos testes unitários e de integração.

**Regras:**
- Utilizar **xUnit.NET** para todos os testes
- Para cada nova funcionalidade, criar testes que cubram todos os casos de uso
- Na pasta `Setup`, utilizar **Testcontainers** para criação do banco de dados de teste
- O arquivo de seed em `Setup` deve popular com dados fake todas as entidades do Domain

**Estrutura:**
```
Lumiere.Tests/
├── Setup/
├── IntegrationTests/
└── UnitTests/
```

---

## Padrão de Repositórios

Cada entidade possui seu próprio repositório. Todos herdam de um repositório base.

**IBaseRepository\<TEntity\>** disponibiliza:
```csharp
Task<IEnumerable<TEntity>> GetAllAsync(
    params Expression<Func<TEntity, bool>>[] conditions);

Task<TEntity?> GetAsync(
    params Expression<Func<TEntity, bool>>[] conditions);

Task AddAsync(TEntity entity);

Task UpdateAsync(TEntity entity);
```

**Regras:**
- `GetAll` e `Get` aceitam múltiplas condições aplicadas dinamicamente
- Utilizar `IQueryable` para composição das consultas
- Todas as operações são assíncronas
- Nunca realizar queries dentro de loops

---

## Convenções de Nomenclatura

| Elemento | Convenção |
|---|---|
| Interfaces | Prefixo `I` — ex: `IUserRepository` |
| Classes | PascalCase |
| Métodos | PascalCase |
| Variáveis privadas | Prefixo `_` — ex: `_userRepository` |
| Enums | Prefixo `E` — ex: `EUserStatus` |

---

## English-Only Convention

Todo o código deve ser escrito em inglês. Isso inclui:

- Nomes de classes, interfaces, métodos, propriedades, variáveis
- Parâmetros, campos, DTOs, Commands, Queries, Validators, Services, Repositories
- Chaves de recursos (resource keys)

**Correto:**
```csharp
private readonly IUserRepository _userRepository;

public async Task<ResultDto<object>> CreateUserAsync(...)
```

**Incorreto:**
```csharp
private readonly IUserRepository _repositorioUsuario;

public async Task<ResultDto<object>> CriarUsuarioAsync(...)
```

---

## Injeção de Dependência

Cada camada possui uma classe de extensão própria:

```csharp
services.AddApplication();
services.AddInfrastructure(configuration);
```

---

## Convenções de Parâmetros de Métodos

Evitar métodos com excesso de parâmetros primitivos.

**Regras:**
- Até 2 parâmetros de negócio é aceitável
- Quando um método requer mais de 2 parâmetros de negócio, encapsulá-los em um DTO, Command, Query ou Request
- Preferir passar Commands, Queries ou DTOs em vez de múltiplos valores primitivos
- Esta regra se aplica especialmente a Application Services
- `CancellationToken` não conta para o limite — é um parâmetro de infraestrutura

**Correto:**
```csharp
Task<ResultDto<object>> CreateUserAsync(
    CreateUserCommand command,
    CancellationToken cancellationToken);
```

**Incorreto:**
```csharp
Task<ResultDto<object>> CreateUserAsync(
    string username,
    string email,
    string password,
    string confirmPassword,
    CancellationToken cancellationToken);
```

**Quando não existe Command ou Query adequado**, criar um DTO em `Lumiere.Application/DTOs` e usá-lo como entrada:

```csharp
// Application/DTOs/CreateUserDto.cs
Task<ResultDto<object>> CreateUserAsync(
    CreateUserDto dto,
    CancellationToken cancellationToken);
```

---

## Organização de Arquivos de Service

Dentro dos Application Services, a ordem dos membros deve seguir:

1. **Construtor** (via primary constructor na declaração da classe)
2. **Métodos públicos** (implementações de interface) — aparecem primeiro no corpo da classe
3. **Métodos privados auxiliares** — aparecem ao final da classe

Validações internas do service devem ser implementadas como métodos privados.

```
UserService
├── Primary Constructor
├── Métodos Públicos (implementações de IUserService)
└── Métodos Privados
    ├── ValidateCreateUserAsync
    ├── MapUser
    └── Outros auxiliares
```

Esta convenção se aplica a todos os services futuros.

---

## Gerenciamento de Mensagens de Erro

Mensagens de erro da camada Application **não devem ser hardcoded** dentro de Services, Commands, Queries, Validators ou Handlers.

Todas as mensagens reutilizáveis devem ser armazenadas em:

```
Application
└── Resources
    └── Errors.resx
```

**Correto:**
```csharp
result.AddError(Errors.EmailAlreadyInUse);
```

**Incorreto:**
```csharp
result.AddError("Email is already in use.");
```

A estrutura de recursos é compatível com localização futura:
```
Resources
├── Errors.resx         ← padrão (inglês)
├── Errors.pt-BR.resx   ← futuro
└── Errors.en-US.resx   ← futuro
```

---

## Responsabilidade de Validação de Negócio

**FluentValidation** deve ser usado apenas para validações de requisição:
- Campos obrigatórios
- Tamanho mínimo/máximo
- Formato (e-mail, regex)
- Senhas e confirmações

**Validações que dependem de banco de dados** devem ser implementadas dentro dos Application Services como métodos privados:
- Unicidade de username ou e-mail
- Validações referenciais
- Validações entre entidades

Repositórios **não devem conter lógica de validação** — sua responsabilidade é exclusivamente persistência e consulta.

---

## Boas Práticas

- Seguir princípios **SOLID** e **Clean Code**
- Evitar duplicação de código
- Usar `async`/`await` em toda operação de I/O
- Usar **DTOs** para comunicação entre API e Application — nunca expor entidades diretamente
- Usar **Result Pattern** para retorno de operações quando aplicável
- Usar **FluentValidation** para validação dos Commands
- Manter baixo acoplamento entre camadas
- Usar `CancellationToken` em operações assíncronas
- **Nunca realizar queries em loop**

---

## Convenções de Estilo de Código

- Sempre usar chaves `{}` em estruturas de controle, mesmo quando o corpo tiver apenas uma linha
- Sempre pular uma linha antes de estruturas de controle (`if`, `for`, `foreach`, `while`, `switch`)
- Sempre usar **primary constructor** quando a classe possuir um único construtor — inclusive ao passar parâmetros para a classe base
- **Remover todos os `using` não utilizados** — nenhum import desnecessário deve permanecer no arquivo
- **Manter indentação consistente** — usar 4 espaços em todo o projeto, sem misturar tabs e espaços
- **Nomes devem ser descritivos e autoexplicativos** — classes, métodos, commands, queries e variáveis devem deixar clara a intenção sem precisar de comentário; evitar abreviações e nomes genéricos como `data`, `obj`, `temp`, `handler2`

**Correto:**
```csharp
var result = DoSomething();

if (result is null)
{
    return NotFound();
}

foreach (var item in items)
{
    Process(item);
}
```

**Incorreto:**
```csharp
var result = DoSomething();
if (result is null)
    return NotFound();
foreach (var item in items)
    Process(item);
```

---

## Nomenclatura de Parâmetros Lambda

Evitar nomes de parâmetro de uma única letra ou genéricos em expressões lambda.

**Correto:**
```csharp
users.Where(user => user.Email == email)
channels.Any(channel => channel.Id == channelId)
permissions.Any(permission => permission.Name == name)
identityResult.Errors.Select(identityError => identityError.Description)
```

**Incorreto:**
```csharp
users.Where(u => u.Email == email)
channels.Any(x => x.Id == channelId)
identityResult.Errors.Select(e => e.Description)
```

Nomes genéricos proibidos: `x`, `y`, `z`, `u`, `e`, `i`. O nome deve refletir a entidade sendo processada **em inglês**. Esta convenção se aplica a todo o projeto.

**Exceções aceitas:**
- `_` como discard quando o parâmetro é intencionalmente ignorado: `.RuleFor(user => user.Active, _ => true)`
- Arquivos auto-gerados (Migrations, Designer.cs) — nunca modificar manualmente

---

## Dependências entre Camadas

```
API  →  Application  →  Domain
Infra  →  Domain
Tests  →  todas as camadas
```

Nenhuma camada deve violar as dependências estabelecidas pela Clean Architecture.

---

## Entidades do Domínio

### Convenções globais de entidades
- Todas as chaves primárias são do tipo `int`
- Todas as entidades possuem as colunas: `CreatedAt` (DateTime), `UpdatedAt` (DateTime?), `Active` (bool/bit)
- Mapeamentos via `IEntityTypeConfiguration<T>` na camada Infra

### User
- Herda de `IdentityUser<int>` (ASP.NET Core Identity com PK int)

---

## Documentação da API (Swagger)

- Utilizar **Swashbuckle.AspNetCore** para geração do spec e UI do Swagger
- A UI **só deve ser exposta em ambientes não-produção** (Development, Staging/Homologação, etc.)
- A lógica de ativação fica em `Extensions/SwaggerExtensions.cs` com dois métodos de extensão:
  - `AddSwaggerDocs()` — registra os serviços via `AddSwaggerGen()`
  - `UseSwaggerDocs()` — habilita `UseSwagger()` e `UseSwaggerUI()` se `!IsProduction()`
- **Nunca habilitar em produção** — a condição é `IsProduction()` para bloquear, não `IsDevelopment()` para permitir
- UI acessível em: `/swagger`

---

## Infraestrutura

### Docker
- `Dockerfile` na raiz para build da API (multi-stage)
- `docker-compose.yml` na raiz com containers: **API** + **SQL Server 2022**
- A API no compose lê a connection string via variável de ambiente `ConnectionStrings__Default`

### Banco de Dados
- **SQL Server** via Entity Framework Core
- Migrations geradas na camada `Lumiere.Infra`
- Porta padrão local: `1433`

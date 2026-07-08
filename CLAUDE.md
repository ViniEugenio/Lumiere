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
- Utilizar **Controllers do ASP.NET Core MVC** (não Minimal APIs) — motivo: rotas recebem objetos complexos via query string (ex: `GetUsersQuery` com `Name`, `Page`, `PageAmount`), e o model binder do MVC resolve isso nativamente sem precisar de `[AsParameters]` nem binders customizados
- Todas as rotas organizadas em controllers separados por funcionalidade
- Apenas receber requisições, validar entradas e encaminhar para a camada Application
- Configurações de DI registradas nesta camada
- Configurações de autenticação e autorização ficam nesta camada

**Estrutura:**
```
Lumiere.API/
├── Controllers/
├── Extensions/
├── Middlewares/
├── Configurations/
└── Program.cs
```

---

## Convenções de Controllers

### BaseController

Todos os controllers devem herdar de `BaseController` (localizado em `Lumiere.API/Controllers/BaseController.cs`).

`BaseController` recebe `ISender` via primary constructor e provê o método `Respond<T>(ResultDto<T> result)` para padronizar as respostas:
- Sucesso → `Ok(result.Data)`
- Falha → `BadRequest(result.Errors)`

```csharp
public abstract class BaseController(ISender sender) : Controller
{
    protected readonly ISender _sender = sender;

    protected IActionResult Respond<T>(ResultDto<T> result)
    {
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }
}
```

### Estrutura dos controllers

Cada funcionalidade deve ter seu próprio controller dentro de `Controllers/`, herdando de `BaseController` e decorado com `[Route("api/{resource}")]`:

```csharp
[Route("api/user")]
public class UserController(ISender sender) : BaseController(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetUsersQuery([FromQuery] GetUsersQuery query)
    {
        var result = await _sender.Send(query);
        return Respond(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _sender.Send(command);
        return Respond(result);
    }
}
```

**Regras:**
- Rota base do controller sempre com prefixo `api/` — ex: `[Route("api/user")]`
- Métodos de ação são assíncronos e retornam `Task<IActionResult>`
- O corpo do método só deve enviar o request via `_sender.Send()` e repassar o resultado para `Respond()` — nenhuma lógica de negócio no controller

### Registro no `Program.cs`

Controllers são registrados via extension methods próprios da camada API (`Extensions/APIExtensions.cs`):

```csharp
public static class APIExtensions
{
    public static void AddAPIExtensions(this IServiceCollection services)
    {
        services.AddControllers();
    }

    public static void AddAPIApplications(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.MapControllers();
    }
}
```

```csharp
builder.Services.AddAPIExtensions();
// ...
app.AddAPIApplications();
```

### Origens de parâmetros

- `[FromServices]` — serviços injetados via DI (prefira injetar no construtor sempre que possível)
- `[FromRoute]` — parâmetros de rota
- `[FromQuery]` — parâmetros de query string (suporta objetos complexos nativamente, ex: `GetUsersQuery`)
- `[FromBody]` — corpo da requisição
- `CancellationToken` — fornecido automaticamente pelo pipeline quando declarado como parâmetro da action

### Documentação OpenAPI

Usar `[Tags]` no controller para agrupar endpoints no Swagger e `[ProducesResponseType]` para documentar os possíveis retornos:

```csharp
[Tags("Users")]
[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class UserController(ISender sender) : BaseController(sender)
```

**Regras:**
- `[ProducesResponseType]` é apenas documentação — não valida nem força o retorno em tempo de execução
- Controllers com `[Authorize]` devem documentar `401` e `403`
- A tag deve corresponder ao nome do recurso (ex: `"Users"`, `"Products"`)

---

## Lumiere.Domain

Responsável pelas regras centrais do domínio.

**Regras:**
- **Não depende de nenhuma outra camada nem de frameworks/bibliotecas externas** — isso inclui ASP.NET Core, EF Core, MediatR, etc. Domain deve ser compilável referenciando apenas o BCL do .NET
- Contém apenas conceitos de domínio
- Todas as entidades ficam nesta camada
- Todas as interfaces dos repositórios ficam nesta camada
- Value Objects, Enums e Exceptions de domínio ficam nesta camada
- Tipos compartilhados de infraestrutura interna do domínio, sem semântica de negócio própria (ex: `BasePaginationResult<T>`) ficam em `Domain/Common` — devem ser POCOs puros, sem dependência externa. Padrões de comunicação de sucesso/erro de um caso de uso (ex: `ResultDto<T>`) são responsabilidade da Application, não do Domain — ficam em `Application/DTOs`
- Projeções específicas de um caso de uso (shapes de leitura como `UserPaginated`, usados só pra formatar a resposta de uma query) não são Value Objects de domínio — ficam em `Application/DTOs`, nunca em `Domain/ValueObjects`
- Sem exceções: nenhuma entidade do Domain deve herdar de classes de framework (ex: ASP.NET Core Identity). O projeto não usa ASP.NET Core Identity — autenticação/gerenciamento de usuário é responsabilidade própria, implementada via Infra (hashing de senha, etc.), nunca vazando pro Domain

**Estrutura:**
```
Lumiere.Domain/
├── Entities/
├── Interfaces/
├── ValueObjects/
├── Common/
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
- **CommandHandlers podem acessar repositórios diretamente quando são o único consumidor daquela orquestração** — o Handler já desempenha o papel de Use Case/Interactor (CQRS + Clean Architecture); um Service só se justifica quando uma mesma lógica de orquestração é reusada por **mais de um** Handler
- **Services existem apenas para eliminar duplicação real entre Handlers** — extrair um Service no momento em que um segundo Handler precisar da mesma orquestração, não preventivamente. Quando existir, o Service é responsável pela orquestração das regras de negócio e conversão de resultados compartilhada entre os Handlers que o consomem
- **Repositórios são responsáveis apenas pela persistência** — não convertem erros em DTOs
- Commands e Queries devem retornar `ResultDto<T>` quando precisam comunicar status de sucesso ou falha
- DTOs usados para comunicação entre Application e API ficam em `Lumiere.Application/DTOs` — Features não devem criar subpastas `DTOs/` próprias sem justificativa forte
- **Services devem receber Commands, Queries ou DTOs como parâmetro** — nunca uma lista de primitivos; ver regra de parâmetros abaixo
- **Depende apenas do Domain** — o `.csproj` da Application não deve referenciar ASP.NET Core (nem via `FrameworkReference`/`PackageReference`) ou qualquer outro framework de apresentação/infraestrutura. As únicas dependências externas aceitas são bibliotecas de aplicação do próprio padrão CQRS (MediatR, FluentValidation)
- Regra de uma única Entity (ex: invariantes de senha) pertence à própria Entity, nunca ao Handler ou ao Service

**Estrutura:**
```
Lumiere.Application/
├── DTOs/
├── Interfaces/
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

**Regra de `DTOs/` vs `Interfaces/`:** `DTOs/` contém apenas tipos concretos de transporte de dado (records/classes, ex: `ResultDto<T>`, `UserPaginated`). Contratos/abstrações que esses DTOs implementam (ex: `IResultDto`, usado pelo `ValidationBehavior` via generics) ficam em `Interfaces/` — mesma separação que o Domain já usa entre `Entities/` e `Interfaces/`. Não confundir com `Services/Interfaces/`, que é especificamente pra contratos de Service.

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
├── Security/
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
- **O Claude nunca deve fazer requisições diretas às rotas da API (curl, HTTP client, Swagger, etc.) para validar uma implementação** — isso grava dados reais no banco de desenvolvimento e atrapalha outros testes/cenários em andamento. Validação de comportamento deve ser feita exclusivamente via testes automatizados em `Lumiere.Tests` (unitários e de integração)

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
    CancellationToken cancellationToken,
    params Expression<Func<TEntity, bool>>[] conditions);

Task<TEntity?> GetAsync(
    CancellationToken cancellationToken,
    params Expression<Func<TEntity, bool>>[] conditions);

Task<bool> ExistsAsync(
    CancellationToken cancellationToken,
    params Expression<Func<TEntity, bool>>[] conditions);

Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

Task<BasePaginationResult<TResult>> GetAllPaginationAsync<TResult>(
    PaginationFilters<TEntity, TResult> filters,
    CancellationToken cancellationToken);
```

**Regras:**
- `GetAll`, `Get` e `Exists` aceitam múltiplas condições aplicadas dinamicamente
- Utilizar `IQueryable` para composição das consultas
- Todas as operações são assíncronas e recebem `CancellationToken`
- Paginação usa `PaginationFilters<TEntity, TResult>`/`BasePaginationResult<TResult>` (`Domain/Common`) em vez de parâmetros primitivos soltos
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

## Git Commit Convention

Todos os commits devem ser escritos em inglês.

Isso inclui título, corpo e qualquer descrição adicional da mensagem de commit.

**Correto:**
```
feat: add user creation endpoint with FluentValidation
fix: resolve duplicate email validation error
refactor: move business validation logic to UserService
```

**Incorreto:**
```
feat: adicionar endpoint de criação de usuário
fix: corrigir validação de email duplicado
```

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

## Organização de Arquivos de Handler e Service

Dentro de CommandHandlers/QueryHandlers e de Application Services, a ordem dos membros deve seguir:

1. **Construtor** (via primary constructor na declaração da classe)
2. **Métodos públicos** (`Handle`, ou implementações de interface no caso de um Service) — aparecem primeiro no corpo da classe
3. **Métodos privados auxiliares** — aparecem ao final da classe

Validações internas devem ser implementadas como métodos privados.

```
CreateUserCommandHandler
├── Primary Constructor (IUserRepository)
├── Handle (público)
└── Métodos Privados
    ├── ValidateCreateUserAsync
    └── Outros auxiliares
```

Esta convenção se aplica a todos os Handlers e Services futuros.

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

**Validações que dependem de banco de dados** devem ser implementadas como métodos privados dentro do Handler (ou do Service, quando este existir por reuso entre Handlers):
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

### BaseEntity
- `Lumiere.Domain/Common/BaseEntity.cs` é uma classe abstrata que concentra `CreatedAt`, `UpdatedAt`, `Active` e os métodos `Activate()`/`Deactivate()`
- **Toda nova entidade deve herdar de `BaseEntity`**, salvo justificativa explícita documentada no código (comentário explicando por que a entidade não se encaixa no ciclo de vida padrão)
- Ao criar uma nova entidade, confirme se ela deve herdar `BaseEntity` antes de redeclarar `CreatedAt`/`UpdatedAt`/`Active` manualmente
- `Id` continua declarado individualmente em cada entidade — não faz parte de `BaseEntity`
- Entidades atuais que seguem essa convenção: `User`, `Channel`

### User
- Entidade própria do Domain, sem herdar de nenhuma classe de framework — herda `BaseEntity` e não depende de nenhum pacote do ASP.NET Core Identity
- Campos: `Id`, `FirstName`, `LastName`, `Email`, `PasswordHash`, mais os herdados de `BaseEntity`
- `PasswordHash` é um primitivo (`string`); o Domain só armazena o valor, nunca conhece o algoritmo
- Não existe pacote de Identity referenciado em nenhuma camada da solução (Domain, Application, Infra ou API)

### Hashing de senha (`IPasswordHasher`)
- O algoritmo de hash (hoje PBKDF2, sem depender de `Microsoft.AspNetCore.Identity*`) fica atrás da interface `IPasswordHasher` (`Domain/Interfaces/IPasswordHasher.cs`), implementada em `Lumiere.Infra/Security/PasswordHasher.cs`
- **Quem decide chamar o hash é o Handler**, não o Repositório — ex: `CreateUserCommandHandler` injeta `IPasswordHasher`, chama `passwordHasher.Hash(request.Password)` e faz `user.SetPassword(...)` antes de persistir. Repositórios ficam responsáveis exclusivamente por persistência (`AddAsync`, `UpdateAsync`, etc.), nunca por aplicar políticas de segurança
- Isso torna a regra "senha sempre é hasheada antes de salvar" visível e testável na camada de Use Case, em vez de escondida dentro de uma implementação concreta de repositório

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

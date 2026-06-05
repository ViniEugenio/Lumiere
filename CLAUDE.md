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
- Services de aplicação para orquestrar processos de negócio quando necessário
- **Depende apenas do Domain**

**Estrutura:**
```
Lumiere.Application/
├── Features/
│   └── Users/
│       ├── Commands/
│       ├── Queries/
│       ├── Handlers/
│       └── DTOs/
├── Services/
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

## Injeção de Dependência

Cada camada possui uma classe de extensão própria:

```csharp
services.AddApplication();
services.AddInfrastructure(configuration);
```

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

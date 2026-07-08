---
name: create-paginated-route
description: Gera toda a infraestrutura de uma nova rota GET paginada (Controller, Feature, Query, QueryHandler, Validator, DTO) seguindo exatamente o padrão da rota GET Users existente no Lumiere. Use quando o usuário digitar /create-paginated-route ou descrever que quer uma nova rota de listagem/busca paginada de uma entidade.
---

Gera a infraestrutura completa (Clean Architecture + CQRS/MediatR) de uma nova rota `GET` paginada, usando a feature `Users` (`GetUsersQuery` / `GetUsersQueryHandler` / `GetUsersQueryValidator` / `UserController`) como referência canônica. Nunca inventar um padrão novo — sempre espelhar o que já existe.

## Passo 0 — Reler os arquivos de referência

Antes de gerar qualquer código, releia estes arquivos (o padrão pode ter mudado desde a última execução da skill):

- `src/Lumiere.API/Controllers/UserController.cs`
- `src/Lumiere.Application/Features/Common/BasePaginatedQuery.cs`
- `src/Lumiere.Application/Features/Users/Queries/GetUsersQuery.cs`
- `src/Lumiere.Application/Features/Users/Handlers/QueryHandlers/GetUsersQueryHandler.cs`
- `src/Lumiere.Application/Validators/BasePaginationValidator.cs`
- `src/Lumiere.Application/Validators/GetUsersQueryValidator.cs`
- `src/Lumiere.Application/DTOs/UserPaginated.cs`
- `src/Lumiere.Domain/Common/PaginationFilters.cs` e `BasePaginationResult.cs`
- `src/Lumiere.Domain/Interfaces/IBaseRepository.cs`
- `src/Lumiere.Application/Resources/Errors.resx` e `Errors.cs`

Os templates abaixo refletem o estado atual desses arquivos, mas o que está no repositório é sempre a fonte da verdade — se algo divergir, siga o arquivo real.

## Passo 1 — Coletar informações do usuário

Pergunte (em conversa normal, não precisa ser tudo de uma vez, mas agrupe o que der):

1. **Entidade do Domain** — qual entidade em `Domain/Entities` essa rota pagina (ex: `Channel`). Precisa existir; a skill não cria entidades novas. A partir dela deduz-se o repositório (`I{Entity}Repository`, já deve existir em `Domain/Interfaces` e estar registrado em `RepositoriesExtensions`).
2. **Controller** — em qual controller a rota entra. Se não existir, será criado.
3. **Endpoint** — rota HTTP do endpoint (normalmente a raiz do resource, igual ao `GetUsers` que é só `[HttpGet]` em `api/user`; se for um sub-caminho, ex. `api/channel/search`, use `[HttpGet("search")]`).
4. **Feature** — nome da pasta em `Application/Features` (ex: `Channels`). Se não existir, criar a estrutura completa (`Queries/`, `Handlers/QueryHandlers/`).
5. **Filtros** — lista de campos de filtro (nome + propriedade da entidade correspondente, quando o nome divergir).
6. **Campos de resposta (DTO de projeção)** — quais propriedades da entidade aparecem no `{Entity}Paginated`. Padrão: todas as propriedades públicas relevantes, exceto navegações de coleção e campos sensíveis (ex: `PasswordHash`). Confirme com o usuário.
7. **Lógica de ordenação dos dados de retorno** — não pergunte apenas "qual campo usar no OrderBy". Peça para o usuário **descrever, com as próprias palavras, como a ordenação deve funcionar** (ex: um campo só, mais de um campo em sequência, ascendente ou descendente, alguma regra condicional, etc.) — na mesma rodada em que confirma filtros e campos do DTO, não trate como opcional nem decida sozinho. A partir da descrição, você traduz para o `orderByExpression` do Handler. Antes de gerar código, avise o usuário se a lógica descrita não couber no padrão atual (`BaseRepository.GetAllPaginationAsync` hoje só aplica `.OrderBy(...)` ascendente sobre uma única expressão, fixa no Handler — sem direção configurável nem múltiplos campos via query string, igual ao `GetUsersQueryHandler` que ordena por `FirstName`) e pergunte se ele quer estender esse padrão (fora do escopo automático desta skill, precisa confirmação explícita) ou simplificar a lógica pra caber no que já existe. Só use a primeira propriedade `string` da entidade, ascendente, como default se o usuário responder explicitamente que não tem preferência.

Não prossiga para geração de código com informação essencial faltando (entidade, feature, filtros, campos do DTO, campo de ordenação) — pergunte de novo.

## Passo 2 — Tornar `BasePaginatedQuery` e `BasePaginationValidator` genéricos (uma vez só)

Hoje `BasePaginatedQuery` está fixo em `UserPaginated`, então só serve para a feature `Users`. Antes de criar a segunda feature paginada, generalizar (idempotente — pule este passo se já estiver genérico):

`src/Lumiere.Application/Features/Common/BasePaginatedQuery.cs`:
```csharp
using Lumiere.Application.DTOs;
using Lumiere.Domain.Common;
using MediatR;

namespace Lumiere.Application.Features.Common;

public record BasePaginatedQuery<TResult>(int? Page, int? PageAmount) : IRequest<ResultDto<BasePaginationResult<TResult>>>
{
    public int? Page { get; private set; } = Page ?? 1;
    public int? PageAmount { get; private set; } = PageAmount ?? 10;
}
```

`src/Lumiere.Application/Validators/BasePaginationValidator.cs`:
```csharp
using FluentValidation;
using Lumiere.Application.Features.Common;
using Lumiere.Application.Resources;

namespace Lumiere.Application.Validators;

public class BasePaginationValidator<TResult> : AbstractValidator<BasePaginatedQuery<TResult>>
{
    public BasePaginationValidator()
    {
        int validMinPage = 1;

        RuleFor(basePagination => basePagination.Page)
            .GreaterThanOrEqualTo(validMinPage)
            .WithMessage(Errors.InvalidPage);

        int validMinPageAmount = 1;
        int validMaxPageAmount = 100;

        RuleFor(basePagination => basePagination.PageAmount)
            .InclusiveBetween(validMinPageAmount, validMaxPageAmount)
            .WithMessage(Errors.InvalidPageAmount);
    }
}
```

Atualizar os dois usos existentes em `Users` para fechar o genérico:

- `GetUsersQuery.cs` → `: BasePaginatedQuery<UserPaginated>(Page, PageAmount)`
- `GetUsersQueryValidator.cs` → `Include(new BasePaginationValidator<UserPaginated>());`

Depois desse passo, `dotnet build` deve continuar passando sem alterar nenhum outro comportamento da feature `Users`.

## Passo 3 — Estrutura da Feature

Se `src/Lumiere.Application/Features/{Feature}/` não existir, criar:
```
Features/{Feature}/
├── Queries/
└── Handlers/
    └── QueryHandlers/
```
(mesma organização de `Features/Users`; não criar `Commands/` a menos que já exista uma feature de comando pra essa entidade).

## Passo 4 — DTO de projeção

`src/Lumiere.Application/DTOs/{Entity}Paginated.cs`:
```csharp
namespace Lumiere.Application.DTOs;

public record {Entity}Paginated({campos confirmados no Passo 1, PascalCase, tipos iguais aos da entidade});
```

## Passo 5 — Query

`src/Lumiere.Application/Features/{Feature}/Queries/Get{Feature}Query.cs`:
```csharp
using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Common;

namespace Lumiere.Application.Features.{Feature}.Queries;

public record Get{Feature}Query({filtro1}, {filtro2}, ..., int? Page, int? PageAmount)
    : BasePaginatedQuery<{Entity}Paginated>(Page, PageAmount);
```
Cada filtro é opcional (`string?`, `bool?`, `int?`, `DateTime?`, etc. — nunca obrigatório, igual ao `Name` de `GetUsersQuery`). Os parâmetros de filtro vêm antes de `Page`/`PageAmount`, na mesma ordem em que foram pedidos ao usuário.

## Passo 6 — QueryHandler

`src/Lumiere.Application/Features/{Feature}/Handlers/QueryHandlers/Get{Feature}QueryHandler.cs`:
```csharp
using Lumiere.Application.DTOs;
using Lumiere.Application.Features.{Feature}.Queries;
using Lumiere.Domain.Common;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Lumiere.Application.Features.{Feature}.Handlers.QueryHandlers;

public class Get{Feature}QueryHandler(I{Entity}Repository {entityCamel}Repository)
    : IRequestHandler<Get{Feature}Query, ResultDto<BasePaginationResult<{Entity}Paginated>>>
{
    public async Task<ResultDto<BasePaginationResult<{Entity}Paginated>>> Handle(Get{Feature}Query request, CancellationToken cancellationToken)
    {
        Expression<Func<{Entity}, bool>> filterExpression = {entityLower} =>
            {condição do filtro 1} &&
            {condição do filtro 2} &&
            ...;

        Expression<Func<{Entity}, object>> orderByExpression = {entityLower} => {entityLower}.{CampoDeOrdenacao};

        Expression<Func<{Entity}, {Entity}Paginated>> selectorExpression = {entityLower} => new {Entity}Paginated(
            {entityLower}.Campo1,
            {entityLower}.Campo2,
            ...);

        PaginationFilters<{Entity}, {Entity}Paginated> paginationFilter = new(
            request.Page!.Value,
            request.PageAmount!.Value,
            filterExpression,
            orderByExpression,
            selectorExpression);

        BasePaginationResult<{Entity}Paginated> {entityCamel}PaginatedResult = await {entityCamel}Repository
            .GetAllPaginationAsync(paginationFilter, cancellationToken);

        ResultDto<BasePaginationResult<{Entity}Paginated>> result = new();
        result.SetData({entityCamel}PaginatedResult);

        return result;
    }
}
```

Regras da nomenclatura lambda: nunca usar `x`/`e`/`u` — usar o nome da entidade em minúsculo (`channel`, `user`), igual ao resto do projeto.

Padrão de condição por tipo de filtro (uma cláusula por filtro, combinadas com `&&`):

| Tipo do filtro | Condição |
|---|---|
| `string?` | `(string.IsNullOrEmpty(request.{Filtro}) \|\| {entidade}.{Propriedade}.Contains(request.{Filtro}))` |
| `bool?` / `int?` / enum `?` | `(!request.{Filtro}.HasValue \|\| {entidade}.{Propriedade} == request.{Filtro})` |
| `DateTime?` (data exata) | `(!request.{Filtro}.HasValue \|\| {entidade}.{Propriedade}.Date == request.{Filtro}.Value.Date)` |

Se um filtro de data precisar ser um intervalo (`From`/`To`) em vez de data exata, confirme isso com o usuário antes de gerar — não assuma.

## Passo 7 — Validator

`src/Lumiere.Application/Validators/Get{Feature}QueryValidator.cs`:
```csharp
using FluentValidation;
using Lumiere.Application.Features.{Feature}.Queries;

namespace Lumiere.Application.Validators;

public class Get{Feature}QueryValidator : AbstractValidator<Get{Feature}Query>
{
    public Get{Feature}QueryValidator()
    {
        Include(new BasePaginationValidator<{Entity}Paginated>());

        {regras extras por filtro, só se o usuário pedir — o GetUsersQueryValidator de referência não valida o campo Name}
    }
}
```
Se alguma regra extra precisar de mensagem de erro nova, adicionar em `Errors.resx` (`<data name="...">`) **e** a propriedade correspondente em `Errors.cs` — nunca mensagem hardcoded (ver "Gerenciamento de Mensagens de Erro" no `CLAUDE.md`).

## Passo 8 — Controller

Se o controller já existir, adicionar a action. Se não existir, criar seguindo exatamente o `UserController.cs` atual (sem `[Tags]`/`[ProducesResponseType]` a menos que o controller de referência já use isso quando você reler o Passo 0):
```csharp
[HttpGet{("{endpoint}") se o endpoint não for a raiz do resource}]
public async Task<IActionResult> Get{Feature}([FromQuery] Get{Feature}Query query)
{
    ResultDto<BasePaginationResult<{Entity}Paginated>> result = await _sender.Send(query);
    return Respond(result);
}
```
Controller novo:
```csharp
using Lumiere.Application.DTOs;
using Lumiere.Application.Features.{Feature}.Queries;
using Lumiere.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers;

[Route("api/{resource}")]
public class {Entity}Controller(ISender sender) : BaseController(sender)
{
    [HttpGet]
    public async Task<IActionResult> Get{Feature}([FromQuery] Get{Feature}Query query)
    {
        ResultDto<BasePaginationResult<{Entity}Paginated>> result = await _sender.Send(query);
        return Respond(result);
    }
}
```

MediatR e FluentValidation são registrados via assembly scan (`AddMediatr`, `AddValidators`) — não é preciso registrar a Query/Handler/Validator novos manualmente em nenhum lugar. Se o repositório da entidade (`I{Entity}Repository`) ainda não existir ou não estiver registrado em `RepositoriesExtensions`, avise o usuário — criar repositório está fora do escopo desta skill.

## Passo 9 — Build

Rodar `dotnet build` na solução para garantir que compila. **Não** fazer requisições HTTP diretas à rota nova (curl, Swagger, etc.) para validar — isso grava dado real no banco de desenvolvimento; validação de comportamento é via testes automatizados em `Lumiere.Tests`, conforme `CLAUDE.md`.

## Passo 10 — Fechar

Resuma pro usuário os arquivos criados/alterados. Não existe hoje nenhum teste de referência para rota paginada em `Lumiere.Tests` (a feature `Users` também não tem) — então esta skill não gera testes automaticamente. Pergunte se o usuário quer que você escreva os testes da nova query/handler agora, como um passo separado.

## Regras gerais

- Sempre usar a implementação atual de `Users` como referência — releia os arquivos do Passo 0 a cada execução, não confie em versões memorizadas.
- Não inventar padrão novo além da generalização do Passo 2 (que é uma correção mínima para o padrão existente suportar mais de uma entidade, não um padrão novo).
- Reaproveitar `IBaseRepository<T>.GetAllPaginationAsync` — nunca escrever paginação manual no Handler.
- Seguir todas as convenções de `CLAUDE.md`: chaves em blocos de controle, primary constructor, `_` para campos privados, inglês em todo o código, ordem construtor → métodos públicos → métodos privados, sem parâmetro lambda de uma letra.
- Se qualquer informação necessária (entidade, filtros, campos do DTO, campo de ordenação) não tiver sido confirmada pelo usuário, perguntar antes de gerar código — não assumir.

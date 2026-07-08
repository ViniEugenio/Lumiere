using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Common;

namespace Lumiere.Application.Features.Users.Queries;

public record GetUsersQuery(string? Name, int? Page, int? PageAmount) : BasePaginatedQuery<UserPaginated>(Page, PageAmount);

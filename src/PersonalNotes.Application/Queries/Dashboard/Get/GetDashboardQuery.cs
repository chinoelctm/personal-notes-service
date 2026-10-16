using MediatR;

namespace PersonalNotes.Application.Queries.Dashboard.Get
{
    public record GetDashboardQuery : IRequest<GetDashboardResponse>;
}

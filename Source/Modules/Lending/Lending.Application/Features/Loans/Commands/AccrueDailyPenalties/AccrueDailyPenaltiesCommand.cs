using MediatR;

namespace Lending.Application.Features.Loans.Commands.AccrueDailyPenalties;

public record AccrueDailyPenaltiesCommand : IRequest<AccrueDailyPenaltiesResult>;
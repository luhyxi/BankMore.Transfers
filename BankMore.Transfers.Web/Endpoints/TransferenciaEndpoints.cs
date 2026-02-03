using BankMore.Transfers.Application.Transferencia.Command.Create;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace BankMore.Transfers.Web.Endpoints;

public static class TransferenciaEndpoints
{
    public static void MapTransferenciaEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("transferencia",
                async (
                    [FromBody] CreateTransferenciaRequest request,
                    HttpContext httpContext,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        if (httpContext.User.Identity?.IsAuthenticated != true)
                        {
                            return Results.StatusCode(StatusCodes.Status403Forbidden);
                        }

                        var token = httpContext.Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                        var senderNumber = httpContext.User.FindFirst("numero")?.Value;
                        var subject = httpContext.User.FindFirst("id")?.Value;

                        if (string.IsNullOrWhiteSpace(senderNumber) ||
                            string.IsNullOrWhiteSpace(subject) || !Guid.TryParse(subject, out _))
                        {
                            return Results.StatusCode(StatusCodes.Status403Forbidden);
                        }

                        var command = new CreateTransferenciaCommand(
                            token,
                            request.ReceiverNumber,
                            request.Amount);
                        var result = await mediator.Send(command, cancellationToken);

                        if (!result.IsSuccess) return Results.BadRequest(new { error = result.Error });

                        return Results.NoContent();
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .WithName("CreateTransferencia")
            .WithSummary("Creates a transaction between registered accounts")
            .WithDescription("Creates a credit transaction for the receiver account and generates a debit transaction for the sender account")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create transaction between registered accounts";
                operation.Description = @"Creates a credit transaction for the receiver account and generates a debit transaction for the sender account
                - Validates that only registered and active accounts can receive transactions
                - Only positive values are accepted
                - Only 'C' (Credit) or 'D' (Debit) types are allowed
                - Requires authentication token in the request header";
                return operation;
            });

    }
}

public sealed record CreateTransferenciaRequest(string ReceiverNumber, decimal Amount);

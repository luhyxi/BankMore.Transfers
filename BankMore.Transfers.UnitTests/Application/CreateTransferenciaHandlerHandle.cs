using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using BankMore.Transfers.Application.Transferencia.Command.Create;
using BankMore.Transfers.Domain.Interfaces;
using NSubstitute.ExceptionExtensions;

namespace BankMore.Transfers.UnitTests.Application;

public class CreateTransferenciaHandlerHandle
{
    private const string SenderId = "sender-123";
    private const string SenderNumber = "000001";
    private const string ReceiverNumber = "000002";

    [Fact]
    public async Task Handle_when_token_is_missing_returns_failure()
    {
        var handler = CreateHandler(out var repository, out var service);
        var command = new CreateTransferenciaCommand("", ReceiverNumber, 10m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Authentication token must be provided.");
        await service.DidNotReceiveWithAnyArgs().RealizeDebit(default!, default);
        await repository.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_when_amount_is_invalid_returns_failure()
    {
        var handler = CreateHandler(out var repository, out var service);
        var command = new CreateTransferenciaCommand(CreateJwt(), ReceiverNumber, 0m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Transfer amount must be greater than zero.");
        await service.DidNotReceiveWithAnyArgs().RealizeDebit(default!, default);
        await repository.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_when_receiver_is_missing_returns_failure()
    {
        var handler = CreateHandler(out var repository, out var service);
        var command = new CreateTransferenciaCommand(CreateJwt(), "", 10m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Destination account number must be provided.");
        await service.DidNotReceiveWithAnyArgs().RealizeDebit(default!, default);
        await repository.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_when_debit_fails_returns_failure()
    {
        var handler = CreateHandler(out var repository, out var service);
        var command = new CreateTransferenciaCommand(CreateJwt(), ReceiverNumber, 10m);

        service.RealizeDebit(command.JwtToken, command.Amount).Returns(new ValueTask<bool>(false));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("debit_failed: checking account service returned an empty transaction id.");
        await service.DidNotReceiveWithAnyArgs().RealizeCredit(default!, default, default);
        await repository.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_when_credit_fails_reverses_and_returns_failure()
    {
        var handler = CreateHandler(out var repository, out var service);
        var command = new CreateTransferenciaCommand(CreateJwt(), ReceiverNumber, 10m);

        service.RealizeDebit(command.JwtToken, command.Amount).Returns(new ValueTask<bool>(true));
        service.RealizeCredit(command.JwtToken, command.Amount, command.ReceiverAccountNumber)
            .Returns(new ValueTask<bool>(false));
        service.RealizeCredit(command.JwtToken, command.Amount, null)
            .Returns(new ValueTask<bool>(true));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(
            "credit_failed: checking account service returned an empty transaction id. reversal_performed: credit reversal succeeded.");
        await service.Received(1).RealizeCredit(command.JwtToken, command.Amount, null);
        await repository.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_when_credit_throws_reverses_and_returns_failure()
    {
        var handler = CreateHandler(out var repository, out var service);
        var command = new CreateTransferenciaCommand(CreateJwt(), ReceiverNumber, 10m);

        service.RealizeDebit(command.JwtToken, command.Amount).Returns(new ValueTask<bool>(true));
        service.RealizeCredit(command.JwtToken, command.Amount, command.ReceiverAccountNumber)
            .ThrowsAsync(new HttpRequestException("credit down"));
        service.RealizeCredit(command.JwtToken, command.Amount, null)
            .Returns(new ValueTask<bool>(false));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(
            "credit_failed: credit down. reversal_failed: checking account service returned an failed result.");
        await service.Received(1).RealizeCredit(command.JwtToken, command.Amount, null);
        await repository.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_when_everything_succeeds_returns_success()
    {
        var handler = CreateHandler(out var repository, out var service);
        var command = new CreateTransferenciaCommand(CreateJwt(), ReceiverNumber, 10m);

        service.RealizeDebit(command.JwtToken, command.Amount).Returns(new ValueTask<bool>(true));
        service.RealizeCredit(command.JwtToken, command.Amount, command.ReceiverAccountNumber)
            .Returns(new ValueTask<bool>(true));
        service.GetAccountUuidByAccountNumber(command.JwtToken, command.ReceiverAccountNumber)
            .Returns(new ValueTask<string>("receiver-uuid-123"));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.EventId.ShouldNotBe(Guid.Empty);
        await repository.Received(1)
            .CreateAsync(Arg.Any<BankMore.Transfers.Domain.TransferenciaAggregate.Transferencia>(), Arg.Any<CancellationToken>());
    }

    private static CreateTransferenciaHandler CreateHandler(
        out ITransferenciaRepository repository,
        out IContaCorrenteService service)
    {
        repository = Substitute.For<ITransferenciaRepository>();
        service = Substitute.For<IContaCorrenteService>();
        var logger = Substitute.For<ILogger<CreateTransferenciaHandler>>();

        return new CreateTransferenciaHandler(repository, service, logger);
    }

    private static string CreateJwt()
    {
        var token = new JwtSecurityToken(
            claims: new[]
            {
                new Claim("id", SenderId),
                new Claim("number", SenderNumber)
            });

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

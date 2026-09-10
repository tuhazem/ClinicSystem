using ClinicSystem.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ClinicSystem.UnitTests.Application;

public record DummyCommand(string Name) : IRequest<string>;

public class DummyCommandValidator : AbstractValidator<DummyCommand>
{
    public DummyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenValidationSucceeds_ShouldCallNextDelegate()
    {
        // Arrange
        var validator = new DummyCommandValidator();
        var behavior = new ValidationBehavior<DummyCommand, string>(new[] { validator });
        var command = new DummyCommand("Valid Name");
        var nextCalled = false;

        RequestHandlerDelegate<string> next = (ct) =>
        {
            nextCalled = true;
            return Task.FromResult("Success");
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new DummyCommandValidator();
        var behavior = new ValidationBehavior<DummyCommand, string>(new[] { validator });
        var command = new DummyCommand(""); // Invalid
        var nextCalled = false;

        RequestHandlerDelegate<string> next = (ct) =>
        {
            nextCalled = true;
            return Task.FromResult("Success");
        };

        // Act
        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required.");
        nextCalled.Should().BeFalse();
    }
}

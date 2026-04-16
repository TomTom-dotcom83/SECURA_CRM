using FluentValidation;
using MediatR;
using Moq;
using SECURA.Application.Common.Behaviors;

namespace SECURA.Application.Tests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_Passes_Through_When_No_Validators()
    {
        var next = new Mock<RequestHandlerDelegate<string>>();
        next.Setup(n => n()).ReturnsAsync("ok");

        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var result = await behavior.Handle(new TestRequest("valid"), next.Object, CancellationToken.None);

        result.Should().Be("ok");
        next.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_ValidationException_When_Invalid()
    {
        var validator = new TestRequestValidator();
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);

        var next = new Mock<RequestHandlerDelegate<string>>();
        next.Setup(n => n()).ReturnsAsync("should-not-reach");

        var act = async () => await behavior.Handle(
            new TestRequest(""),  // empty = invalid per validator
            next.Object,
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Value*");

        next.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_Passes_Through_When_Valid()
    {
        var validator = new TestRequestValidator();
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);

        var next = new Mock<RequestHandlerDelegate<string>>();
        next.Setup(n => n()).ReturnsAsync("ok");

        var result = await behavior.Handle(
            new TestRequest("hello"),
            next.Object,
            CancellationToken.None);

        result.Should().Be("ok");
    }
}

public sealed record TestRequest(string Value) : IRequest<string>;

public sealed class TestRequestValidator : AbstractValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required.");
    }
}

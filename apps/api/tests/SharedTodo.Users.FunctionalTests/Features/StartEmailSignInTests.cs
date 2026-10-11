using SharedTodo.Users.Contracts.Events;
using SharedTodo.Users.Features;
using SharedTodo.Users.FunctionalTests.TestSupport;

namespace SharedTodo.Users.FunctionalTests.Features;

public class StartEmailSignInTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact]
    public async Task ShouldRequireMinimumFields()
    {
        // Arrange
        var command = new StartEmailSignInCommand(string.Empty);

        // Act
        var result = await SendAsync(command);

        // Assert
        result.IsError.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateUserAndSendCodeForNewEmail()
    {
        // Arrange
        var command = new StartEmailSignInCommand("user@spike.test");

        // Act
        var result = await SendAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var user = await Users.FindByEmailAsync(command.Email);
        user.ShouldNotBeNull();

        var @event = Publisher.Notifications
            .OfType<EmailSignInStartedEvent>()
            .ShouldHaveSingleItem();

        @event.Email.ShouldBe(command.Email);
        @event.Code.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldSendCodeForExistingEmail()
    {
        // Arrange
        var command = new StartEmailSignInCommand("user@spike.test");

        await Users.CreateAsync(command.Email);

        // Act
        var result = await SendAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var @event = Publisher.Notifications
            .OfType<EmailSignInStartedEvent>()
            .ShouldHaveSingleItem();

        @event.Email.ShouldBe(command.Email);
        @event.Code.ShouldNotBeNull();
    }
}

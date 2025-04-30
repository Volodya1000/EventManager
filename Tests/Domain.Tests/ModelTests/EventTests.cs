using EventManager.Domain.Models;
using FluentAssertions;

namespace Domain.Tests.ModelTests;

public class EventTests
{
    private readonly Event _testEvent;
    private readonly Participant _testParticipant;

    public EventTests()
    {
        _testEvent = CreateTestEvent();
        _testParticipant = CreateTestParticipant(_testEvent.Id);
    }

    #region Test Helpers

    private static Event CreateTestEvent(
        string name = "Test Event",
        int maxParticipants = 2,
        DateTime? dateTime = null)
    {
        return Event.Create(
            Guid.NewGuid(),
            name,
            "Test Description",
            dateTime ?? DateTime.UtcNow.AddDays(1),
            "Test Location",
            "Test Category",
            maxParticipants);
    }

    private static Participant CreateTestParticipant(
        Guid eventId,
        Guid? userId = null,
        string firstName = "John",
        string lastName = "Doe")
    {
        return Participant.Create(
            userId ?? Guid.NewGuid(),
            eventId,
            DateTime.UtcNow,
            firstName,
            lastName,
            DateTime.UtcNow.AddYears(-20));
    }

    #endregion

    [Theory(DisplayName = "Create: Происходит exception при невалидном названии")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        FluentActions.Invoking(() => CreateTestEvent(name: invalidName))
            .Should().Throw<ArgumentException>();
    }

    [Theory(DisplayName = "Create: Происходит exception при невалидном количестве участников")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidMaxParticipants_ThrowsArgumentException(int invalidMax)
    {
        // Act & Assert
        FluentActions.Invoking(() => CreateTestEvent(maxParticipants: invalidMax))
            .Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "UpdateDescription: Обновляет описание при валидных данных")]
    public void UpdateDescription_WithValidData_UpdatesProperty()
    {
        // Arrange
        const string newDescription = "New Description";

        // Act
        _testEvent.UpdateDescription(newDescription);

        // Assert
        _testEvent.Description.Should().Be(newDescription);
    }

    [Theory(DisplayName = "UpdateDescription: Происходит exception при пустом описании")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateDescription_WithInvalidData_ThrowsArgumentException(string invalidDescription)
    {
        // Act & Assert
        FluentActions.Invoking(() => _testEvent.UpdateDescription(invalidDescription))
            .Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "UpdateDateTime: Обновляет дату и время при валидных данных")]
    public void UpdateDateTime_WithValidData_UpdatesProperty()
    {
        // Arrange
        var newDateTime = DateTime.UtcNow.AddDays(2);

        // Act
        _testEvent.UpdateDateTime(newDateTime);

        // Assert
        _testEvent.DateTime.Should().Be(newDateTime);
    }

    [Fact(DisplayName = "UpdateDateTime: Происходит exception при установке прошедшей даты")]
    public void UpdateDateTime_WithPastDate_ThrowsArgumentException()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        FluentActions.Invoking(() => _testEvent.UpdateDateTime(pastDate))
            .Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "UpdateLocation: Обновляет локацию при валидных данных")]
    public void UpdateLocation_WithValidData_UpdatesProperty()
    {
        // Arrange
        const string newLocation = "New Location";

        // Act
        _testEvent.UpdateLocation(newLocation);

        // Assert
        _testEvent.Location.Should().Be(newLocation);
    }

    [Theory(DisplayName = "UpdateLocation: Происходит exception при невалидной локации")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateLocation_WithInvalidData_ThrowsArgumentException(string invalidLocation)
    {
        // Act & Assert
        _testEvent.Invoking(e => e.UpdateLocation(invalidLocation))
            .Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "AddParticipant: Происходит exception при дублировании пользователя")]
    public void AddParticipant_WithDuplicateUserId_ThrowsException()
    {
        // Arrange
        var duplicateParticipant = CreateTestParticipant(
            _testEvent.Id,
            _testParticipant.UserId,
            "Jane",
            "Doe");

        _testEvent.AddParticipant(_testParticipant);

        // Act & Assert
        FluentActions.Invoking(() => _testEvent.AddParticipant(duplicateParticipant))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "RemoveParticipant: Возвращает false при отсутствии участника")]
    public void RemoveParticipant_WithNonExistingUser_ReturnsFalse()
    {
        // Act & Assert
        FluentActions.Invoking(() => _testEvent.RemoveParticipant(Guid.NewGuid()))
              .Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "RemoveParticipant: Удаляет существующего участника")]
    public void RemoveParticipant_WithExistingUser_ReturnsTrue()
    {
        // Arrange
        _testEvent.AddParticipant(_testParticipant);

        // Act
        var result = _testEvent.RemoveParticipant(_testParticipant.UserId);

        // Assert
        result.Should().BeTrue();
        _testEvent.RegisteredParticipants.Should().Be(0);
    }
}
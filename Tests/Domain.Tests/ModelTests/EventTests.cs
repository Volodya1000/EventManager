using EventManager.Domain.Models;

namespace Domain.Tests.ModelTests;

public class EventTests
{
    private readonly Event _testEvent;
    private readonly Participant _testParticipant;

    public EventTests()
    {
        _testEvent = Event.Create(
            Guid.NewGuid(),
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            "Test Location",
            "Test Category",
            2);

        _testParticipant = Participant.Create(
            Guid.NewGuid(),
            _testEvent.Id,
            DateTime.UtcNow,
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-20));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void CreateEvent_InvalidName_ThrowsException(string name)
    {
        Assert.Throws<ArgumentException>(() => Event.Create(
            Guid.NewGuid(),
            name,
            "Description",
            DateTime.UtcNow.AddDays(1),
            "Location",
            "Category",
            10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateEvent_InvalidMaxParticipants_ThrowsException(int maxParticipants)
    {
        Assert.Throws<ArgumentException>(() => Event.Create(
            Guid.NewGuid(),
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(1),
            "Location",
            "Category",
            maxParticipants));
    }

    [Fact]
    public void UpdateDescription_ValidData_UpdatesCorrectly()
    {
        var newDescription = "New Description";
        _testEvent.UpdateDescription(newDescription);
        Assert.Equal(newDescription, _testEvent.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateDescription_InvalidData_ThrowsException(string description)
    {
        Assert.Throws<ArgumentException>(() => _testEvent.UpdateDescription(description));
    }

    [Fact]
    public void UpdateDateTime_ValidData_UpdatesCorrectly()
    {
        var newDateTime = DateTime.UtcNow.AddDays(2);
        _testEvent.UpdateDateTime(newDateTime);
        Assert.Equal(newDateTime, _testEvent.DateTime);
    }

    [Fact]
    public void UpdateDateTime_PastDate_ThrowsException()
    {
        var pastDate = DateTime.UtcNow.AddDays(-1);
        Assert.Throws<ArgumentException>(() => _testEvent.UpdateDateTime(pastDate));
    }

    [Fact]
    public void AddParticipant_DuplicateUserId_ThrowsException()
    {
        var participant2 = Participant.Create(
            _testParticipant.UserId,
            _testEvent.Id,
            DateTime.UtcNow,
            "Jane",
            "Doe",
            DateTime.UtcNow.AddYears(-20));

        _testEvent.AddParticipant(_testParticipant);
        Assert.Throws<InvalidOperationException>(() => _testEvent.AddParticipant(participant2));
    }


}

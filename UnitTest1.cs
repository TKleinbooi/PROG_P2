
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Test;

public class UnitTest1
{
    private readonly RaceDayApiFactory _factory;
    private readonly HttpClient _client;

    public UnitTest1()
    {
        _factory = new RaceDayApiFactory();
        _client = _factory.CreateClient();
        _factory.InitializeDatabase();
    }

    private async Task RegisterAsync(
        string email,
        string role = "Participant")
    {
        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                firstName = "Test",
                lastName = "User",
                email,
                password = "TestPassword123!",
                role
            });

        response.EnsureSuccessStatusCode();
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new
            {
                email,
                password = "TestPassword123!"
            });

        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return json.RootElement
            .GetProperty("token").GetString()!;
    }

    private void UseToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<HttpResponseMessage> CreateEventAsync(
        string token)
    {
        UseToken(token);

        return await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                name = "Test Fun Run",
                description = "Integration test event",
                eventDate = DateTime.UtcNow.AddDays(30),
                location = "Cape Town",
                distance = 5m,
                eventType = "Run"
            });
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                firstName = "Test",
                lastName = "Participant",
                email = "register@test.example",
                password = "TestPassword123!",
                role = "Participant"
            });

        Assert.Equal(
            HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        await RegisterAsync("duplicate@test.example");

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                firstName = "Another",
                lastName = "User",
                email = "duplicate@test.example",
                password = "TestPassword123!",
                role = "Participant"
            });

        Assert.Equal(
            HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                firstName = "Test",
                lastName = "User",
                email = "invalidrole@test.example",
                password = "TestPassword123!",
                role = "Admin"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        await RegisterAsync("login@test.example");

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new
            {
                email = "login@test.example",
                password = "TestPassword123!"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        await RegisterAsync("wrongpassword@test.example");

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new
            {
                email = "wrongpassword@test.example",
                password = "WrongPassword123!"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEvents_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/Events");

        Assert.Equal(
            HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanCreateEvent()
    {
        await RegisterAsync(
            "organiser@test.example", "Organiser");

        var token = await LoginAsync("organiser@test.example");

        var response = await CreateEventAsync(token);

        Assert.Equal(
            HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        await RegisterAsync("participant@test.example");

        var token = await LoginAsync("participant@test.example");

        var response = await CreateEventAsync(token);

        Assert.Equal(
            HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CanEnrol_AndViewEnrolment()
    {
        // Create an event as an Organiser.
        await RegisterAsync(
            "enrol.organiser@test.example", "Organiser");

        var organiserToken =
            await LoginAsync("enrol.organiser@test.example");

        var eventResponse =
            await CreateEventAsync(organiserToken);

        Assert.Equal(
            HttpStatusCode.Created, eventResponse.StatusCode);

        using var eventJson = JsonDocument.Parse(
            await eventResponse.Content.ReadAsStringAsync());

        var eventId = eventJson.RootElement
            .GetProperty("eventID").GetInt32();

        // Create a category for that event.
        UseToken(organiserToken);

        var categoryResponse = await _client.PostAsJsonAsync(
            "/api/Categories",
            new
            {
                categoryName = "Test Open Category",
                minimumAge = 18,
                maximumAge = 60,
                distance = 5m,
                eventID = eventId
            });

        Assert.True(
            categoryResponse.IsSuccessStatusCode,
            $"Category creation failed: {(int)categoryResponse.StatusCode} " +
            await categoryResponse.Content.ReadAsStringAsync());

        using var categoryJson = JsonDocument.Parse(
            await categoryResponse.Content.ReadAsStringAsync());

        var categoryId = categoryJson.RootElement
            .GetProperty("categoryID").GetInt32();

        // Enrol as a Participant.
        await RegisterAsync("enrol.participant@test.example");

        var participantToken =
            await LoginAsync("enrol.participant@test.example");

        UseToken(participantToken);

        var enrolResponse = await _client.PostAsJsonAsync(
            "/api/Enrolments",
            new
            {
                eventID = eventId,
                categoryID = categoryId
            });

        Assert.True(
        enrolResponse.StatusCode == HttpStatusCode.OK ||
        enrolResponse.StatusCode == HttpStatusCode.Created,
        $"Enrolment failed: {(int)enrolResponse.StatusCode} " +
        await enrolResponse.Content.ReadAsStringAsync());

        // Confirm that the enrolment was recorded.
        var myResponse = await _client.GetAsync(
            "/api/Enrolments/my");

        Assert.Equal(HttpStatusCode.OK, myResponse.StatusCode);

        using var myJson = JsonDocument.Parse(
            await myResponse.Content.ReadAsStringAsync());

        Assert.Single(myJson.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task Participant_DuplicateEnrolment_ReturnsConflict()
    {
        // Create event and category.
        await RegisterAsync(
            "duplicate.organiser@test.example", "Organiser");

        var organiserToken =
            await LoginAsync("duplicate.organiser@test.example");

        var eventResponse = await CreateEventAsync(organiserToken);

        using var eventJson = JsonDocument.Parse(
            await eventResponse.Content.ReadAsStringAsync());

        var eventId = eventJson.RootElement
            .GetProperty("eventID").GetInt32();

        UseToken(organiserToken);

        var categoryResponse = await _client.PostAsJsonAsync(
            "/api/Categories",
            new
            {
                categoryName = "Duplicate Test Category",
                minimumAge = 18,
                maximumAge = 60,
                distance = 5m,
                eventID = eventId
            });

        Assert.True(categoryResponse.IsSuccessStatusCode);

        using var categoryJson = JsonDocument.Parse(
            await categoryResponse.Content.ReadAsStringAsync());

        var categoryId = categoryJson.RootElement
            .GetProperty("categoryID").GetInt32();

        // Register a Participant.
        await RegisterAsync("duplicate.participant@test.example");

        var participantToken =
            await LoginAsync("duplicate.participant@test.example");

        UseToken(participantToken);

        var body = new
        {
            eventID = eventId,
            categoryID = categoryId
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/Enrolments", body);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/Enrolments", body);

        Assert.Equal(
            HttpStatusCode.Conflict, secondResponse.StatusCode);
    }
}

using System.Net;
using System.Text;
using System.Text.Json;

namespace UserManagementApiTests;

[TestFixture]
public class UserApiTests
{
    private HttpClient _httpClient;
    private const string BaseUrl = "https://jsonplaceholder.typicode.com/users";

    [SetUp]
    public void Setup() => _httpClient = new HttpClient();

    [TearDown]
    public void Cleanup() => _httpClient.Dispose();

    [Test]
    public async Task TestJsonPlaceholderBehavior()
    {
        // 1. Verify GET works
        var getResponse = await _httpClient.GetAsync($"{BaseUrl}/1");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var user = JsonSerializer.Deserialize<User>(
            await getResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.Multiple(() =>
        {
            Assert.That(user?.Id, Is.EqualTo(1));
            Assert.That(user?.Name, Is.Not.Null);
        });

        // 2. Verify POST returns 201 but doesn't actually create
        var newUser = new { name = "Test User", email = "test@example.com" };
        var postResponse = await _httpClient.PostAsync(
            BaseUrl,
            new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json"));
        
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var createdUser = JsonSerializer.Deserialize<User>(
            await postResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // JSONPlaceholder always returns ID=11 for new "created" users
        Assert.That(createdUser?.Id, Is.EqualTo(11));

        // 3. Verify DELETE returns 200 but doesn't actually delete
        var deleteResponse = await _httpClient.DeleteAsync($"{BaseUrl}/1");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // 4. Verify the "deleted" user still exists
        var verifyResponse = await _httpClient.GetAsync($"{BaseUrl}/1");
        Assert.That(verifyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
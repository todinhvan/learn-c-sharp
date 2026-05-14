using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System;

namespace MySession.IntegrationTests
{
    public class HomeControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HomeControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        // --- Happy Cases ---

        [Fact]
        public async Task Get_Index_ReturnsSuccessAndSetsCookie()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            
            // Check if cookie is set
            Assert.True(response.Headers.Contains("Set-Cookie"));
            var setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            Assert.Contains("MY_SESSION_ID=", setCookieHeader);
        }

        [Fact]
        public async Task SetCustomSession_And_GetCustomSession_WorksWithSameCookie()
        {
            // Arrange
            var client = _factory.CreateClient();
            var key = "testKey1";
            var value = "testValue1";

            // Act 1: Set session
            var setResponse = await client.PostAsync($"/Home/SetCustomSession?key={key}&value={value}", null);
            setResponse.EnsureSuccessStatusCode();

            // Extract cookie
            var setCookieHeader = setResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            var cookieValue = setCookieHeader.Split(';')[0]; // Gets 'MY_SESSION_ID=guid'

            // Act 2: Get session with the same cookie
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Home/GetCustomSession?key={key}");
            request.Headers.Add("Cookie", cookieValue);
            var getResponse = await client.SendAsync(request);

            // Assert
            getResponse.EnsureSuccessStatusCode();
            var responseString = await getResponse.Content.ReadAsStringAsync();
            Assert.Equal(value, responseString);
        }

        // --- Edge Cases ---

        [Fact]
        public async Task SetCustomSession_Overwrite_ExistingKey_DoesNotCrash()
        {
            // Arrange
            var client = _factory.CreateClient();
            var key = "overwriteKey";
            
            // Act 1: Set value 1
            var setResponse1 = await client.PostAsync($"/Home/SetCustomSession?key={key}&value=Value1", null);
            setResponse1.EnsureSuccessStatusCode();

            // Extract cookie
            var setCookieHeader = setResponse1.Headers.GetValues("Set-Cookie").FirstOrDefault();
            var cookieValue = setCookieHeader.Split(';')[0];

            // Act 2: Overwrite with value 2 (same key, same cookie)
            var request2 = new HttpRequestMessage(HttpMethod.Post, $"/Home/SetCustomSession?key={key}&value=Value2");
            request2.Headers.Add("Cookie", cookieValue);
            var setResponse2 = await client.SendAsync(request2);

            // Assert
            setResponse2.EnsureSuccessStatusCode(); // Should not crash with ArgumentException

            // Verify value is actually overwritten
            var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/Home/GetCustomSession?key={key}");
            getRequest.Headers.Add("Cookie", cookieValue);
            var getResponse = await client.SendAsync(getRequest);
            var responseString = await getResponse.Content.ReadAsStringAsync();
            
            Assert.Equal("Value2", responseString);
        }

        [Fact]
        public async Task GetCustomSession_FileNotExists_DoesNotCrash()
        {
            // Arrange
            var client = _factory.CreateClient();
            
            // Generate a random valid GUID to simulate a cookie, but no file has been committed
            var fakeSessionId = Guid.NewGuid().ToString();
            
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Home/GetCustomSession?key=someKey");
            request.Headers.Add("Cookie", $"MY_SESSION_ID={fakeSessionId}");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            // It should return 200 OK with empty body, NOT throw FileNotFoundException
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseString);
        }

        [Fact]
        public async Task GetCustomSession_NoCookie_DoesNotCrash_AndReturnsEmpty()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act: Request GetCustomSession without any cookie
            var response = await client.GetAsync("/Home/GetCustomSession?key=someKey");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseString);
            
            // Also check that a new cookie was assigned
            Assert.True(response.Headers.Contains("Set-Cookie"));
            var setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            Assert.Contains("MY_SESSION_ID=", setCookieHeader);
        }
    }
}

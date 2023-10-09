using APIintegrationBookShop.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text;

namespace APIintegrationBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankController : ControllerBase
    {
        HttpClient Client;
        public static string AuthToken;
        public BankController()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri("https://localhost:7157/");
        }

        [HttpPost("Banklogin")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                // Log received credentials
                Log.Information($"Received login request - Email: {email}, Password: {password}");
                var content = new StringContent($"email={email}&password={password}");
                HttpResponseMessage response = await Client.PostAsync("api/UserLogin/user-login", content);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadAsStringAsync();
                    AuthToken = tokenResponse; // Store the token as JSON string

                    return Ok(new { Token = tokenResponse }); // Return JSON response
                }
                else
                {
                    Log.Warning($"Login failed - Status code: {response.StatusCode}");
                    return Unauthorized("Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred during login: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}

using APIintegrationBookShop.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net.Http.Headers;
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
            var loginRequest = new { Email = email, Password = password };

            try
            {
                // Log received credentials
                Log.Information($"Received login request - Email: {email}, Password: {password}");

                HttpResponseMessage response = await Client.PostAsJsonAsync("api/UserLogin/user-login", loginRequest);

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
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw(int accountNumber, decimal withdrawalAmount)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", AuthToken);

            try
            {
                var withdrawRequest = new { accountNumber, withdrawalAmount };

                HttpResponseMessage response = await Client.PutAsJsonAsync("api/AccountOperation/withdraw", withdrawRequest);

                if (response.IsSuccessStatusCode)
                {
                    
                    return Ok("Withdrawal successful");
                }
                else
                {
                    // Handle failure
                    return BadRequest($"Withdrawal failed. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



    }
}

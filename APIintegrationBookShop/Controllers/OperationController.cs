using APIintegrationBookShop.models;
using APIintegrationBookShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;

namespace APIintegrationBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationController : ControllerBase
    {
        
        HttpClient Client;
        public static string AuthToken;
        public OperationController()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri("https://localhost:7166/");
        }






        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            Log.Information($"AuthToken value: {AuthToken}");
            // Check if the token is available
            if (string.IsNullOrEmpty(AuthToken))
            {
                return Unauthorized("Token is missing or invalid.");
            }

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", AuthToken);

            try
            {
                HttpResponseMessage resp = await Client.GetAsync("api/BookOperation/ViewAllBooks");

                if (resp.IsSuccessStatusCode)
                {
                    string responseContent = await resp.Content.ReadAsStringAsync();
                    List<Book> books = JsonConvert.DeserializeObject<List<Book>>(responseContent);
                    return Ok(books);
                }
                else
                {
                    return BadRequest($"Failed to retrieve books. Status code: {resp.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("login")]
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

        [HttpPost("BorrowBook")]
        public async Task<IActionResult> BorrowBook(int patronId, int bookId)
        {
            var borrowRequest = new BorrowRequest { PatronId = patronId, BookId = bookId };

            try
            {
                HttpResponseMessage response = await Client.PostAsJsonAsync("/api/PatronOperation/BorrowBook", borrowRequest);

                if (response.IsSuccessStatusCode)
                {
                    // Process successful response
                    return Ok("Book borrowed successfully");
                }
                else
                {
                    // Handle unsuccessful response
                    Log.Warning($"Borrowing book failed - Status code: {response.StatusCode}");
                    return BadRequest("Failed to borrow book");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred during book borrowing: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}

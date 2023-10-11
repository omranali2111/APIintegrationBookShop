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
        
       public static HttpClient BankClient;
       public static  HttpClient LibraryClient;
      //  static BankController BankMethods;
        public static string AuthToken;
        public static string AuthToken2;


        public OperationController()
        {
            BankClient = new HttpClient();
            LibraryClient = new HttpClient();

            BankClient.BaseAddress = new Uri("https://localhost:7157/");
            LibraryClient.BaseAddress = new Uri("https://localhost:7166/");

             //BankMethods = new BankController();
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

            LibraryClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", AuthToken);

            try
            {
                HttpResponseMessage resp = await LibraryClient.GetAsync("api/BookOperation/ViewAllBooks");

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

                HttpResponseMessage response = await LibraryClient.PostAsJsonAsync("api/UserLogin/user-login", loginRequest);

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
        public async Task<IActionResult> BorrowBook(BorrowRequest br)
        {
            LibraryClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", AuthToken);
            var borrowRequest = new BorrowRequest { PatronId = br.PatronId, BookId = br.BookId };
            var loginRequest = new { Email = br.BankEmail, Password = br.BankPassword };
            var withdrawRequest = new { br.accountNumber, br.withdrawalAmount };
            try
            {
              

                // Step 1: Log in to the bank
                HttpResponseMessage response2 = await BankClient.PostAsJsonAsync("api/UserLogin/user-login", loginRequest);
                var tokenResponse1 = await response2.Content.ReadAsStringAsync();
                AuthToken2 = tokenResponse1; // Store the token as a JSON string
                if (response2.IsSuccessStatusCode)
                {
                    BankClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", AuthToken2);
                    // Step 2: Perform a withdrawal

                    HttpResponseMessage response3 = await BankClient.PutAsJsonAsync("api/AccountOperation/withdraw", withdrawRequest);

                    if (response3.IsSuccessStatusCode)
                    {
                        Log.Information("Withdrawal successful");
                        // Step 3: Borrow a book
                        HttpResponseMessage  response = await LibraryClient.PostAsJsonAsync("api/PatronOperation/BorrowBook", borrowRequest);

                        if (response.IsSuccessStatusCode)
                        {


                            return Ok("Book borrowed and bank operations completed successfully");
                        }
                        else
                        {
                            // Handle unsuccessful book borrowing
                            Log.Warning($"Borrowing book failed - Status code: {response.StatusCode}");
                            return BadRequest("Failed to borrow book");
                        }
                    }
                    else
                    {
                        // Handle unsuccessful withdrawal
                        Log.Warning($"Withdrawal failed - Status code: {response3.StatusCode}");
                        return BadRequest("Failed to withdraw amount from the bank");
                    }
                }
                else
                {
                    // Handle unsuccessful bank login
                    Log.Warning($"Login failed - Status code: {response2.StatusCode}");
                    return BadRequest("Failed to log in to the bank");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Log.Error($"An error occurred during book borrowing: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}

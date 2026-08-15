using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MarwadiGheeSweetsWeb.Controllers;

[Route("otp")]
[ApiController]
public class OtpController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<OtpController> _logger;
    private static readonly HttpClient _http = new();

    public OtpController(IConfiguration config, ILogger<OtpController> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Verifies the MSG91 OTP widget token server-side.
    /// Called by the checkout page via fetch() after the customer completes OTP.
    /// Returns { "success": true } or { "success": false, "error": "..." }.
    /// </summary>
    [HttpPost("verify-token")]
    [IgnoreAntiforgeryToken]   // AJAX endpoint — CSRF not applicable here
    public async Task<IActionResult> VerifyToken([FromBody] OtpVerifyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Token))
            return BadRequest(new { success = false, error = "Token is required." });

        var authKey = _config["Msg91:AuthKey"];
        if (string.IsNullOrWhiteSpace(authKey))
        {
            _logger.LogError("Msg91:AuthKey is not configured.");
            return StatusCode(500, new { success = false, error = "OTP service not configured." });
        }

        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                access_token = request.Token
            });

            using var reqMsg = new HttpRequestMessage(HttpMethod.Post,
                "https://api.msg91.com/api/v5/widget/verifyToken");
            reqMsg.Headers.Add("authkey", authKey);
            reqMsg.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(reqMsg);

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("MSG91 verifyToken response: {Body}", body);

            using var doc = JsonDocument.Parse(body);
            var type = doc.RootElement.TryGetProperty("type", out var t) ? t.GetString() : null;

            if (type == "success")
                return Ok(new { success = true });

            return Ok(new { success = false, error = "OTP verification failed. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MSG91 verifyToken API.");
            return StatusCode(500, new { success = false, error = "OTP service unavailable." });
        }
    }
}

public record OtpVerifyRequest(string Token);

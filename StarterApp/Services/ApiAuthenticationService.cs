using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ApiAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private User? _currentUser;
    private readonly List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser != null && !IsTokenExpired();
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => _currentUserRoles;

    public ApiAuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private string? _jwtToken;
    private DateTime _tokenExpiresAt;

    private bool IsTokenExpired()
    {
        return string.IsNullOrEmpty(_jwtToken) || DateTime.UtcNow >= _tokenExpiresAt;
    }

    private void ClearAuthenticationState()
    {
        _currentUser = null;
        _currentUserRoles.Clear();
        _jwtToken = null;
        _tokenExpiresAt = DateTime.MinValue;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/token", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new AuthenticationResult(false, error?.Message ?? "Login failed");
            }

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();

            _jwtToken = token!.Token;
            _tokenExpiresAt = token.ExpiresAt;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _jwtToken);

            if (IsTokenExpired())
            {
                ClearAuthenticationState();
                AuthenticationStateChanged?.Invoke(this, false);
                return new AuthenticationResult(false, "Session expired. Please log in again.");
            }

            var meResponse = await _httpClient.GetAsync("users/me");
            var profile = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();

            _currentUser = new User
            {
                Id = profile!.Id,
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                CreatedAt = profile.CreatedAt,
                IsActive = true
            };

            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

        if (!response.IsSuccessStatusCode)
        {
            var rawError = await response.Content.ReadAsStringAsync();
            return new AuthenticationResult(
                false,
                $"Registration failed: {(int)response.StatusCode} {response.ReasonPhrase}. {rawError}"
            );
        }

            return new AuthenticationResult(true, "Registration successful. Please log in.");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

        public Task LogoutAsync()
        {
            ClearAuthenticationState();
            AuthenticationStateChanged?.Invoke(this, false);
            return Task.CompletedTask;
        }

    public bool HasRole(string roleName) =>
        _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    public bool HasAnyRole(params string[] roleNames) =>
        roleNames.Any(HasRole);

    public bool HasAllRoles(params string[] roleNames) =>
        roleNames.All(HasRole);

    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        // Not supported by the shared API
        return Task.FromResult(false);
    }

    // --- API response DTOs ---

    private record TokenResponse(string Token, DateTime ExpiresAt, int UserId);

    private record UserProfileResponse(
        int Id, string Email, string FirstName, string LastName, DateTime CreatedAt);

    private record ApiErrorResponse(string Error, string Message);
}
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ApiAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext; // lets the API auth service create/update the matching local database user
    private User? _currentUser;
    private readonly List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser != null && !IsTokenExpired();
    public User? CurrentUser => _currentUser;
    public int CurrentLocalUserId { get; private set; } // stores the ID of the synced local database user
    public List<string> CurrentUserRoles => _currentUserRoles;

    public ApiAuthenticationService(HttpClient httpClient, AppDbContext dbContext)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
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
        CurrentLocalUserId = 0;
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
                Id = profile!.Id, // this is still the shared API user ID
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                CreatedAt = profile.CreatedAt,
                IsActive = true
            };

            CurrentLocalUserId = await SyncLocalUserAsync(profile); // creates or updates a matching local DB user and stores its local ID

            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    private async Task<int> SyncLocalUserAsync(UserProfileResponse profile)
    {
        // first try to find the user by API user ID
        var localUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.ExternalApiUserId == profile.Id);

        // if not found, try matching by email
        if (localUser == null)
        {
            localUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == profile.Email);
        }

        if (localUser == null)
        {
            // create a new local user row if none exists
            localUser = new User
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Email = profile.Email,
                ExternalApiUserId = profile.Id,
                PasswordHash = "API_USER", // placeholder because local auth is not used for this synced API user
                PasswordSalt = "API_USER", // placeholder because local auth is not used for this synced API user
                CreatedAt = profile.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _dbContext.Users.Add(localUser);
        }
        else
        {
            // update local row if the API user already exists
            localUser.FirstName = profile.FirstName;
            localUser.LastName = profile.LastName;
            localUser.Email = profile.Email;
            localUser.ExternalApiUserId = profile.Id;
            localUser.UpdatedAt = DateTime.UtcNow;
            localUser.IsActive = true;
        }

        await _dbContext.SaveChangesAsync();
        return localUser.Id; // return the LOCAL database user ID, not the API user ID
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

    private record TokenResponse(string Token, DateTime ExpiresAt, int UserId);

    private record UserProfileResponse(
        int Id, string Email, string FirstName, string LastName, DateTime CreatedAt);

    private record ApiErrorResponse(string Error, string Message);
}
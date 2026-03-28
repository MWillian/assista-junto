using Microsoft.JSInterop;

namespace AssistaJunto.Client.Services;

public class AuthStateService
{
    public const int MaxUsernameLength = 50;

    private readonly IJSRuntime _js;
    private string? _username;

    public event Action? OnAuthStateChanged;

    public bool IsAuthenticated => _username is not null;
    public string? Username => _username;

    public AuthStateService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        _username = await _js.InvokeAsync<string?>("localStorage.getItem", "user_name");

        if (!string.IsNullOrWhiteSpace(_username))
            _username = _username.Trim();

        if (!string.IsNullOrWhiteSpace(_username) && _username.Length > MaxUsernameLength)
        {
            _username = null;
            await _js.InvokeVoidAsync("localStorage.removeItem", "user_name");
            OnAuthStateChanged?.Invoke();
        }
    }

    public async Task SetUsernameAsync(string username)
    {
        var sanitized = username?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(sanitized))
            throw new InvalidOperationException("Informe um nome de usuário válido.");

        if (sanitized.Length > MaxUsernameLength)
            throw new InvalidOperationException($"Nome com no máximo {MaxUsernameLength} caracteres.");

        _username = sanitized;
        await _js.InvokeVoidAsync("localStorage.setItem", "user_name", sanitized);
        OnAuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        _username = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "user_name");
        OnAuthStateChanged?.Invoke();
    }
}

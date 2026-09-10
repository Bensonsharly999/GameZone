using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GameZone.Application.DTOs.Auth;
using GameZone.Application.DTOs.Clients;
using GameZone.Application.DTOs.Dashboard;
using GameZone.Application.DTOs.GamingItems;
using GameZone.Application.DTOs.Payments;
using GameZone.Application.DTOs.Reports;
using GameZone.Application.DTOs.Sessions;
using GameZone.Application.DTOs.Users;

namespace GameZone.Maui.Services;

public class GameZoneApi
{
    private readonly AppSettings _settings;
    private readonly SessionState _session;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GameZoneApi(AppSettings settings, SessionState session)
    {
        _settings = settings;
        _session = session;
    }

    public Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        => PostAnonymousAsync<LoginRequest, LoginResponse>("api/auth/login", new LoginRequest
        {
            Username = username,
            Password = password
        }, cancellationToken);

    public Task<DashboardStatsDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        => GetAsync<DashboardStatsDto>("api/dashboard", cancellationToken);

    public Task<List<ClientDto>> GetClientsAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(search)
            ? "api/clients"
            : $"api/clients?search={Uri.EscapeDataString(search)}";
        return GetAsync<List<ClientDto>>(path, cancellationToken);
    }

    public Task<ClientDto> CreateClientAsync(ClientEditorDto dto, CancellationToken cancellationToken = default)
        => PostAsync<ClientEditorDto, ClientDto>("api/clients", dto, cancellationToken);

    public Task<ClientDto> UpdateClientAsync(int id, ClientEditorDto dto, CancellationToken cancellationToken = default)
        => PutAsync<ClientEditorDto, ClientDto>($"api/clients/{id}", dto, cancellationToken);

    public Task<ClientHistoryDto> GetClientHistoryAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<ClientHistoryDto>($"api/clients/{id}/history", cancellationToken);

    public Task<List<GamingItemDto>> GetItemsAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<GamingItemDto>>("api/items", cancellationToken);

    public Task<List<GamingItemDto>> GetActiveItemsAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<GamingItemDto>>("api/items/active", cancellationToken);

    public Task<GamingItemDto> CreateItemAsync(GamingItemDto dto, CancellationToken cancellationToken = default)
        => PostAsync<GamingItemDto, GamingItemDto>("api/items", dto, cancellationToken);

    public Task SetItemActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        => PostAsync($"{(isActive ? $"api/items/{id}/activate" : $"api/items/{id}/deactivate")}", cancellationToken);

    public Task<List<SessionDto>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<SessionDto>>("api/sessions/active", cancellationToken);

    public Task<List<SessionDto>> GetTodaysSessionsAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<SessionDto>>("api/sessions/today", cancellationToken);

    public Task<SessionDto> StartSessionAsync(StartSessionRequest request, CancellationToken cancellationToken = default)
        => PostAsync<StartSessionRequest, SessionDto>("api/sessions", request, cancellationToken);

    public Task<EndSessionResult> EndSessionAsync(int id, CancellationToken cancellationToken = default)
        => PostAsync<object, EndSessionResult>($"api/sessions/{id}/end", new { }, cancellationToken);

    public Task<List<PaymentDto>> GetPaymentsAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<PaymentDto>>("api/payments", cancellationToken);

    public Task<List<PaymentDto>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<PaymentDto>>("api/payments/pending", cancellationToken);

    public Task<PaymentDto> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
        => PostAsync<RecordPaymentRequest, PaymentDto>("api/payments", request, cancellationToken);

    public Task<PaymentDto> MarkPaidAsync(int id, RecordPaymentRequest request, CancellationToken cancellationToken = default)
        => PostAsync<RecordPaymentRequest, PaymentDto>($"api/payments/{id}/paid", request, cancellationToken);

    public Task<List<DailyRevenueDto>> GetDailyRevenueAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<List<DailyRevenueDto>>($"api/reports/daily?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", cancellationToken);

    public Task<List<RevenueByItemDto>> GetRevenueByItemAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<RevenueByItemDto>>("api/reports/items", cancellationToken);

    public Task<List<TopClientDto>> GetTopClientsAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<TopClientDto>>("api/reports/top-clients", cancellationToken);

    public Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<UserDto>>("api/users", cancellationToken);

    public Task<UserDto> CreateUserAsync(UserEditorDto dto, CancellationToken cancellationToken = default)
        => PostAsync<UserEditorDto, UserDto>("api/users", dto, cancellationToken);

    public Task<UserDto> UpdateUserAsync(int id, UserEditorDto dto, CancellationToken cancellationToken = default)
        => PutAsync<UserEditorDto, UserDto>($"api/users/{id}", dto, cancellationToken);

    public Task SetUserActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        => PostAsync(isActive ? $"api/users/{id}/activate" : $"api/users/{id}/deactivate", cancellationToken);

    public Task ResetPasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default)
        => PostWithoutResultAsync($"api/users/{id}/reset-password", new { newPassword }, cancellationToken);

    public Task<GamingItemDto> UpdateItemAsync(int id, GamingItemDto dto, CancellationToken cancellationToken = default)
        => PutAsync<GamingItemDto, GamingItemDto>($"api/items/{id}", dto, cancellationToken);

    public Task DeleteItemAsync(int id, CancellationToken cancellationToken = default)
        => PostAsync($"api/items/{id}/delete", cancellationToken);

    public Task DeleteClientAsync(int id, CancellationToken cancellationToken = default)
        => PostAsync($"api/clients/{id}/delete", cancellationToken);

    private HttpClient CreateClient(bool anonymous = false)
    {
        var client = new HttpClient { BaseAddress = new Uri(_settings.ApiBaseUrl + "/"), Timeout = TimeSpan.FromSeconds(30) };
        if (!anonymous && !string.IsNullOrWhiteSpace(_session.Token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);
        return client;
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var response = await client.GetAsync(path, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task<TResponse> PostAnonymousAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken)
    {
        using var client = CreateClient(true);
        using var response = await client.PostAsJsonAsync(path, body, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var response = await client.PostAsJsonAsync(path, body, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private async Task PostAsync(string path, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var response = await client.PostAsync(path, null, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    private async Task PostWithoutResultAsync<TRequest>(string path, TRequest body, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var response = await client.PostAsJsonAsync(path, body, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    private async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var response = await client.DeleteAsync(path, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    private async Task<TResponse> PutAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var response = await client.PutAsJsonAsync(path, body, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccess(response, cancellationToken);
        var value = await response.Content.ReadFromJsonAsync<T>(_json, cancellationToken);
        return value ?? throw new ApiException("Empty response from server.");
    }

    private async Task EnsureSuccess(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var error = JsonSerializer.Deserialize<ApiError>(text, _json);
            throw new ApiException(error?.Error ?? $"Request failed ({(int)response.StatusCode}).");
        }
        catch (ApiException)
        {
            throw;
        }
        catch
        {
            throw new ApiException(string.IsNullOrWhiteSpace(text)
                ? $"Request failed ({(int)response.StatusCode}). Check API URL and internet."
                : text);
        }
    }
}

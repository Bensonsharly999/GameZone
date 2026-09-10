using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Users;
using GameZone.Domain.Enums;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

public partial class UsersViewModel : ObservableObject
{
    private readonly GameZoneApi _api;
    private readonly SessionState _session;

    public UsersViewModel(GameZoneApi api, SessionState session)
    {
        _api = api;
        _session = session;
    }

    public ObservableCollection<UserDto> Users { get; } = new();
    public IReadOnlyList<UserRole> Roles { get; } = Enum.GetValues<UserRole>();
    public bool IsAdmin => _session.IsAdmin;

    [ObservableProperty] private int? _editingId;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private UserRole _role = UserRole.Employee;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private string? _status;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        try
        {
            var list = await _api.GetUsersAsync();
            Users.Clear();
            foreach (var user in list)
                Users.Add(user);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private void Edit(UserDto? user)
    {
        if (user is null) return;
        EditingId = user.Id;
        Name = user.Name;
        Username = user.Username;
        Role = user.Role;
        Password = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        Error = null;
        Status = null;
        try
        {
            var dto = new UserEditorDto
            {
                Id = EditingId,
                Name = Name,
                Username = Username,
                Password = Password,
                Role = Role
            };
            if (EditingId is null)
                await _api.CreateUserAsync(dto);
            else
                await _api.UpdateUserAsync(EditingId.Value, dto);
            EditingId = null;
            Name = Username = Password = string.Empty;
            Role = UserRole.Employee;
            Status = "User saved.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ToggleAsync(UserDto? user)
    {
        if (user is null) return;
        Error = null;
        try
        {
            await _api.SetUserActiveAsync(user.Id, !user.IsActive);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ResetAsync(UserDto? user)
    {
        if (user is null) return;
        Error = null;
        Status = null;
        try
        {
            var password = await Application.Current!.MainPage!.DisplayPromptAsync("Reset password", $"New password for {user.Username}", "Save", "Cancel", maxLength: 40);
            if (string.IsNullOrWhiteSpace(password))
                return;
            await _api.ResetPasswordAsync(user.Id, password);
            Status = "Password updated.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}

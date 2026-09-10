using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.GamingItems;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

public partial class ItemsViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public ItemsViewModel(GameZoneApi api)
    {
        _api = api;
    }

    public ObservableCollection<GamingItemDto> Items { get; } = new();

    [ObservableProperty] private int? _editingId;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _rateText = string.Empty;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = await _api.GetItemsAsync();
            Items.Clear();
            foreach (var item in list)
                Items.Add(item);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Edit(GamingItemDto? item)
    {
        if (item is null) return;
        EditingId = item.Id;
        Name = item.Name;
        RateText = item.RatePerHour.ToString("0.##");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!decimal.TryParse(RateText, out var rate))
        {
            Error = "Enter a valid hourly rate.";
            return;
        }

        Error = null;
        try
        {
            var dto = new GamingItemDto { Id = EditingId ?? 0, Name = Name, RatePerHour = rate, IsActive = true };
            if (EditingId is null)
                await _api.CreateItemAsync(dto);
            else
                await _api.UpdateItemAsync(EditingId.Value, dto);
            EditingId = null;
            Name = string.Empty;
            RateText = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(GamingItemDto? item)
    {
        if (item is null) return;
        try
        {
            await _api.DeleteItemAsync(item.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ToggleAsync(GamingItemDto? item)
    {
        if (item is null)
            return;
        try
        {
            await _api.SetItemActiveAsync(item.Id, !item.IsActive);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}

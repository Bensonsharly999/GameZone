using GameZone.Application.Common;
using GameZone.Application.DTOs.GamingItems;
using GameZone.Application.Interfaces;
using GameZone.Application.Mapping;
using GameZone.Domain.Entities;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;

namespace GameZone.Application.Services;

public class GamingItemService : IGamingItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public GamingItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<GamingItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.GamingItems.GetAllAsync(cancellationToken);
        return items.OrderBy(i => i.Name).Select(i => i.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<GamingItemDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.GamingItems.GetActiveAsync(cancellationToken);
        return items.OrderBy(i => i.Name).Select(i => i.ToDto()).ToList();
    }

    public async Task<Result<GamingItemDto>> CreateAsync(GamingItemDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto);
        if (validation is not null)
            return Result<GamingItemDto>.Failure(validation);

        if (await _unitOfWork.GamingItems.NameExistsAsync(dto.Name.Trim(), null, cancellationToken))
            return Result<GamingItemDto>.Failure("A gaming item with this name already exists.");

        var item = new GamingItem
        {
            Name = dto.Name.Trim(),
            IsActive = true
        };
        ApplyRates(item, dto);

        await _unitOfWork.GamingItems.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<GamingItemDto>.Success(item.ToDto());
    }

    public async Task<Result<GamingItemDto>> UpdateAsync(GamingItemDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto);
        if (validation is not null)
            return Result<GamingItemDto>.Failure(validation);

        var item = await _unitOfWork.GamingItems.GetByIdAsync(dto.Id, cancellationToken);
        if (item is null)
            return Result<GamingItemDto>.Failure("Gaming item not found.");

        if (await _unitOfWork.GamingItems.NameExistsAsync(dto.Name.Trim(), item.Id, cancellationToken))
            return Result<GamingItemDto>.Failure("A gaming item with this name already exists.");

        item.Name = dto.Name.Trim();
        ApplyRates(item, dto);
        _unitOfWork.GamingItems.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<GamingItemDto>.Success(item.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.GamingItems.GetByIdAsync(id, cancellationToken);
        if (item is null)
            return Result.Failure("Gaming item not found.");

        var related = await _unitOfWork.Sessions.FindAsync(s => s.GamingItemId == id, cancellationToken);
        if (related.Any(s => s.SessionStatus == SessionStatus.Active))
            return Result.Failure("This item has an active session. End that session first, then delete.");

        foreach (var session in related)
        {
            var tracked = await _unitOfWork.Sessions.GetWithDetailsAsync(session.Id, cancellationToken);
            if (tracked is null)
                continue;

            foreach (var payment in tracked.Payments.ToList())
                _unitOfWork.Payments.Remove(payment);

            _unitOfWork.Sessions.Remove(tracked);
        }

        _unitOfWork.GamingItems.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.GamingItems.GetByIdAsync(id, cancellationToken);
        if (item is null)
            return Result.Failure("Gaming item not found.");

        item.IsActive = isActive;
        _unitOfWork.GamingItems.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static string? Validate(GamingItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return "Item name is required.";
        if (dto.Rate30MinOnePlayer <= 0 || dto.Rate30MinTwoPlayers <= 0
            || dto.RateOneHourOnePlayer <= 0 || dto.RateOneHourTwoPlayers <= 0)
            return "Enter all four prices: 30 min and 1 hour for 1 player and 2 players.";
        return null;
    }

    private static void ApplyRates(GamingItem item, GamingItemDto dto)
    {
        item.Rate30MinOnePlayer = dto.Rate30MinOnePlayer;
        item.Rate30MinTwoPlayers = dto.Rate30MinTwoPlayers;
        item.RateOneHourOnePlayer = dto.RateOneHourOnePlayer;
        item.RateOneHourTwoPlayers = dto.RateOneHourTwoPlayers;
        item.RatePerHour = dto.RateOneHourOnePlayer;
    }
}

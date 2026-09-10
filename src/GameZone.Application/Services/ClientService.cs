using GameZone.Application.Common;
using GameZone.Application.DTOs.Clients;
using GameZone.Application.Interfaces;
using GameZone.Application.Mapping;
using GameZone.Domain.Entities;
using GameZone.Domain.Interfaces;

namespace GameZone.Application.Services;

public class ClientService : IClientService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _unitOfWork.Clients.SearchWithSessionsAsync(null, cancellationToken);
        return clients.OrderByDescending(c => c.CreatedDate).Select(c => c.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ClientDto>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var clients = await _unitOfWork.Clients.SearchWithSessionsAsync(term, cancellationToken);
        return clients.Select(c => c.ToDto()).ToList();
    }

    public async Task<Result<ClientDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.Clients.GetWithSessionsAsync(id, cancellationToken);
        return client is null
            ? Result<ClientDto>.Failure("Client not found.")
            : Result<ClientDto>.Success(client.ToDto());
    }

    public async Task<Result<ClientDto>> CreateAsync(ClientEditorDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto);
        if (validation is not null)
            return Result<ClientDto>.Failure(validation);

        var phone = NormalizePhone(dto.PhoneNumber);
        if (await _unitOfWork.Clients.PhoneExistsAsync(phone, null, cancellationToken))
            return Result<ClientDto>.Failure("A client with this phone number already exists.");

        var client = new Client
        {
            Name = dto.Name.Trim(),
            PhoneNumber = phone,
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            CreatedDate = DateTime.Now
        };

        await _unitOfWork.Clients.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ClientDto>.Success(client.ToDto());
    }

    public async Task<Result<ClientDto>> UpdateAsync(ClientEditorDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Id is null)
            return Result<ClientDto>.Failure("Client id is required.");

        var validation = Validate(dto);
        if (validation is not null)
            return Result<ClientDto>.Failure(validation);

        var client = await _unitOfWork.Clients.GetByIdAsync(dto.Id.Value, cancellationToken);
        if (client is null)
            return Result<ClientDto>.Failure("Client not found.");

        var phone = NormalizePhone(dto.PhoneNumber);
        if (await _unitOfWork.Clients.PhoneExistsAsync(phone, client.Id, cancellationToken))
            return Result<ClientDto>.Failure("A client with this phone number already exists.");

        client.Name = dto.Name.Trim();
        client.PhoneNumber = phone;
        client.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();

        _unitOfWork.Clients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var withSessions = await _unitOfWork.Clients.GetWithSessionsAsync(client.Id, cancellationToken);
        return Result<ClientDto>.Success((withSessions ?? client).ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.Clients.GetWithSessionsAsync(id, cancellationToken);
        if (client is null)
            return Result.Failure("Client not found.");

        if (client.Sessions.Count > 0)
            return Result.Failure("Cannot delete a client who has session history. Keep the record for reporting.");

        _unitOfWork.Clients.Remove(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<ClientHistoryDto>> GetHistoryAsync(int clientId, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.Clients.GetWithSessionsAsync(clientId, cancellationToken);
        if (client is null)
            return Result<ClientHistoryDto>.Failure("Client not found.");

        var visits = client.Sessions
            .OrderByDescending(s => s.EntryTime)
            .Select(s =>
            {
                var payment = s.Payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();
                return new ClientHistoryItemDto
                {
                    VisitDate = s.EntryTime.Date,
                    GamingItem = s.GamingItem?.Name ?? "-",
                    EntryTime = s.EntryTime,
                    ExitTime = s.ExitTime,
                    Duration = s.DurationMinutes is null ? "-" : SessionDtoDuration(s.DurationMinutes.Value),
                    Amount = s.Amount,
                    PaymentMethod = payment is null ? "-" : DtoMapper.FormatMethod(payment.PaymentMethod),
                    PaymentStatus = payment?.PaymentStatus.ToString() ?? "-"
                };
            })
            .ToList();

        return Result<ClientHistoryDto>.Success(new ClientHistoryDto
        {
            Client = client.ToDto(),
            Visits = visits
        });
    }

    private static string? Validate(ClientEditorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return "Name is required.";
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return "Phone number is required.";

        var digits = new string(dto.PhoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 8)
            return "Enter a valid phone number.";

        return null;
    }

    private static string NormalizePhone(string phone) =>
        new(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

    private static string SessionDtoDuration(int minutes) =>
        DTOs.Sessions.SessionDto.FormatDuration(minutes);
}

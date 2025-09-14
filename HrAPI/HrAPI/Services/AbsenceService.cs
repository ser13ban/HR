using HrAPI.Data;
using HrAPI.DTOs;
using HrAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HrAPI.Services;

public class AbsenceService : IAbsenceService
{
    private readonly HrDbContext _context;

    public AbsenceService(HrDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AbsenceRequestDto>> GetMyAbsenceRequestsAsync(int employeeId)
    {
        var absenceRequests = await _context.AbsenceRequests
            .Include(ar => ar.Employee)
            .Include(ar => ar.ApprovedBy)
            .Where(ar => ar.EmployeeId == employeeId)
            .OrderByDescending(ar => ar.CreatedAt)
            .ToListAsync();

        return absenceRequests.Select(ar => MapToDto(ar));
    }

    public async Task<IEnumerable<AbsenceRequestDto>> GetEmployeeAbsenceRequestsAsync(int employeeId, int requestingEmployeeId)
    {
        // Check if requesting employee is a manager
        var isManager = await IsEmployeeManagerAsync(requestingEmployeeId);
        if (!isManager)
        {
            throw new UnauthorizedAccessException("Only managers can view other employees' absence requests.");
        }

        var absenceRequests = await _context.AbsenceRequests
            .Include(ar => ar.Employee)
            .Include(ar => ar.ApprovedBy)
            .Where(ar => ar.EmployeeId == employeeId)
            .OrderByDescending(ar => ar.CreatedAt)
            .ToListAsync();

        return absenceRequests.Select(ar => MapToDto(ar));
    }

    public async Task<IEnumerable<AbsenceRequestDto>> GetApprovedAbsenceRequestsAsync()
    {
        var absenceRequests = await _context.AbsenceRequests
            .Include(ar => ar.Employee)
            .Include(ar => ar.ApprovedBy)
            .Where(ar => ar.Status == AbsenceStatus.Approved)
            .OrderByDescending(ar => ar.StartDate)
            .ToListAsync();

        return absenceRequests.Select(ar => MapToDto(ar, excludeSensitiveInfo: true));
    }

    public async Task<IEnumerable<AbsenceRequestDto>> GetPendingApprovalsForManagerAsync(int managerId)
    {
        // Check if requesting employee is a manager
        var isManager = await IsEmployeeManagerAsync(managerId);
        if (!isManager)
        {
            throw new UnauthorizedAccessException("Only managers can view pending approvals.");
        }

        var absenceRequests = await _context.AbsenceRequests
            .Include(ar => ar.Employee)
            .Include(ar => ar.ApprovedBy)
            .Where(ar => ar.Status == AbsenceStatus.Pending && ar.EmployeeId != managerId) // Exclude manager's own requests
            .OrderByDescending(ar => ar.CreatedAt)
            .ToListAsync();

        return absenceRequests.Select(ar => MapToDto(ar));
    }

    public async Task<AbsenceRequestDto> CreateAbsenceRequestAsync(int employeeId, CreateAbsenceRequestDto createDto)
    {
        // Validate dates
        if (createDto.StartDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Start date cannot be in the past.");
        }

        if (createDto.EndDate < createDto.StartDate)
        {
            throw new ArgumentException("End date must be after start date.");
        }

        // Check for overlapping requests
        var hasOverlappingRequest = await _context.AbsenceRequests
            .AnyAsync(ar => ar.EmployeeId == employeeId &&
                           ar.Status != AbsenceStatus.Rejected &&
                           ar.Status != AbsenceStatus.Cancelled &&
                           ((ar.StartDate <= createDto.StartDate && ar.EndDate >= createDto.StartDate) ||
                            (ar.StartDate <= createDto.EndDate && ar.EndDate >= createDto.EndDate) ||
                            (ar.StartDate >= createDto.StartDate && ar.EndDate <= createDto.EndDate)));

        if (hasOverlappingRequest)
        {
            throw new ArgumentException("You already have an absence request for the selected dates.");
        }

        var absenceRequest = new AbsenceRequest
        {
            EmployeeId = employeeId,
            Type = createDto.Type,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            Reason = createDto.Reason,
            Status = AbsenceStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AbsenceRequests.Add(absenceRequest);
        await _context.SaveChangesAsync();

        // Load the created request with navigation properties
        var createdRequest = await _context.AbsenceRequests
            .Include(ar => ar.Employee)
            .Include(ar => ar.ApprovedBy)
            .FirstAsync(ar => ar.Id == absenceRequest.Id);

        return MapToDto(createdRequest);
    }

    public async Task<AbsenceRequestDto> ApproveAbsenceRequestAsync(int requestId, int approverId, ApprovalActionDto approvalDto)
    {
        var isManager = await IsEmployeeManagerAsync(approverId);
        if (!isManager)
        {
            throw new UnauthorizedAccessException("Only managers can approve absence requests.");
        }

        var absenceRequest = await _context.AbsenceRequests
            .Include(ar => ar.Employee)
            .Include(ar => ar.ApprovedBy)
            .FirstOrDefaultAsync(ar => ar.Id == requestId);

        if (absenceRequest == null)
        {
            throw new ArgumentException("Absence request not found.");
        }

        if (absenceRequest.Status != AbsenceStatus.Pending)
        {
            throw new ArgumentException("Only pending requests can be approved.");
        }

        absenceRequest.Status = AbsenceStatus.Approved;
        absenceRequest.ApprovedById = approverId;
        absenceRequest.ApprovedAt = DateTime.UtcNow;
        absenceRequest.ApprovalNotes = approvalDto.ApproverNotes;
        absenceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload to get updated navigation properties
        await _context.Entry(absenceRequest)
            .Reference(ar => ar.ApprovedBy)
            .LoadAsync();

        return MapToDto(absenceRequest);
    }

    public async Task<AbsenceRequestDto> DeclineAbsenceRequestAsync(int requestId, int approverId, ApprovalActionDto approvalDto)
    {
        var isManager = await IsEmployeeManagerAsync(approverId);
        if (!isManager)
        {
            throw new UnauthorizedAccessException("Only managers can decline absence requests.");
        }

        var absenceRequest = await _context.AbsenceRequests
            .Include(ar => ar.Employee)
            .Include(ar => ar.ApprovedBy)
            .FirstOrDefaultAsync(ar => ar.Id == requestId);

        if (absenceRequest == null)
        {
            throw new ArgumentException("Absence request not found.");
        }

        if (absenceRequest.Status != AbsenceStatus.Pending)
        {
            throw new ArgumentException("Only pending requests can be declined.");
        }

        absenceRequest.Status = AbsenceStatus.Rejected;
        absenceRequest.ApprovedById = approverId;
        absenceRequest.ApprovedAt = DateTime.UtcNow;
        absenceRequest.ApprovalNotes = approvalDto.ApproverNotes;
        absenceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload to get updated navigation properties
        await _context.Entry(absenceRequest)
            .Reference(ar => ar.ApprovedBy)
            .LoadAsync();

        return MapToDto(absenceRequest);
    }

    public async Task<bool> CancelAbsenceRequestAsync(int requestId, int employeeId)
    {
        var absenceRequest = await _context.AbsenceRequests
            .FirstOrDefaultAsync(ar => ar.Id == requestId && ar.EmployeeId == employeeId);

        if (absenceRequest == null)
        {
            return false;
        }

        if (absenceRequest.Status != AbsenceStatus.Pending)
        {
            throw new ArgumentException("Only pending requests can be cancelled.");
        }

        absenceRequest.Status = AbsenceStatus.Cancelled;
        absenceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsEmployeeManagerAsync(int employeeId)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        return employee?.Role == EmployeeRole.Manager;
    }

    private static AbsenceRequestDto MapToDto(AbsenceRequest absenceRequest, bool excludeSensitiveInfo = false)
    {
        return new AbsenceRequestDto
        {
            Id = absenceRequest.Id,
            EmployeeId = absenceRequest.EmployeeId,
            EmployeeName = absenceRequest.Employee.FullName,
            EmployeeEmail = absenceRequest.Employee.Email ?? string.Empty,
            Type = absenceRequest.Type,
            StartDate = absenceRequest.StartDate,
            EndDate = absenceRequest.EndDate,
            Reason = excludeSensitiveInfo ? null : absenceRequest.Reason,
            Status = absenceRequest.Status,
            CreatedAt = absenceRequest.CreatedAt,
            UpdatedAt = absenceRequest.UpdatedAt,
            ApprovalNotes = absenceRequest.ApprovalNotes,
            ApprovedById = absenceRequest.ApprovedById,
            ApprovedByName = absenceRequest.ApprovedBy?.FullName,
            ApprovedAt = absenceRequest.ApprovedAt,
            DurationInDays = absenceRequest.DurationInDays
        };
    }
}

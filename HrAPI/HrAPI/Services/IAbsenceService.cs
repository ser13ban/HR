using HrAPI.DTOs;
using HrAPI.Models;

namespace HrAPI.Services;

public interface IAbsenceService
{
    Task<IEnumerable<AbsenceRequestDto>> GetMyAbsenceRequestsAsync(int employeeId);
    Task<IEnumerable<AbsenceRequestDto>> GetEmployeeAbsenceRequestsAsync(int employeeId, int requestingEmployeeId);
    Task<IEnumerable<AbsenceRequestDto>> GetApprovedAbsenceRequestsAsync();
    Task<IEnumerable<AbsenceRequestDto>> GetPendingApprovalsForManagerAsync(int managerId);
    Task<AbsenceRequestDto> CreateAbsenceRequestAsync(int employeeId, CreateAbsenceRequestDto createDto);
    Task<AbsenceRequestDto> ApproveAbsenceRequestAsync(int requestId, int approverId, ApprovalActionDto approvalDto);
    Task<AbsenceRequestDto> DeclineAbsenceRequestAsync(int requestId, int approverId, ApprovalActionDto approvalDto);
    Task<bool> CancelAbsenceRequestAsync(int requestId, int employeeId);
    Task<bool> IsEmployeeManagerAsync(int employeeId);
}

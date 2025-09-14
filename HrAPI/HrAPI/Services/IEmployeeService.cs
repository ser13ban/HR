using HrAPI.DTOs;

namespace HrAPI.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeListDto>> GetAllEmployeesAsync();
    Task<EmployeeProfileDto?> GetEmployeeByIdAsync(string id, int requestingUserId);
    Task<EmployeeProfileDto?> UpdateEmployeeAsync(string id, UpdateEmployeeDto updateDto, int requestingUserId);
}

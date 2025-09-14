namespace HrAPI.Services;

public interface IAuthorizationService
{
    // Manager-specific permissions
    Task<bool> IsManagerAsync(int userId);
    Task RequireManagerAsync(int userId);

    // Employee profile access permissions  
    Task<bool> CanViewEmployeeProfileAsync(int requestingUserId, int targetEmployeeId);
    Task<bool> CanEditEmployeeProfileAsync(int requestingUserId, int targetEmployeeId);
    Task RequireEditEmployeeProfileAsync(int requestingUserId, int targetEmployeeId);

    // Absence request permissions
    Task<bool> CanViewAbsenceRequestsAsync(int requestingUserId, int targetEmployeeId);
    Task<bool> CanApproveAbsenceRequestsAsync(int userId);
    Task RequireAbsenceApprovalPermissionAsync(int userId);

    // Feedback permissions
    Task<bool> CanViewFeedbackAsync(int requestingUserId, int targetEmployeeId);
    Task<bool> CanGiveFeedbackAsync(int fromUserId, int toUserId);
    Task RequireFeedbackViewPermissionAsync(int requestingUserId, int targetEmployeeId);
}

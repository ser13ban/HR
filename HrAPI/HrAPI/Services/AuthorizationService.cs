using Microsoft.EntityFrameworkCore;
using HrAPI.Data;
using HrAPI.Models;

namespace HrAPI.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly HrDbContext _context;

    public AuthorizationService(HrDbContext context)
    {
        _context = context;
    }

    // Manager-specific permissions
    public async Task<bool> IsManagerAsync(int userId)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == userId);

        return employee?.Role == EmployeeRole.Manager;
    }

    public async Task RequireManagerAsync(int userId)
    {
        var isManager = await IsManagerAsync(userId);
        if (!isManager)
        {
            throw new UnauthorizedAccessException("Only managers can perform this action.");
        }
    }

    // Employee profile access permissions  
    public async Task<bool> CanViewEmployeeProfileAsync(int requestingUserId, int targetEmployeeId)
    {
        // Users can view their own profile
        if (requestingUserId == targetEmployeeId)
        {
            return true;
        }

        // Managers can view any profile
        return await IsManagerAsync(requestingUserId);
    }

    public async Task<bool> CanEditEmployeeProfileAsync(int requestingUserId, int targetEmployeeId)
    {
        // Users can edit their own profile
        if (requestingUserId == targetEmployeeId)
        {
            return true;
        }

        // Managers can edit any profile
        return await IsManagerAsync(requestingUserId);
    }

    public async Task RequireEditEmployeeProfileAsync(int requestingUserId, int targetEmployeeId)
    {
        var canEdit = await CanEditEmployeeProfileAsync(requestingUserId, targetEmployeeId);
        if (!canEdit)
        {
            throw new UnauthorizedAccessException("You don't have permission to edit this profile.");
        }
    }

    // Absence request permissions
    public async Task<bool> CanViewAbsenceRequestsAsync(int requestingUserId, int targetEmployeeId)
    {
        // Users can view their own absence requests
        if (requestingUserId == targetEmployeeId)
        {
            return true;
        }

        // Managers can view any employee's absence requests
        return await IsManagerAsync(requestingUserId);
    }

    public async Task<bool> CanApproveAbsenceRequestsAsync(int userId)
    {
        return await IsManagerAsync(userId);
    }

    public async Task RequireAbsenceApprovalPermissionAsync(int userId)
    {
        var canApprove = await CanApproveAbsenceRequestsAsync(userId);
        if (!canApprove)
        {
            throw new UnauthorizedAccessException("Only managers can approve or decline absence requests.");
        }
    }

    // Feedback permissions
    public async Task<bool> CanViewFeedbackAsync(int requestingUserId, int targetEmployeeId)
    {
        // Users can view their own received feedback
        if (requestingUserId == targetEmployeeId)
        {
            return true;
        }

        // Managers can view any employee's feedback
        return await IsManagerAsync(requestingUserId);
    }

    public async Task<bool> CanGiveFeedbackAsync(int fromUserId, int toUserId)
    {
        // Cannot give feedback to self
        if (fromUserId == toUserId)
        {
            return false;
        }

        // Verify both employees exist
        var fromEmployee = await _context.Employees.FindAsync(fromUserId);
        var toEmployee = await _context.Employees.FindAsync(toUserId);

        return fromEmployee != null && toEmployee != null;
    }

    public async Task RequireFeedbackViewPermissionAsync(int requestingUserId, int targetEmployeeId)
    {
        var canView = await CanViewFeedbackAsync(requestingUserId, targetEmployeeId);
        if (!canView)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this feedback.");
        }
    }
}

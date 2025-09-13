using Microsoft.EntityFrameworkCore;
using HrAPI.Data;
using HrAPI.DTOs;
using HrAPI.Models;

namespace HrAPI.Services;

public class FeedbackService : IFeedbackService
{
    private readonly HrDbContext _context;

    public FeedbackService(HrDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FeedbackListDto>> GetReceivedFeedbackAsync(int employeeId, int requestingUserId)
    {
        // Check permissions first
        if (!await CanUserViewFeedbackAsync(employeeId, requestingUserId))
        {
            throw new UnauthorizedAccessException("You are not authorized to view this feedback.");
        }

        var feedbacks = await _context.Feedbacks
            .Include(f => f.FromEmployee)
            .Where(f => f.ToEmployeeId == employeeId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackListDto
            {
                Id = f.Id,
                FromEmployeeName = f.IsAnonymous ? "Anonymous" : f.FromEmployee.FullName,
                Content = f.Content,
                PolishedContent = f.PolishedContent,
                Type = f.Type,
                Rating = f.Rating,
                IsAnonymous = f.IsAnonymous,
                IsPolished = f.IsPolished,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync();

        return feedbacks;
    }

    public async Task<IEnumerable<FeedbackListDto>> GetGivenFeedbackAsync(int employeeId, int requestingUserId)
    {
        // Only allow users to view their own given feedback
        if (employeeId != requestingUserId)
        {
            throw new UnauthorizedAccessException("You can only view your own given feedback.");
        }

        var feedbacks = await _context.Feedbacks
            .Include(f => f.ToEmployee)
            .Where(f => f.FromEmployeeId == employeeId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackListDto
            {
                Id = f.Id,
                FromEmployeeName = f.ToEmployee.FullName, // For given feedback, show recipient name
                Content = f.Content,
                PolishedContent = f.PolishedContent,
                Type = f.Type,
                Rating = f.Rating,
                IsAnonymous = f.IsAnonymous,
                IsPolished = f.IsPolished,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync();

        return feedbacks;
    }

    public async Task<FeedbackDetailDto?> GetFeedbackByIdAsync(int feedbackId, int requestingUserId)
    {
        var feedback = await _context.Feedbacks
            .Include(f => f.FromEmployee)
            .Include(f => f.ToEmployee)
            .FirstOrDefaultAsync(f => f.Id == feedbackId);

        if (feedback == null)
        {
            return null;
        }

        // Check if user can view this feedback
        var canView = await CanUserViewFeedbackAsync(feedback.ToEmployeeId, requestingUserId) ||
                     feedback.FromEmployeeId == requestingUserId;

        if (!canView)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this feedback.");
        }

        return new FeedbackDetailDto
        {
            Id = feedback.Id,
            FromEmployeeId = feedback.FromEmployeeId,
            FromEmployeeName = feedback.IsAnonymous ? "Anonymous" : feedback.FromEmployee.FullName,
            ToEmployeeId = feedback.ToEmployeeId,
            ToEmployeeName = feedback.ToEmployee.FullName,
            Content = feedback.Content,
            PolishedContent = feedback.PolishedContent,
            Type = feedback.Type,
            Rating = feedback.Rating,
            IsAnonymous = feedback.IsAnonymous,
            IsPolished = feedback.IsPolished,
            CreatedAt = feedback.CreatedAt,
            UpdatedAt = feedback.UpdatedAt
        };
    }

    public async Task<FeedbackDetailDto> CreateFeedbackAsync(CreateFeedbackDto createFeedbackDto, int fromEmployeeId)
    {
        // Validate permissions
        if (!await CanUserGiveFeedbackAsync(createFeedbackDto.ToEmployeeId, fromEmployeeId))
        {
            throw new UnauthorizedAccessException("You cannot give feedback to this employee.");
        }

        // Verify the recipient exists
        var toEmployee = await _context.Employees.FindAsync(createFeedbackDto.ToEmployeeId);
        if (toEmployee == null)
        {
            throw new ArgumentException("Recipient employee not found.");
        }

        var feedback = new Feedback
        {
            FromEmployeeId = fromEmployeeId,
            ToEmployeeId = createFeedbackDto.ToEmployeeId,
            Content = createFeedbackDto.Content,
            Type = createFeedbackDto.Type,
            Rating = createFeedbackDto.Rating,
            IsAnonymous = createFeedbackDto.IsAnonymous,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(feedback)
            .Reference(f => f.FromEmployee)
            .LoadAsync();
        await _context.Entry(feedback)
            .Reference(f => f.ToEmployee)
            .LoadAsync();

        return new FeedbackDetailDto
        {
            Id = feedback.Id,
            FromEmployeeId = feedback.FromEmployeeId,
            FromEmployeeName = feedback.IsAnonymous ? "Anonymous" : feedback.FromEmployee.FullName,
            ToEmployeeId = feedback.ToEmployeeId,
            ToEmployeeName = feedback.ToEmployee.FullName,
            Content = feedback.Content,
            PolishedContent = feedback.PolishedContent,
            Type = feedback.Type,
            Rating = feedback.Rating,
            IsAnonymous = feedback.IsAnonymous,
            IsPolished = feedback.IsPolished,
            CreatedAt = feedback.CreatedAt,
            UpdatedAt = feedback.UpdatedAt
        };
    }

    public async Task<bool> CanUserViewFeedbackAsync(int targetEmployeeId, int requestingUserId)
    {
        // Users can view their own received feedback
        if (targetEmployeeId == requestingUserId)
        {
            return true;
        }

        // Check if requesting user is a manager
        var requestingUser = await _context.Employees.FindAsync(requestingUserId);
        if (requestingUser?.Role == EmployeeRole.Manager || requestingUser?.Role == EmployeeRole.Admin)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> CanUserGiveFeedbackAsync(int toEmployeeId, int fromEmployeeId)
    {
        // Cannot give feedback to self
        if (toEmployeeId == fromEmployeeId)
        {
            return false;
        }

        // Verify both employees exist
        var fromEmployee = await _context.Employees.FindAsync(fromEmployeeId);
        var toEmployee = await _context.Employees.FindAsync(toEmployeeId);

        return fromEmployee != null && toEmployee != null;
    }
}

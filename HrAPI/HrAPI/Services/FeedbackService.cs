using Microsoft.EntityFrameworkCore;
using HrAPI.Data;
using HrAPI.DTOs;
using HrAPI.Models;

namespace HrAPI.Services;

public class FeedbackService : IFeedbackService
{
    private readonly HrDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public FeedbackService(HrDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<IEnumerable<FeedbackListDto>> GetReceivedFeedbackAsync(int employeeId, int requestingUserId)
    {
        // Check permissions first
        await _authorizationService.RequireFeedbackViewPermissionAsync(requestingUserId, employeeId);

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
        var canViewFeedback = await _authorizationService.CanViewFeedbackAsync(requestingUserId, feedback.ToEmployeeId);
        var isOwnGivenFeedback = feedback.FromEmployeeId == requestingUserId;

        if (!canViewFeedback && !isOwnGivenFeedback)
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
        var canGiveFeedback = await _authorizationService.CanGiveFeedbackAsync(fromEmployeeId, createFeedbackDto.ToEmployeeId);
        if (!canGiveFeedback)
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
}

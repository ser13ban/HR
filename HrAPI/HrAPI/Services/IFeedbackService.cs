using HrAPI.DTOs;

namespace HrAPI.Services;

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackListDto>> GetReceivedFeedbackAsync(int employeeId, int requestingUserId);
    Task<IEnumerable<FeedbackListDto>> GetGivenFeedbackAsync(int employeeId, int requestingUserId);
    Task<FeedbackDetailDto?> GetFeedbackByIdAsync(int feedbackId, int requestingUserId);
    Task<FeedbackDetailDto> CreateFeedbackAsync(CreateFeedbackDto createFeedbackDto, int fromEmployeeId);
}

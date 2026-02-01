using App.DTOs;

namespace App.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<EnrollmentResponseDTO> CreateEnrollmentAsync(CreateEnrollmentDTO dto);
        Task<IEnumerable<EnrollmentResponseDTO>> CreateEnrollmentsFromPurchaseAsync(int purchaseId);
        Task<EnrollmentDetailDTO?> GetEnrollmentByIdAsync(int id);
        Task<EnrollmentResponseDTO?> GetUserEnrollmentForCourseAsync(string userId, int courseId);
        Task<PagedResult<EnrollmentResponseDTO>> GetUserEnrollmentsAsync(string userId, int? page, int? limit);
        Task<IEnumerable<EnrollmentDetailDTO>> GetCourseEnrollmentsAsync(int courseId);
        Task<EnrollmentResponseDTO> UpdateProgressAsync(int id, UpdateEnrollmentProgressDTO dto);
        Task<EnrollmentResponseDTO> UpdateStatusAsync(int id, UpdateEnrollmentStatusDTO dto);
        Task<bool> DeleteEnrollmentAsync(int id);
        Task<EnrollmentResponseDTO> RevokeAccessAsync(int id);
        Task<EnrollmentResponseDTO> RestoreAccessAsync(int id);
        Task<bool> IsUserEnrolledAsync(string userId, int courseId);
        Task<bool> HasActiveAccessAsync(string userId, int courseId);
        Task<int> GetCourseEnrollmentCountAsync(int courseId);
        Task<IEnumerable<EnrollmentResponseDTO>> GetActiveEnrollmentsByUserAsync(string userId);
        Task<int> TotalStudentsEnrolledAsync(int courseId);
    }
}
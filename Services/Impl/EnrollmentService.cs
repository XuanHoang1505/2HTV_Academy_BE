using App.Domain.Enums;
using App.Domain.Models;
using App.DTOs;
using App.Repositories.Interfaces;
using App.Services.Interfaces;
using App.Utils.Exceptions;

namespace App.Services.Implementations
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IPurchaseRepository _purchaseRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IReviewRepository _reviewRepo;
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepo,
            IPurchaseRepository purchaseRepo,
            ICourseRepository courseRepo,
            IReviewRepository reviewRepo,
            ILogger<EnrollmentService> logger)
        {
            _enrollmentRepo = enrollmentRepo;
            _purchaseRepo = purchaseRepo;
            _courseRepo = courseRepo;
            _reviewRepo = reviewRepo;
            _logger = logger;
        }

        public async Task<EnrollmentResponseDTO> CreateEnrollmentAsync(CreateEnrollmentDTO dto)
        {
            // Kiểm tra enrollment đã tồn tại chưa
            var existingEnrollment = await _enrollmentRepo.GetByUserAndCourseAsync(dto.UserId, dto.CourseId);
            if (existingEnrollment != null)
            {
                throw new InvalidOperationException("Học viên đã đăng ký khóa học này rồi");
            }

            // Kiểm tra course có tồn tại không
            var course = await _courseRepo.GetByIdAsync(dto.CourseId);
            if (course == null)
            {
                throw new InvalidOperationException("Khóa học không tồn tại");
            }

            var enrollment = new Enrollment
            {
                UserId = dto.UserId,
                CourseId = dto.CourseId,
                EnrolledAt = DateTime.UtcNow,
                ExpiresAt = dto.ExpiresAt,
                Progress = 0,
                Status = EnrollmentStatus.Active
            };

            var created = await _enrollmentRepo.CreateAsync(enrollment);


            await TotalStudentsEnrolledAsync(created.CourseId);

            return MapToResponseDTO(created, course.CourseTitle);
        }

        public async Task<IEnumerable<EnrollmentResponseDTO>> CreateEnrollmentsFromPurchaseAsync(int purchaseId)
        {
            // Lấy thông tin Purchase bao gồm PurchaseItems
            var purchase = await _purchaseRepo.GetByIdAsync(purchaseId);
            if (purchase == null)
            {
                throw new InvalidOperationException("Purchase không tồn tại");
            }

            if (purchase.Status != PurchaseStatus.Completed)
            {
                throw new InvalidOperationException("Purchase chưa được thanh toán");
            }

            // Kiểm tra có PurchaseItems không
            if (purchase.PurchaseItems == null || !purchase.PurchaseItems.Any())
            {
                throw new InvalidOperationException("Purchase không có khóa học nào");
            }

            var enrollments = new List<EnrollmentResponseDTO>();

            foreach (var item in purchase.PurchaseItems)
            {
                try
                {
                    var existingEnrollment = await _enrollmentRepo.GetByUserAndCourseAsync(
                        purchase.UserId,
                        item.CourseId);

                    if (existingEnrollment != null)
                    {
                        var existingCourse = await _courseRepo.GetByIdAsync(item.CourseId);
                        enrollments.Add(MapToResponseDTO(existingEnrollment, existingCourse?.CourseTitle ?? ""));
                        continue;
                    }

                    // Lấy thông tin Course
                    var courseInfo = await _courseRepo.GetByIdAsync(item.CourseId);
                    if (courseInfo == null)
                    {
                        _logger.LogWarning($"Course {item.CourseId} not found, skipping enrollment creation");
                        continue;
                    }

                    DateTime? expiresAt = DateTime.UtcNow.AddYears(1);

                    // Tạo enrollment
                    var enrollment = new Enrollment
                    {
                        UserId = purchase.UserId,
                        CourseId = item.CourseId,
                        EnrolledAt = DateTime.UtcNow,
                        ExpiresAt = expiresAt,
                        Progress = 0,
                        Status = EnrollmentStatus.Active
                    };

                    var created = await _enrollmentRepo.CreateAsync(enrollment);
                    await TotalStudentsEnrolledAsync(created.CourseId);
                    enrollments.Add(MapToResponseDTO(created, courseInfo.CourseTitle));

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to create enrollment for Course {item.CourseId} in Purchase {purchaseId}");
                    // Continue với các items khác
                }
            }

            if (!enrollments.Any())
            {
                throw new InvalidOperationException("Không thể tạo enrollment nào");
            }

            return enrollments;
        }

        public async Task<EnrollmentDetailDTO?> GetEnrollmentByIdAsync(int id)
        {
            var enrollment = await _enrollmentRepo.GetByIdAsync(id);
            if (enrollment == null)
                throw new AppException(ErrorCode.EnrollmentNotFound, "Enrollment không tồn tại");

            var completedLectureIds = enrollment.CourseProgresses
                .Select(cp => cp.LectureId)
                .ToHashSet();

            var userReview = await _reviewRepo.GetByUserAndCourseAsync(enrollment.UserId, enrollment.CourseId);

            var result = new EnrollmentDetailDTO
            {
                Id = enrollment.Id,
                UserId = enrollment.UserId,
                CourseId = enrollment.CourseId,
                EnrolledAt = enrollment.EnrolledAt,
                ExpiresAt = enrollment.ExpiresAt,
                Progress = enrollment.Progress,
                Status = enrollment.Status,
                IsExpired = enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTime.UtcNow,
                CreatedAt = enrollment.CreatedAt,
                UpdatedAt = enrollment.UpdatedAt,
                Course = new CourseDetailDTO
                {
                    Id = enrollment.Course.Id,
                    CourseTitle = enrollment.Course.CourseTitle,
                    CourseDescription = enrollment.Course.CourseDescription,
                    Slug = enrollment.Course.Slug,
                    CoursePrice = enrollment.Course.CoursePrice,
                    Discount = enrollment.Course.Discount,
                    CourseThumbnail = enrollment.Course.CourseThumbnail,
                    Status = enrollment.Course.Status,
                    IsPublished = enrollment.Course.IsPublished,
                    PublishedAt = enrollment.Course.PublishedAt,
                    CreatedAt = enrollment.Course.CreatedAt,
                    EducatorName = enrollment.Course.Educator.FullName,
                    CategoryName = enrollment.Course.Category.Name,
                    Curriculum = enrollment.Course.CourseContent
                    .OrderBy(ch => ch.ChapterOrder)
                    .Select(ch => new ChapterCurriculumDTO
                    {
                        Id = ch.Id,
                        ChapterTitle = ch.ChapterTitle,
                        ChapterOrder = ch.ChapterOrder,
                        Lectures = ch.ChapterContent
                            .OrderBy(l => l.LectureOrder)
                            .Select(l => new LectureDTO
                            {
                                Id = l.Id,
                                LectureTitle = l.LectureTitle,
                                LectureOrder = l.LectureOrder,
                                LectureUrl = l.LectureUrl,
                                IsPreviewFree = l.IsPreviewFree,
                                LectureDuration = l.LectureDuration,
                                IsCompleted = completedLectureIds.Contains(l.Id)
                            }).ToList()
                    }).ToList(),
                },
                UserReview = userReview == null ? null : new UserReviewDTO
                {
                    Id = userReview.Id,
                    Rating = userReview.Rating,
                    Comment = userReview.Comment
                }
            };
            return result;
        }

        public async Task<EnrollmentResponseDTO?> GetUserEnrollmentForCourseAsync(string userId, int courseId)
        {
            var enrollment = await _enrollmentRepo.GetByUserAndCourseAsync(userId, courseId);
            if (enrollment == null) return null;

            return MapToResponseDTO(enrollment, enrollment.Course?.CourseTitle ?? "");
        }

        public async Task<PagedResult<EnrollmentResponseDTO>> GetUserEnrollmentsAsync(string userId, int? page, int? limit)
        {
            if (!page.HasValue || !limit.HasValue)
            {
                var allEnrollments = await _enrollmentRepo.GetByUserIdAsync(userId, 1, int.MaxValue);

                return new PagedResult<EnrollmentResponseDTO>
                {
                    Data = allEnrollments.Select(e => MapToResponseDTO(e, e.Course?.CourseTitle ?? "")),
                    Total = allEnrollments.Count
                };
            }

            var enrollments = await _enrollmentRepo.GetByUserIdAsync(userId, page.Value, limit.Value);

            return new PagedResult<EnrollmentResponseDTO>
            {
                Data = enrollments.Select(e => MapToResponseDTO(e, e.Course?.CourseTitle ?? "")),
                Total = enrollments.TotalItemCount,
                TotalPages = enrollments.PageCount,
                CurrentPage = enrollments.PageNumber,
                Limit = enrollments.PageSize
            };
        }
       

        public async Task<IEnumerable<EnrollmentDetailDTO>> GetCourseEnrollmentsAsync(int courseId)
        {
            var enrollments = await _enrollmentRepo.GetByCourseIdAsync(courseId);
            return enrollments.Select(MapToDetailDTO).ToList();
        }

        public async Task<EnrollmentResponseDTO> UpdateProgressAsync(int id, UpdateEnrollmentProgressDTO dto)
        {
            var enrollment = await _enrollmentRepo.GetByIdAsync(id);
            if (enrollment == null)
            {
                throw new InvalidOperationException("Enrollment không tồn tại");
            }

            // Validate progress
            if (dto.Progress < 0 || dto.Progress > 100)
            {
                throw new ArgumentException("Progress phải từ 0 đến 100");
            }

            enrollment.Progress = dto.Progress;

            var updated = await _enrollmentRepo.UpdateAsync(enrollment);

            _logger.LogInformation($"Updated progress for enrollment {id} to {dto.Progress}%");

            return MapToResponseDTO(updated, updated.Course?.CourseTitle ?? "");
        }

        public async Task<EnrollmentResponseDTO> UpdateStatusAsync(int id, UpdateEnrollmentStatusDTO dto)
        {
            var enrollment = await _enrollmentRepo.GetByIdAsync(id);
            if (enrollment == null)
            {
                throw new InvalidOperationException("Enrollment không tồn tại");
            }

            enrollment.Status = dto.Status;

            var updated = await _enrollmentRepo.UpdateAsync(enrollment);

            _logger.LogInformation($"Updated status for enrollment {id} to {dto.Status}");

            return MapToResponseDTO(updated, updated.Course?.CourseTitle ?? "");
        }

        public async Task<bool> DeleteEnrollmentAsync(int id)
        {
            return await _enrollmentRepo.DeleteAsync(id);
        }

        public async Task<EnrollmentResponseDTO> RevokeAccessAsync(int id)
        {
            var enrollment = await _enrollmentRepo.GetByIdAsync(id);
            if (enrollment == null)
            {
                throw new InvalidOperationException("Enrollment không tồn tại");
            }

            // Kiểm tra nếu đã bị thu hồi rồi
            if (enrollment.Status == EnrollmentStatus.Cancelled)
            {
                throw new InvalidOperationException("Enrollment đã bị thu hồi quyền truy cập");
            }

            // Cập nhật status thành Suspended (tạm ngưng)
            enrollment.Status = EnrollmentStatus.Cancelled;

            var updated = await _enrollmentRepo.UpdateAsync(enrollment);

            _logger.LogWarning($"Revoked access for enrollment {id}");

            return MapToResponseDTO(updated, updated.Course?.CourseTitle ?? "");
        }

        public async Task<EnrollmentResponseDTO> RestoreAccessAsync(int id)
        {
            var enrollment = await _enrollmentRepo.GetByIdAsync(id);
            if (enrollment == null)
            {
                throw new InvalidOperationException("Enrollment không tồn tại");
            }

            // Kiểm tra nếu chưa bị thu hồi
            if (enrollment.Status == EnrollmentStatus.Active)
            {
                throw new InvalidOperationException("Enrollment đang hoạt động bình thường");
            }

            // Kiểm tra xem có hết hạn chưa
            if (enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Không thể khôi phục vì enrollment đã hết hạn");
            }

            // Khôi phục lại status
            enrollment.Status = EnrollmentStatus.Active;

            var updated = await _enrollmentRepo.UpdateAsync(enrollment);

            _logger.LogInformation($"Restored access for enrollment {id}");

            return MapToResponseDTO(updated, updated.Course?.CourseTitle ?? "");
        }

        public async Task<bool> IsUserEnrolledAsync(string userId, int courseId)
        {
            return await _enrollmentRepo.ExistsAsync(userId, courseId);
        }

        public async Task<bool> HasActiveAccessAsync(string userId, int courseId)
        {
            var enrollment = await _enrollmentRepo.GetByUserAndCourseAsync(userId, courseId);

            if (enrollment == null) return false;

            // Kiểm tra status
            if (enrollment.Status != EnrollmentStatus.Active) return false;

            // Kiểm tra expiration
            if (enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task<int> GetCourseEnrollmentCountAsync(int courseId)
        {
            return await _enrollmentRepo.GetTotalEnrollmentsByCourseAsync(courseId);
        }

        public async Task<IEnumerable<EnrollmentResponseDTO>> GetActiveEnrollmentsByUserAsync(string userId)
        {
            var enrollments = await _enrollmentRepo.GetActiveEnrollmentsByUserAsync(userId);
            return enrollments.Select(e => MapToResponseDTO(e, e.Course?.CourseTitle ?? "")).ToList();
        }

        private EnrollmentResponseDTO MapToResponseDTO(Enrollment enrollment, string courseName)
        {
            return new EnrollmentResponseDTO
            {
                Id = enrollment.Id,
                UserId = enrollment.UserId,
                CourseId = enrollment.CourseId,
                FinalPrice = enrollment.Course != null
                    ? enrollment.Course.CoursePrice - (enrollment.Course.CoursePrice * enrollment.Course.Discount / 100)
                    : 0,
                CourseName = courseName,
                Slug = enrollment.Course?.Slug ?? "",
                CourseThumbnail = enrollment.Course?.CourseThumbnail ?? "",
                EnrolledAt = enrollment.EnrolledAt,
                ExpiresAt = enrollment.ExpiresAt,
                Progress = enrollment.Progress,
                Status = enrollment.Status,
                IsExpired = enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTime.UtcNow,
                CreatedAt = enrollment.CreatedAt,
                UpdatedAt = enrollment.UpdatedAt
            };
        }

        private EnrollmentDetailDTO MapToDetailDTO(Enrollment enrollment)
        {
            return new EnrollmentDetailDTO
            {
                Id = enrollment.Id,
                UserId = enrollment.UserId,
                UserName = enrollment.User.FullName,
                UserEmail = enrollment.User.Email,
                UserAvatar = enrollment.User.ImageUrl,
                CourseId = enrollment.CourseId,
                CourseName = enrollment.Course?.CourseTitle ?? "",
                CourseThumbnail = enrollment.Course?.CourseThumbnail ?? "",
                EnrolledAt = enrollment.EnrolledAt,
                ExpiresAt = enrollment.ExpiresAt,
                Progress = enrollment.Progress,
                Status = enrollment.Status,
                IsExpired = enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTime.UtcNow,
                CreatedAt = enrollment.CreatedAt,
                UpdatedAt = enrollment.UpdatedAt
            };
        }

        public async Task<int> TotalStudentsEnrolledAsync(int courseId)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);

            if (course == null)
            {
                throw new InvalidOperationException("Khóa học không tồn tại");
            }

            course.TotalStudents = course.Enrollments?.Count(e => e.Status == EnrollmentStatus.Active) ?? 0;

            await _courseRepo.UpdateAsync(course);

            return course.TotalStudents;
        }
    }
}
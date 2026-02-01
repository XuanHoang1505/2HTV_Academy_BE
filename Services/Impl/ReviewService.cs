using App.Domain.Enums;
using App.Domain.Models;
using App.DTOs;
using App.Enums;
using App.Repositories.Interfaces;
using App.Services.Interfaces;
using App.Utils.Exceptions;
using AutoMapper;

namespace App.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IMapper _mapper;

        public ReviewService(
            IReviewRepository reviewRepository,
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IPurchaseRepository purchaseRepository,
            IEnrollmentRepository enrollmentRepository,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _purchaseRepository = purchaseRepository;
            _mapper = mapper;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<ReviewDTO> CreateReviewAsync(CreateReviewDTO dto)
        {
            var course = await _courseRepository.GetByIdAsync(dto.CourseId);
            if (course == null)
            {
                throw new AppException(ErrorCode.CourseNotFound, $"Không tìm thấy khóa học với ID = {dto.CourseId}");
            }

            var user = await _userRepository.GetUserByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new AppException(ErrorCode.UserNotFound, $"Không tìm thấy người dùng với ID = {dto.UserId}");
            }

            var existingReview = await _reviewRepository.ExistsAsync(dto.UserId, dto.CourseId);
            if (existingReview)
            {
                throw new AppException(ErrorCode.AlreadyReview, "Bạn đã đánh giá khóa học này rồi");
            }

            var isEnrolled = await _enrollmentRepository.IsUserEnrolledInCourseAsync(dto.UserId, dto.CourseId);

            if (!isEnrolled)
            {
                throw new AppException(ErrorCode.BadRequest, "Bạn chỉ có thể đánh giá khóa học đã đăng ký");
            }

            var review = new Review
            {
                UserId = dto.UserId,
                CourseId = dto.CourseId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.Now,
                Status = ReviewStatus.Enable
            };

            var createdReview = await _reviewRepository.CreateAsync(review);

            var result = await _reviewRepository.GetByIdWithDetailsAsync(createdReview.Id);

            await TotalReviewsAsync(createdReview.CourseId);
            await AverageRatingAsync(createdReview.CourseId);


            return new ReviewDTO
            {
                Id = result.Id,
                UserId = result.UserId,
                UserName = result.User.FullName,
                UserAvatar = result.User.ImageUrl,
                CourseId = result.CourseId,
                CourseName = result.Course.CourseTitle,
                Rating = result.Rating,
                Comment = result.Comment,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };
        }

        public async Task<ReviewDTO> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdWithDetailsAsync(id);
            if (review == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy đánh giá với ID = {id}");
            }
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<PagedResult<ReviewDTO>> GetAllReviewsAsync(int? page, int? limit)
        {
            if (!page.HasValue || !limit.HasValue)
            {
                var allReviews = await _reviewRepository.AllAsync();
                return new PagedResult<ReviewDTO>
                {
                    Data = _mapper.Map<IEnumerable<ReviewDTO>>(allReviews),
                    Total = allReviews.Count()
                };
            }
            var pagedReviews = await _reviewRepository.GetAllAsync(page.Value, limit.Value);
            return new PagedResult<ReviewDTO>
            {
                Data = _mapper.Map<IEnumerable<ReviewDTO>>(pagedReviews),
                Total = pagedReviews.TotalItemCount,
                TotalPages = pagedReviews.PageCount,
                CurrentPage = pagedReviews.PageNumber,
                Limit = pagedReviews.PageSize
            };

        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy khóa học với ID = {courseId}");
            }

            var reviews = await _reviewRepository.GetByCourseIdAsync(courseId);
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByUserIdAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy người dùng với ID = {userId}");
            }

            var reviews = await _reviewRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<ReviewDTO> UpdateReviewAsync(int id, UpdateReviewDTO dto, string userId)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy đánh giá với ID = {id}");
            }

            if (review.UserId != userId)
            {
                throw new AppException(ErrorCode.Forbidden, "Bạn không có quyền sửa đánh giá này");
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.Now;

            await _reviewRepository.UpdateAsync(review);

            var result = await _reviewRepository.GetByIdWithDetailsAsync(id);
            await TotalReviewsAsync(review.CourseId);
            await AverageRatingAsync(review.CourseId);

            return _mapper.Map<ReviewDTO>(result);
        }

        public async Task<bool> DeleteReviewAsync(int id, string userId)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy đánh giá với ID = {id}");
            }

            if (review.UserId != userId)
            {
                throw new AppException(ErrorCode.Forbidden, "Bạn không có quyền xóa đánh giá này");
            }

            await TotalReviewsAsync(review.CourseId);
            await AverageRatingAsync(review.CourseId);

            return await _reviewRepository.DeleteAsync(id);
        }

        public async Task<CourseReviewStatsDTO> GetCourseReviewStatsAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy khóa học với ID = {courseId}");
            }

            var totalReviews = await _reviewRepository.GetTotalReviewsCountAsync(courseId);
            var averageRating = await _reviewRepository.GetAverageRatingAsync(courseId);
            var distribution = await _reviewRepository.GetRatingDistributionAsync(courseId);

            return new CourseReviewStatsDTO
            {
                CourseId = courseId,
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 2),
                FiveStarCount = distribution.GetValueOrDefault(5, 0),
                FourStarCount = distribution.GetValueOrDefault(4, 0),
                ThreeStarCount = distribution.GetValueOrDefault(3, 0),
                TwoStarCount = distribution.GetValueOrDefault(2, 0),
                OneStarCount = distribution.GetValueOrDefault(1, 0)
            };
        }

        public async Task<bool> UserHasReviewedCourseAsync(string userId, int courseId)
        {
            return await _reviewRepository.ExistsAsync(userId, courseId);
        }

        public async Task<ReviewDTO> HideReviewAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy đánh giá với ID = {id}");
            }

            // Change status to Disable
            review.Status = ReviewStatus.Disable;
            review.UpdatedAt = DateTime.Now;

            await _reviewRepository.UpdateAsync(review);

            var result = await _reviewRepository.GetByIdWithDetailsAsync(id);
            return _mapper.Map<ReviewDTO>(result);
        }

        public async Task<ReviewDTO> ShowReviewAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy đánh giá với ID = {id}");
            }

            review.Status = ReviewStatus.Enable;
            review.UpdatedAt = DateTime.Now;

            await _reviewRepository.UpdateAsync(review);

            var result = await _reviewRepository.GetByIdWithDetailsAsync(id);
            return _mapper.Map<ReviewDTO>(result);
        }

        public async Task TotalReviewsAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy khóa học với ID = {courseId}");
            }

            var totalReviews = await _reviewRepository.GetTotalReviewsCountAsync(courseId);
            course.TotalReviews = totalReviews;
            await _courseRepository.UpdateAsync(course);

        }

        public async Task AverageRatingAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new AppException(ErrorCode.NotFound, $"Không tìm thấy khóa học với ID = {courseId}");

            var averageRating = await _reviewRepository.GetAverageRatingAsync(courseId);
            course.AverageRating = Math.Round(averageRating, 2);
            await _courseRepository.UpdateAsync(course);
        }
    }
}
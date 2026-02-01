using AutoMapper;
using App.DTOs;
using App.Domain.Models;
using App.Repositories.Interfaces;
using App.Services.Interfaces;
using App.Utils.Exceptions;
using App.Services;
using App.Domain.Enums;

namespace App.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repository;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinaryService;


        public CourseService(ICourseRepository repository, IMapper mapper, CloudinaryService cloudinaryService)
        {
            _repository = repository;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PagedResult<CourseDTO>> GetAllCourses(int? page, int? limit, Dictionary<string, string>? filters = null)
        {
            if (!page.HasValue || !limit.HasValue)
            {
                var allCorses = await _repository.AllCoursesAsync();
                var totalCourses = allCorses.Count();

                return new PagedResult<CourseDTO>
                {
                    Data = _mapper.Map<IEnumerable<CourseDTO>>(allCorses),
                    Total = totalCourses
                };
            }

            var courses = await _repository.GetAllAsync(page.Value, limit.Value, filters);

            return new PagedResult<CourseDTO>
            {
                Data = _mapper.Map<IEnumerable<CourseDTO>>(courses),
                Total = courses.TotalItemCount,
                TotalPages = courses.PageCount,
                CurrentPage = courses.PageNumber,
                Limit = courses.PageSize
            };
        }
        public async Task<PagedResult<CourseDTO>> GetAllCoursesPublishAsync(int? page, int? limit, Dictionary<string, string>? filters = null)
        {
            if (!page.HasValue || !limit.HasValue)
            {
                var allCorsesPublish = await _repository.AllCoursesPublishAsync();
                var totalCourses = allCorsesPublish.Count();

                return new PagedResult<CourseDTO>
                {
                    Data = _mapper.Map<IEnumerable<CourseDTO>>(allCorsesPublish),
                    Total = totalCourses
                };
            }

            var courses = await _repository.GetAllPublishAsync(page.Value, limit.Value, filters);

            return new PagedResult<CourseDTO>
            {
                Data = _mapper.Map<IEnumerable<CourseDTO>>(courses),
                Total = courses.TotalItemCount,
                TotalPages = courses.PageCount,
                CurrentPage = courses.PageNumber,
                Limit = courses.PageSize
            };
        }

        public async Task<CourseDetailDTO?> GetByIdAsync(int id)
        {
            var course = await _repository.GetByIdAsync(id);
            if (course == null)
                throw new AppException(ErrorCode.CourseNotFound, $"Không tìm thấy khóa học với ID = {id}");

            var result = new CourseDetailDTO
            {
                Id = course.Id,
                CourseTitle = course.CourseTitle,
                CourseDescription = course.CourseDescription,
                ShortDescription = course.ShortDescription,
                Language = course.Language,
                Level = course.Level,
                Slug = course.Slug,
                CoursePrice = course.CoursePrice,
                Discount = course.Discount,
                TotalLectures = course.TotalLectures,
                TotalDuration = course.TotalDuration,
                TotalStudents = course.TotalStudents,
                AverageRating = course.AverageRating,
                CourseThumbnail = course.CourseThumbnail,
                Status = course.Status,
                IsPublished = course.IsPublished,
                PublishedAt = course.PublishedAt,
                CreatedAt = course.CreatedAt,
                EducatorName = course.Educator.FullName,
                CategoryId = course.Category.Id,
                CategoryName = course.Category.Name,
                Curriculum = course.CourseContent
                    .OrderBy(ch => ch.ChapterOrder)
                    .Select(ch => new ChapterCurriculumDTO
                    {
                        Id = ch.Id,
                        ChapterTitle = ch.ChapterTitle,
                        ChapterOrder = ch.ChapterOrder,
                        ChapterDescription = ch.ChapterDescription,
                        Lectures = ch.ChapterContent
                            .OrderBy(l => l.LectureOrder)
                            .Select(l => new LectureDTO
                            {
                                Id = l.Id,
                                LectureTitle = l.LectureTitle,
                                LectureOrder = l.LectureOrder,
                                LectureUrl = l.LectureUrl,
                                IsPreviewFree = l.IsPreviewFree,
                                LectureDuration = l.LectureDuration
                            }).ToList()
                    }).ToList(),
            };
            return result;
        }

        public async Task<CourseDetailDTO?> GetBySlugAsync(string slug)
        {
            var course = await _repository.GetBySlugAsync(slug);
            if (course == null)
                throw new AppException(ErrorCode.CourseNotFound, $"Không tìm thấy khóa học với Slug = {slug}");

            var result = new CourseDetailDTO
            {
                Id = course.Id,
                CourseTitle = course.CourseTitle,
                CourseDescription = course.CourseDescription,
                ShortDescription = course.ShortDescription,
                Language = course.Language,
                Level = course.Level,
                Slug = course.Slug,
                CoursePrice = course.CoursePrice,
                Discount = course.Discount,
                TotalLectures = course.TotalLectures,
                TotalDuration = course.TotalDuration,
                TotalStudents = course.TotalStudents,
                AverageRating = course.AverageRating,
                TotalReviews = course.TotalReviews,
                CourseThumbnail = course.CourseThumbnail,
                Status = course.Status,
                IsPublished = course.IsPublished,
                PublishedAt = course.PublishedAt,
                CreatedAt = course.CreatedAt,
                EducatorName = course.Educator.FullName,
                CategoryId = course.Category.Id,
                CategoryName = course.Category.Name,
                Curriculum = course.CourseContent
                    .OrderBy(ch => ch.ChapterOrder)
                    .Select(ch => new ChapterCurriculumDTO
                    {
                        Id = ch.Id,
                        ChapterTitle = ch.ChapterTitle,
                        ChapterOrder = ch.ChapterOrder,
                        ChapterDescription = ch.ChapterDescription,
                        Lectures = ch.ChapterContent
                            .OrderBy(l => l.LectureOrder)
                            .Select(l => new LectureDTO
                            {
                                Id = l.Id,
                                LectureTitle = l.LectureTitle,
                                LectureOrder = l.LectureOrder,
                                LectureUrl = l.IsPreviewFree ? l.LectureUrl : null,
                                IsPreviewFree = l.IsPreviewFree,
                                LectureDuration = l.LectureDuration
                            }).ToList()
                    }).ToList(),
            };
            return result;
        }


        public async Task<CourseDTO> CreateAsync(CourseDTO dto)
        {
            var slugExists = await _repository.ExistsBySlugAsync(dto.Slug);
            if (slugExists)
                throw new AppException(
                    ErrorCode.SlugAlreadyExists,
                    $"Slug '{dto.Slug}' đã tồn tại"
                );

            var entity = _mapper.Map<Course>(dto);

            if (dto.CourseThumbnailFile != null && dto.CourseThumbnailFile.Length > 0)
            {
                entity.CourseThumbnail = await _cloudinaryService
                    .UploadImageAsync(dto.CourseThumbnailFile, "course_thumbnail");
            }

            if (dto.Status == CourseStatus.published)
            {
                entity.IsPublished = true;
                entity.PublishedAt = DateTime.UtcNow;
            }
            else
            {
                entity.IsPublished = false;
                entity.PublishedAt = null;
            }

            var created = await _repository.AddAsync(entity);
            return _mapper.Map<CourseDTO>(created);
        }

        public async Task<CourseDTO> UpdateAsync(int id, CourseDTO dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new AppException(
                    ErrorCode.CourseNotFound,
                    $"Không tìm thấy khóa học với ID = {id}"
                );

            if (dto.CourseThumbnailFile != null && dto.CourseThumbnailFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existing.CourseThumbnail))
                {
                    var oldPublicId = CloudinaryService.ExtractPublicId(existing.CourseThumbnail);
                    await _cloudinaryService.DeleteImageAsync(oldPublicId);
                }

                existing.CourseThumbnail = await _cloudinaryService
                    .UploadImageAsync(dto.CourseThumbnailFile, "course_thumbnail");
            }

            _mapper.Map(dto, existing);

            if (dto.Status == CourseStatus.published)
            {
                existing.Status = dto.Status;
                existing.IsPublished = true;
                if (existing.PublishedAt == null)
                {
                    existing.PublishedAt = DateTime.UtcNow;
                }
            }
            else
            {
                existing.Status = dto.Status;
                existing.IsPublished = false;
                existing.PublishedAt = null;
            }

            await _repository.UpdateAsync(existing);

            return _mapper.Map<CourseDTO>(existing);
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new AppException(ErrorCode.CourseNotFound, $"Không tìm thấy khóa học với ID = {id}");

            if (!string.IsNullOrEmpty(existing.CourseThumbnail))
            {
                var publicId = CloudinaryService.ExtractPublicId(existing.CourseThumbnail);
                await _cloudinaryService.DeleteImageAsync(publicId);
            }

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<CourseDetailDTO?> CourseDetailAsync(int id)
        {
            var course = await _repository.GetByIdAsync(id);

            if (course == null)
                throw new AppException(ErrorCode.CourseNotFound, $"Không tìm thấy khóa học với ID = {id}");

            var result = new CourseDetailDTO
            {
                Id = course.Id,
                CourseTitle = course.CourseTitle,
                CourseDescription = course.CourseDescription,
                ShortDescription = course.ShortDescription,
                Language = course.Language,
                Level = course.Level,
                Slug = course.Slug,
                CoursePrice = course.CoursePrice,
                Discount = course.Discount,
                TotalLectures = course.TotalLectures,
                TotalDuration = course.TotalDuration,
                TotalStudents = course.TotalStudents,
                AverageRating = course.AverageRating,
                CourseThumbnail = course.CourseThumbnail,
                Status = course.Status,
                IsPublished = course.IsPublished,
                PublishedAt = course.PublishedAt,
                CreatedAt = course.CreatedAt,
                EducatorName = course.Educator.FullName,
                CategoryId = course.Category.Id,
                CategoryName = course.Category.Name,
                Curriculum = course.CourseContent
                    .OrderBy(ch => ch.ChapterOrder)
                    .Select(ch => new ChapterCurriculumDTO
                    {
                        Id = ch.Id,
                        ChapterTitle = ch.ChapterTitle,
                        ChapterOrder = ch.ChapterOrder,
                        ChapterDescription = ch.ChapterDescription,
                        Lectures = ch.ChapterContent 
                            .OrderBy(l => l.LectureOrder)
                            .Select(l => new LectureDTO
                            {
                                Id = l.Id,
                                LectureTitle = l.LectureTitle,
                                LectureOrder = l.LectureOrder,
                                LectureUrl = l.LectureUrl,
                                IsPreviewFree = l.IsPreviewFree,
                                LectureDuration = l.LectureDuration
                            }).ToList()
                    }).ToList(),
            };
            return result;
        }


        public async Task<IEnumerable<CourseDTO>> SearchAsync(CourseFilterDTO filter)
        {
            var courses = await _repository.SearchAsync(filter);
            return _mapper.Map<IEnumerable<CourseDTO>>(courses);
        }



        public async Task<IEnumerable<CourseDTO>> GetCoursesBestSellerAsync()
        {
            var courses = await _repository.GetCoursesBestSellerAsync();
            return _mapper.Map<IEnumerable<CourseDTO>>(courses);
        }

        public async Task<IEnumerable<CourseDTO>> GetCoursesNewestAsync()
        {
            var courses = await _repository.GetCoursesNewestAsync();
            return _mapper.Map<IEnumerable<CourseDTO>>(courses);
        }

        public async Task<IEnumerable<CourseDTO>> GetCoursesRatingAsync()
        {
            var courses = await _repository.GetCoursesRatingAsync();
            return _mapper.Map<IEnumerable<CourseDTO>>(courses);
        }

        public Task<IEnumerable<StudentCourseProgressDTO>> GetStudentProgressByCourseIdAsync(int courseId)
        {
            throw new NotImplementedException();
        }
    }
}

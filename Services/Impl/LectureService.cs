using AutoMapper;
using App.DTOs;
using App.Domain.Models;
using App.Repositories.Interfaces;
using App.Services.Interfaces;
using App.Utils.Exceptions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace App.Services.Implementations
{
    public class LectureService : ILectureService
    {
        private readonly ILectureRepository _lecture;
        private readonly ICourseRepository _course;
        private readonly IChapterRepository _chapter;
        private readonly IMapper _mapper;
        public LectureService(ILectureRepository lecture, IMapper mapper, ICourseRepository course, IChapterRepository chapter)
        {
            _lecture = lecture;
            _mapper = mapper;
            _course = course;
            _chapter = chapter;
        }
        public async Task CountLecturesAsync(int courseId)
        {
            var course = await _course.GetByIdAsync(courseId);

            if (course == null)
                throw new AppException(ErrorCode.CategoryNotFound, $"Không tìm thấy khóa học với ID = {courseId}");
            course.TotalLectures = course.CourseContent?.Sum(ch => ch.ChapterContent?.Count ?? 0) ?? 0;

            await _course.UpdateAsync(course);
        }

        public async Task TotalDurationAsync(int courseId)
        {
            var course = await _course.GetByIdAsync(courseId);

            if (course == null)
                throw new AppException(ErrorCode.CategoryNotFound, $"Không tìm thấy khóa học với ID = {courseId}");

            course.TotalDuration = course.CourseContent?.Sum(ch => ch.ChapterContent?.Sum(lec => lec.LectureDuration) ?? 0) ?? 0;

            await _course.UpdateAsync(course);
        }
        public async Task<LectureDTO> CreateAsync(LectureDTO dto)
        {
            var existing = await _lecture.GetByTitleAsync(dto.LectureTitle);
            if (existing != null)
                throw new AppException(ErrorCode.CategorySlugAlreadyExists, $"Slug '{dto.LectureTitle}' đã tồn tại.");

            var entity = _mapper.Map<Lecture>(dto);
            var maxOrder = await _lecture.GetMaxOrderByChapterIdAsync(dto.ChapterId);
            entity.LectureOrder = maxOrder + 1;
            var created = await _lecture.AddAsync(entity);
            var chapter = await _chapter.GetByIdAsync(entity.ChapterId);
            if (chapter != null)
            {
                var course = await _course.GetByIdAsync(chapter.CourseId);
                if (course != null)
                {
                    await CountLecturesAsync(course.Id);
                    await TotalDurationAsync(course.Id);
                }
            }

            var dtoResult = _mapper.Map<LectureDTO>(created);
            return dtoResult;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _lecture.GetByIdAsync(id);
            if (existing == null)
                throw new AppException(ErrorCode.CategoryNotFound, $"Không tìm thấy bài học với ID = {id}");

            await _lecture.DeleteAsync(id);

            var chapter = await _chapter.GetByIdAsync(existing.ChapterId);
            if (chapter != null)
            {
                var course = await _course.GetByIdAsync(chapter.CourseId);
                if (course != null)
                {
                    await CountLecturesAsync(course.Id);
                    await TotalDurationAsync(course.Id);
                }
            }

            return true;
        }

        public async Task<PagedResult<LectureDTO>> GetAllAsync(int? page, int? limit, Dictionary<string, string>? filters = null)
        {
            if (!page.HasValue || !limit.HasValue)
            {
                var allLectures = await _lecture.AllAsync();

                return new PagedResult<LectureDTO>
                {
                    Data = _mapper.Map<IEnumerable<LectureDTO>>(allLectures),
                    Total = allLectures.Count()
                };
            }

            var lectures = await _lecture.GetAllAsync(page.Value, limit.Value, filters);
            return new PagedResult<LectureDTO>
            {
                Data = _mapper.Map<IEnumerable<LectureDTO>>(lectures),
                Total = lectures.TotalItemCount,
                TotalPages = lectures.PageCount,
                CurrentPage = lectures.PageNumber,
                Limit = lectures.PageSize
            };
        }

        public async Task<LectureDTO?> GetByIdAsync(int id)
        {
            var lecture = await _lecture.GetByIdAsync(id);
            if (lecture == null)
                throw new AppException(ErrorCode.CategoryNotFound, $"Không tìm thấy bài học với ID = {id}");

            return _mapper.Map<LectureDTO>(lecture);
        }

        public async Task<LectureDTO?> GetByTitleAsync(string lectureTitle)
        {
            var lecture = await _lecture.GetByTitleAsync(lectureTitle);
            if (lecture == null)
            {
                throw new AppException(ErrorCode.CategoryNotFound, $"Không tìm thấy bài học với ID = {lectureTitle}");
            }
            return _mapper.Map<LectureDTO>(lecture);
        }

        public async Task<LectureDTO> UpdateAsync(int id, LectureDTO dto)
        {
            var existing = await _lecture.GetByIdAsync(id);
            if (existing == null)
                throw new AppException(ErrorCode.CategoryNotFound, $"Không tìm thấy bài học với ID = {id}");

            existing.LectureTitle = dto.LectureTitle ?? existing.LectureTitle;
            existing.LectureDuration = dto.LectureDuration ?? existing.LectureDuration;
            existing.LectureUrl = dto.LectureUrl ?? existing.LectureUrl;
            existing.IsPreviewFree = dto.IsPreviewFree ?? existing.IsPreviewFree;
            
            await _lecture.UpdateAsync(existing);

            var chapter = await _chapter.GetByIdAsync(existing.ChapterId);
            if (chapter != null)
            {
                var course = await _course.GetByIdAsync(chapter.CourseId);
                if (course != null)
                {
                    await TotalDurationAsync(course.Id);
                }
            }

            var dtoResult = _mapper.Map<LectureDTO>(existing);
            return dtoResult;
        }
    }
}
using App.Data;
using App.Domain.Models;
using App.DTOs;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;

namespace App.Mappings
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            CreateMap<Chapter, ChapterDTO>().ReverseMap();
            CreateMap<Lecture, LectureDTO>();
            CreateMap<LectureDTO, Lecture>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Chapter, MyCourseChapterDTO>()
                .ForMember(dest => dest.Lectures,
                    opt => opt.MapFrom(src => src.ChapterContent));

            CreateMap<Course, MyCourseDTO>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.EducatorName,
                    opt => opt.MapFrom(src => src.Educator.FullName))
                .ForMember(dest => dest.TotalChapters,
                    opt => opt.MapFrom(src => src.CourseContent.Count))
                .ForMember(dest => dest.TotalLectures,
                    opt => opt.MapFrom(src => src.CourseContent
                        .SelectMany(ch => ch.ChapterContent)
                        .Count()))
                .ForMember(dest => dest.Chapters,
                    opt => opt.MapFrom(src => src.CourseContent));

            CreateMap<ApplicationUser, UserDTO>();
            CreateMap<UserDTO, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());


            CreateMap<ApplicationUser, RegisterDTO>().ReverseMap();

            CreateMap<Cart, CartDTO>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.CartItems.Sum(ci => ci.Price)))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.CartItems.Count));

            // CartItem -> CartItemDto
            CreateMap<CartItem, CartItemDTO>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseTitle))
                .ForMember(dest => dest.CourseImage, opt => opt.MapFrom(src => src.Course.CourseThumbnail))
                .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Course.Discount));

            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryDTO, Category>()
             .ForMember(dest => dest.Id, opt => opt.Ignore());


            CreateMap<CourseDTO, Course>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.CourseThumbnail, opt => opt.Ignore());

            CreateMap<Course, CourseDTO>()
             .ForMember(dest => dest.EducatorName, opt => opt.MapFrom(src => src.Educator.FullName))
             .ForMember(dest => dest.CourseThumbnailFile, opt => opt.Ignore())
             .ForMember(dest => dest.CourseThumbnail, opt => opt.MapFrom(src => src.CourseThumbnail));

            CreateMap<Course, CourseDetailDTO>().ReverseMap();


            CreateMap<Purchase, PurchaseDTO>();
            CreateMap<PurchaseDTO, Purchase>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<PurchaseItem, PurchaseItemDTO>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.CourseTitle));
            CreateMap<PurchaseItemDTO, PurchaseItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Review, ReviewDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.ImageUrl))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseTitle))
                .ForMember(dest => dest.HelpfulCount, opt => opt.Ignore()); 

            CreateMap<ReviewDTO, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.Status, opt => opt.Ignore()); 
        }
    }
}


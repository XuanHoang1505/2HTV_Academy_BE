using System.ComponentModel.DataAnnotations;
using App.Domain.Models;
using App.Enums;
using Microsoft.AspNetCore.Identity;

namespace App.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public bool IsLocked { get; set; } = false;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public Cart Cart { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Course> EducatorCourses { get; set; } = new List<Course>();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<CourseProgress> CourseProgresses { get; set; } = new List<CourseProgress>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        

    }
}

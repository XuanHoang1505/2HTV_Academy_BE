using System.ComponentModel.DataAnnotations;

namespace App.DTOs
{
    public class CreateReviewDTO
    {   
        public string? UserId { get; set; }
        [Required(ErrorMessage = "CourseId là bắt buộc")]
        public int CourseId { get; set; }
        
        [Required(ErrorMessage = "Rating là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
        public int Rating { get; set; }
        
        [MaxLength(1000, ErrorMessage = "Comment không được vượt quá 1000 ký tự")]
        public string Comment { get; set; }
    }
}
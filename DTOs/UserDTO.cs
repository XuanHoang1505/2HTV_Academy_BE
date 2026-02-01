namespace App.DTOs
{
    public class UserDTO
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? ImageUrl { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool? IsLocked { get; set; }
        public string? PhoneNumber { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}

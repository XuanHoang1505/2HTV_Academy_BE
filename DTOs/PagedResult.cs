namespace App.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
        public int? TotalPages { get; set; }
        public int? CurrentPage { get; set; }
        public int? Limit { get; set; }
    }
}
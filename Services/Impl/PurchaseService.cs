using App.Domain.Enums;
using App.Domain.Models;
using App.DTOs;
using App.Repositories.Interfaces;
using App.Services.Interfaces;
using App.Utils.Exceptions;
using AutoMapper;
using Azure;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;

    public PurchaseService(IPurchaseRepository purchaseRepository, IMapper mapper, ICourseRepository courseRepository)
    {
        _purchaseRepository = purchaseRepository;
        _mapper = mapper;
        _courseRepository = courseRepository;
    }

    public async Task<PagedResult<PurchaseDTO>> GetAllPurchasesAsync(int? page, int? limit)
    {

        if (!page.HasValue || !limit.HasValue)
        {
            var allPurchases = await _purchaseRepository.AllPurchasesAsync();

            return new PagedResult<PurchaseDTO>
            {
                Data = _mapper.Map<IEnumerable<PurchaseDTO>>(allPurchases),
                Total = allPurchases.Count()
            };
        }
        var purchases = await _purchaseRepository.GetPagedPurchasesAsync(page.Value, limit.Value);
        return new PagedResult<PurchaseDTO>
        {
            Data = _mapper.Map<IEnumerable<PurchaseDTO>>(purchases),
            Total = purchases.TotalItemCount,
            TotalPages = purchases.PageCount,
            CurrentPage = purchases.PageNumber,
            Limit = purchases.PageSize
        };
    }

    public async Task<IEnumerable<PurchaseItemDTO>> GetPurchaseItemByPurchaseIdAsync(int purchaseId)
    {
        var purchaseItems = await _purchaseRepository.GetPurchaseItemByPurchaseIdAsync(purchaseId);
        return _mapper.Map<IEnumerable<PurchaseItemDTO>>(purchaseItems);
    }

    public async Task<PurchaseDTO> GetPurchaseByIdAsync(int purchaseId)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
        if (purchase == null)
        {
            throw new AppException(
                ErrorCode.PurchaseNotFound,
                $"Không tìm thấy đơn mua với ID = {purchaseId}"
            );
        }

        var purchaseDto = new PurchaseDTO
        {
            Id = purchase.Id,
            UserId = purchase.UserId,
            Amount = purchase.Amount,
            Status = purchase.Status,
            CreatedAt = purchase.CreatedAt,
            UserName = purchase.User?.FullName,
            Email = purchase.User?.Email,
            Items = purchase.PurchaseItems?
                .Select(item => new PurchaseItemDTO
                {
                    Id = item.Id,
                    CourseId = item.CourseId,
                    CourseTitle = item.Course?.CourseTitle,
                    Price = item.Price,
                    Discount = item.Course?.Discount ?? 0
                })
                .ToList() ?? new List<PurchaseItemDTO>()
        };

        return purchaseDto;
    }

    public async Task<PurchaseDTO> CreatePurchaseAsync(CreatePurchaseDTO dto)
    {
        var courses = new List<Course>();
        foreach (var courseId in dto.CourseIds)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new AppException(ErrorCode.CourseNotFound, $"Không tìm thấy khóa học với ID = {courseId}");
            }
            courses.Add(course);
        }

        var purchase = new Purchase
        {
            UserId = dto.UserId,
            Amount = dto.Amount,
            Status = PurchaseStatus.Pending,
            CreatedAt = DateTime.Now,
            PurchaseItems = new List<PurchaseItem>()
        };

        foreach (var course in courses)
        {
            purchase.PurchaseItems.Add(new PurchaseItem
            {
                CourseId = course.Id,
                Price = course.CoursePrice
            });
        }

        var createdPurchase = await _purchaseRepository.CreateAsync(purchase);

        // Lấy lại purchase với đầy đủ thông tin
        var result = await _purchaseRepository.GetByIdAsync(createdPurchase.Id);

        return _mapper.Map<PurchaseDTO>(result);
    }

    public async Task<PurchaseDTO> UpdatePurchaseAsync(int id, PurchaseDTO dto)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(id);
        if (purchase == null)
        {
            throw new AppException(ErrorCode.FileNotFound, $"Không tìm thấy đơn mua với ID = {id}");
        }
        _mapper.Map(dto, purchase);
        await _purchaseRepository.UpdatePurchaseAsync(purchase);
        return _mapper.Map<PurchaseDTO>(purchase);
    }

    public async Task<bool> DeletePurchaseAsync(int id)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(id);
        if (purchase == null)
        {
            throw new AppException(ErrorCode.FileNotFound, $"Không tìm thấy đơn mua với ID = {id}");
        }
        await _purchaseRepository.DeletePurchaseAsync(id);
        return true;
    }

    public async Task<IEnumerable<PurchaseDTO>> GetPurchasesByUserIdAsync(string userId)
    {
        var purchases = await _purchaseRepository.GetAllPurchaseByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<PurchaseDTO>>(purchases);
    }

    public async Task<PurchaseDTO> UpdatePurchaseStatusAsync(int id, UpdatePurchaseStatusDTO dto)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(id);
        if (purchase == null)
        {
            throw new AppException(ErrorCode.PurchaseNotFound, $"Không tìm thấy đơn mua với ID = {id}");
        }

        purchase.Status = dto.Status;

        if (dto.Status == PurchaseStatus.Completed)
        {
            purchase.CreatedAt = DateTime.Now;
        }

        await _purchaseRepository.UpdatePurchaseAsync(purchase);

        return _mapper.Map<PurchaseDTO>(purchase);
    }

}
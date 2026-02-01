using App.DTOs;

namespace App.Repositories.Interfaces
{
        public interface IDashboardRepository
    {
        Task<DashboardOverviewDTO> GetDashboardOverview(int currentYear);
    }
}
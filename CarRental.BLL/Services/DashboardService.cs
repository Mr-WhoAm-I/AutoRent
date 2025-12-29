using CarRental.DAL.Repositories;
using CarRental.Domain.DTO;

namespace CarRental.BLL.Services
{
    public class DashboardService
    {
        private readonly DashboardRepository _repo = new();

        public DashboardStats GetDashboardStats()
        {
            return _repo.GetStats();
        }
    }
}
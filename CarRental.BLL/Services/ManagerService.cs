using CarRental.DAL.Repositories;
using CarRental.Domain.DTO;

namespace CarRental.BLL.Services
{
    public class ManagerService
    {
        private readonly ManagerRepository _repo = new();

        public ManagerDashboardDTO GetDashboardData()
        {
            return _repo.GetData();
        }
    }
}
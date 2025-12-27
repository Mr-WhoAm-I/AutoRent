using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class MaintenanceService
    {
        private readonly MaintenanceRepository _repo;

        public MaintenanceService()
        {
            _repo = new MaintenanceRepository();
        }

        public List<Maintenance> GetActive()
        {
            return _repo.GetActiveMaintenances();
        }

        public List<Maintenance> GetHistory()
        {
            return _repo.GetAllHistory();
        }

        public void SendCarToService(int carId, int mechanicId, string type, string description)
        {
            var maintenance = new Maintenance
            {
                CarId = carId,
                EmployeeId = mechanicId,
                ServiceType = type,
                Description = description,
                DateStart = DateTime.Now
            };
            _repo.AddMaintenance(maintenance);
        }

        public void FinishService(int maintenanceId, decimal cost)
        {
            if (cost < 0) throw new Exception("Стоимость не может быть отрицательной");
            _repo.CompleteMaintenance(maintenanceId, DateTime.Now, cost);
        }
    }
}
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
        public List<Maintenance> GetArchivedMaintenance()
        {
            return _repo.GetArchivedMaintenance();
        }
        public List<Maintenance> GetHistory()
        {
            return _repo.GetAllHistory();
        }

        public List<Maintenance> GetHistoryByCarId(int carId)
        {
            return _repo.GetHistoryByCarId(carId);
        }
        public void RestoreMaintenance(int id) => _repo.RestoreMaintenance(id);

        public void Delete(int id) => _repo.Delete(id);

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

        public void Save(Maintenance m)
        {
            if (m.Id == 0) _repo.AddMaintenance(m);
            else _repo.Update(m);
        }
        public void FinishService(int maintenanceId, decimal cost)
        {
            if (cost < 0) throw new Exception("Стоимость не может быть отрицательной");
            _repo.CompleteMaintenance(maintenanceId, DateTime.Now, cost);
        }
    }
}
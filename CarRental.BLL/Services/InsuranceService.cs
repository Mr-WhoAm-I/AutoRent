using System.Collections.Generic;
using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class InsuranceService
    {
        private readonly InsuranceRepository _repo;

        public InsuranceService()
        {
            _repo = new InsuranceRepository();
        }

        public List<Insurance> GetHistory(int carId)
        {
            return _repo.GetByCarId(carId);
        }
    }
}
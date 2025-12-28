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

        public void Save(Insurance ins)
        {
            if (ins.Id == 0) _repo.Add(ins);
            else _repo.Update(ins);
        }

        public void Delete(int id) => _repo.Delete(id);
    }
}
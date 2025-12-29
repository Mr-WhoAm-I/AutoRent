using System.Collections.Generic;
using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class ReferenceService
    {
        private readonly ReferenceRepository _repository;

        public ReferenceService()
        {
            _repository = new ReferenceRepository();
        }

        public List<Role> GetRoles() => _repository.GetRoles();
        public List<CarBrand> GetBrands() => _repository.GetBrands();
        public List<CarClass> GetClasses() => _repository.GetClasses();
        public List<BodyType> GetBodyTypes() => _repository.GetBodyTypes();
        public List<TransmissionType> GetTransmissions() => _repository.GetTransmissionTypes();
        public List<FuelType> GetFuelTypes() => _repository.GetFuelTypes();
        public List<CarStatus> GetStatuses() => _repository.GetStatuses();
        public List<ReferenceItem> GetItems(string tableName) => _repository.GetItems(tableName);
        public void SaveChanges(string tableName, List<ReferenceItem> items, List<int> deletedIds)
            => _repository.SaveChanges(tableName, items, deletedIds);
        public bool HasDescription(string tableName) => _repository.HasDescription(tableName);
    }
}
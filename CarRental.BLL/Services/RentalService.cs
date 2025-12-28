using System.Collections.Generic;
using CarRental.DAL.Repositories;
using CarRental.Domain.DTO;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class RentalService
    {
        private readonly RentalRepository _repo = new();

        public List<RentalViewItem> GetAllRentals()
        {
            return _repo.GetRentalsView();
        }
    }
}
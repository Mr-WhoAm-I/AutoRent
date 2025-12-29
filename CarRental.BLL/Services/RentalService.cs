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
        public void CreateRental(Rental rental)
        {
            // Здесь можно добавить валидацию (например, что дата начала < дата окончания)
            if (rental.StartDate >= rental.PlannedEndDate)
                throw new Exception("Дата окончания должна быть позже даты начала.");

            _repo.AddRental(rental);
        }

        public Rental? GetRentalById(int id)
        {
            return _repo.GetRentalById(id);
        }

        public void UpdateRental(Rental rental)
        {
            // Здесь тоже можно добавить валидацию
            _repo.UpdateRental(rental);
        }

        public void FinishRental(int rentalId)
        {
            _repo.FinishRental(rentalId, System.DateTime.Now);
        }

        public void UpdateComment(int rentalId,string  comment)
        {
            _repo.UpdateComment(rentalId, comment);
        }

        public decimal CalculateCost(DateTime start, DateTime end, decimal pricePerDay)
            => _repo.CalculatePotentialCost(start, end, pricePerDay);
    }
}
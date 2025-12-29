using CarRental.DAL.Repositories;
using CarRental.Domain.DTO;
using CarRental.Domain.Entities;
using System.Collections.Generic;

namespace CarRental.BLL.Services
{
    public class BookingService
    {
        private readonly BookingRepository _repo = new();

        public List<BookingViewItem> GetAllBookings()
        {
            return _repo.GetBookingsView();
        }

        public Booking? GetBookingById(int id) => _repo.GetBookingById(id);

        public void CreateBooking(Booking booking)
        {
            Validate(booking);
            _repo.AddBooking(booking);
        }

        public void UpdateBooking(Booking booking)
        {
            Validate(booking);
            _repo.UpdateBooking(booking);
        }

        private void Validate(Booking booking)
        {
            if (booking.StartDate >= booking.EndDate)
                throw new Exception("Дата окончания должна быть позже даты начала.");

            if (booking.ClientId <= 0 || booking.CarId <= 0)
                throw new Exception("Не выбраны клиент или автомобиль.");
        }
    }
}
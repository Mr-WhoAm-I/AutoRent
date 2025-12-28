using System.Collections.Generic;
using CarRental.DAL.Repositories;
using CarRental.Domain.DTO;

namespace CarRental.BLL.Services
{
    public class BookingService
    {
        private readonly BookingRepository _repo = new();

        public List<BookingViewItem> GetAllBookings()
        {
            return _repo.GetBookingsView();
        }
    }
}
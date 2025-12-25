using System;

namespace CarRental.Domain.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Comment { get; set; }

        public string ClientFullName { get; set; } = string.Empty;
        public string CarDisplayName { get; set; } = string.Empty;
    }

    public class Rental
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }
        public int EmployeeId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        public decimal PriceAtRentalMoment { get; set; }
        public decimal? TotalPrice { get; set; }

        public string ClientSurname { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
    }

    public class Payment
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
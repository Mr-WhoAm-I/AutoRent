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

        public Car Car { get; set; } = new Car();

        public string CarDisplayName => $"{Car.BrandName} {Car.Model}";
        public string CarSubInfo => $"{Car.ClassName} • {Car.PlateNumber}";
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
        public string? Review { get; set; }
        public Car Car { get; set; } = new Car();

        public string CarDisplayName => $"{Car.BrandName} {Car.Model}";
        public string CarSubInfo => $"{Car.ClassName} • {Car.PlateNumber}";
    }

    public class Payment
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class CalendarItem
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Type { get; set; } = string.Empty; // "Rental", "Booking", "Maintenance"
        public string TooltipText { get; set; } = string.Empty; // "В аренде у Иванов И.И."
    }
}
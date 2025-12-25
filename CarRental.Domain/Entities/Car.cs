namespace CarRental.Domain.Entities
{
    public class Car
    {
        public int Id { get; set; }

        public int BrandId { get; set; }
        public int ClassId { get; set; }
        public int BodyTypeId { get; set; }
        public int TransmissionId { get; set; }
        public int FuelId { get; set; }
        public int StatusId { get; set; }

        public string Model { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Mileage { get; set; }
        public decimal PricePerDay { get; set; }

        public string? ImagePath { get; set; }

        // --- Свойства для отображения ---
        public string BrandName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string TransmissionName { get; set; } = string.Empty;
        public string FuelName { get; set; } = string.Empty;
        public string BodyTypeName { get; set; } = string.Empty;

        public string DisplayName => $"{BrandName} {Model} ({Year})";
    }
}
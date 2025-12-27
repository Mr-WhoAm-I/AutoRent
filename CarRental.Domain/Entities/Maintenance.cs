namespace CarRental.Domain.Entities
{
    public class Maintenance
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public int EmployeeId { get; set; } // ID Механика

        public string ServiceType { get; set; } = string.Empty; // ТО, Ремонт, Шиномонтаж
        public string? Description { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; } // Null, если еще в работе
        public decimal? Cost { get; set; }     // Null, пока не закончили

        // Для отображения (JOIN)
        public string CarName { get; set; } = string.Empty; // Марка + Модель
        public string PlateNumber { get; set; } = string.Empty;
        public string MechanicName { get; set; } = string.Empty; // Фамилия механика
    }
}
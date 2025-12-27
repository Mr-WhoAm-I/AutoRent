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
        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string BrandAndModel => $"{BrandName} {Model}";
        public string StatusColorHex
        {
            get
            {
                if (StatusName == "Свободен") return "#00C853"; // Зеленый
                if (StatusName.Contains("ремонт") || StatusName.Contains("обслуживание")) return "#FF9800"; // Оранжевый
                return "#6366F1"; // Синий (Аренда, Бронь)
            }
        }
        public string StatusBgHex
        {
            get
            {
                if (StatusName == "Свободен") return "#E8F5E9"; // Светло-зеленый
                if (StatusName.Contains("ремонт") || StatusName.Contains("обслуживание")) return "#FFF3E0"; // Светло-оранжевый
                return "#EEF2FF"; // Светло-синий
            }
        }
        // Логика цвета ИНДИКАТОРА СТРАХОВКИ
        public string InsuranceIndicatorColor
        {
            get
            {
                if (InsuranceExpiryDate == null) return "#CCC"; // Нет страховки
                var days = (InsuranceExpiryDate.Value - DateTime.Now).TotalDays;
                if (days < 7) return "#F44336"; // Красный (< недели)
                if (days < 30) return "#FF9800"; // Оранжевый (< месяца)
                return "#CCC"; // Серый (все ок)
            }
        }

        // Логика цвета ИНДИКАТОРА ОБСЛУЖИВАНИЯ
        public string MaintenanceIndicatorColor
        {
            get
            {
                if (NextMaintenanceDate == null) return "#CCC";
                var days = (NextMaintenanceDate.Value - DateTime.Now).TotalDays;
                if (days <= 7 && days >= 0) return "#FF9800"; // Скоро (< недели)
                return "#CCC";
            }
        }
    }
}
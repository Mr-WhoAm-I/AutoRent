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
        public string MechanicPosition { get; set; } = string.Empty;

        // Свойство для удобного вывода в гриде: "Иванов И.И. (Главный механик)"
        public string MechanicFullName => $"{MechanicName} ({MechanicPosition})";

        public string Period
        {
            get
            {
                string start = DateStart.ToString("d");
                if (DateEnd.HasValue)
                    return $"{start} — {DateEnd.Value:d}";

                return $"{start} (В процессе)";
            }
        }

        // 2. Флаг: Это событие в будущем?
        public bool IsFuture => DateStart.Date > DateTime.Now.Date;
    }
}
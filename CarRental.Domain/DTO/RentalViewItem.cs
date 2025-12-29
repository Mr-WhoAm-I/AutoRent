using System;

namespace CarRental.Domain.DTO
{
    public class RentalViewItem
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime DateEndPlanned { get; set; }
        public DateTime? DateEndActual { get; set; }

        // Логика: если вернули, показываем фактическую, иначе плановую
        public DateTime DisplayEndDate => DateEndActual ?? DateEndPlanned;

        public string Status { get; set; } = string.Empty;

        // Клиент
        public string ClientFullName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;

        // Авто
        public string CarName { get; set; } = string.Empty; // Марка + Модель
        public string CarPlate { get; set; } = string.Empty;
        public string? CarPhoto { get; set; }

        // Сотрудник
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeePosition { get; set; } = string.Empty;

        // Финансы
        public decimal TotalCost { get; set; }
        public decimal Paid { get; set; }
        public decimal Debt { get; set; }

        // Цвет долга для UI
        public string DebtColor => Debt > 0 ? "#F44336" : "#00C853";
        public bool HasDebt => Debt > 0;
    }
}
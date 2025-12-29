using System;

namespace CarRental.Domain.DTO
{
    // 1. Клиенты (Без изменений)
    public class ClientReportItem
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RentalsCount { get; set; }
    }

    // 2. Авто (Без изменений)
    public class CarPerformanceItem
    {
        public string CarName { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit => Revenue - Expenses;
        public decimal ProfitIndex => Expenses == 0 ? Revenue : Math.Round(Revenue / Expenses, 2);
        public string Status => ProfitIndex >= 1.0m ? "Выгоден" : "Убыточен";
    }

    // 3. НОВЫЙ DTO для Детального Финансового отчета
    public class PaymentReportItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // Аванс, Доплата...
        public string CarInfo { get; set; } = string.Empty; // Авто, за которое платят
    }
}
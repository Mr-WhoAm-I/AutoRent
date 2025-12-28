using System;

namespace CarRental.Domain.DTO
{
    public class BookingViewItem
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime DateCreated { get; set; }

        public string Status { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        // Клиент
        public string ClientFullName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string ClientInfo => $"{ClientFullName}\n{ClientPhone}";

        // Авто
        public string CarName { get; set; } = string.Empty;
        public string CarPlate { get; set; } = string.Empty;
        public string CarInfo => $"{CarName}\n{CarPlate}";

        // Форматирование
        public string Period => $"{DateStart:dd.MM.yyyy} — {DateEnd:dd.MM.yyyy}";

        // Цвет статуса
        public string StatusColor => Status == "Истекла" ? "#999" : (Status == "Активна" ? "#00C853" : "#6366F1");
    }
}
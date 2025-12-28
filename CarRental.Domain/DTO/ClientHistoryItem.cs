namespace CarRental.Domain.DTO
{
    public class ClientHistoryItem
    {
        public int Id { get; set; } // ID Аренды или Брони
        public int CarId { get; set; }
        public string Type { get; set; } // "Rental" или "Booking"

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } // Фактическая или Плановая

        public string CarTitle { get; set; } = string.Empty; // BMW X5
        public string CarDetails { get; set; } = string.Empty; // Внедорожник • 1234 XX-7

        public decimal? Cost { get; set; }
        public string? Note { get; set; } // Отзыв или Комментарий

        public string Status { get; set; } // "Активна", "Завершена", "Бронь"

        // Для сортировки (Активные -> Бронь -> Завершенные)
        public int SortOrder { get; set; }
    }
}
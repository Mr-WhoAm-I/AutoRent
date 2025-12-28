using System;

namespace CarRental.Domain.Entities
{
    public class Insurance
    {
        public int Id { get; set; }
        public int CarId { get; set; }

        public string PolicyNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // ОСАГО / КАСКО

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsArchived { get; set; }
        public decimal Cost { get; set; }

        // Логика для отображения статуса в таблице
        public string Status
        {
            get
            {
                if (DateTime.Now > EndDate) return "Истекла";
                if (DateTime.Now < StartDate) return "Будущая";
                return "Активна";
            }
        }
        public bool IsFuture => StartDate.Date > DateTime.Now.Date;
    }
}
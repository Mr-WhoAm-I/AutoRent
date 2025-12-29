using System;

namespace CarRental.Domain.Entities
{
    public class Fine
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
    }
}
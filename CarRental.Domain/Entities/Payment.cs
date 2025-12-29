using System;

namespace CarRental.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }
        public string Type { get; set; } = "Полная оплата"; // Аванс, Доплата, Штраф
    }
}
using System.Collections.Generic;
using CarRental.Domain.Entities;

namespace CarRental.Domain.DTO
{
    public class ManagerDashboardDTO
    {
        // 1. Ожидаемые выдачи на сегодня
        public List<RentalViewItem> IssuesToday { get; set; } = new();

        // 2. Ожидаемые возвраты сегодня
        public List<RentalViewItem> ReturnsToday { get; set; } = new();

        // 3. Просроченные (Должны были вернуть раньше, но не вернули)
        public List<RentalViewItem> OverdueRentals { get; set; } = new();

        // 4. Машины в ремонте прямо сейчас
        public List<Maintenance> CarsInService { get; set; } = new();

        // Счетчики для заголовков
        public int IssuesCount => IssuesToday.Count;
        public int ReturnsCount => ReturnsToday.Count;
        public int OverdueCount => OverdueRentals.Count;
        public int ServiceCount => CarsInService.Count;
    }
}
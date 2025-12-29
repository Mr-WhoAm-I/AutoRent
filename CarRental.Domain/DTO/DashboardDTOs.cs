using System;
using System.Collections.Generic;

namespace CarRental.Domain.DTO
{
    public class DashboardStats
    {
        // 1. Карточки сверху
        public decimal MonthlyIncome { get; set; }    // Доходы за месяц
        public decimal MonthlyExpenses { get; set; }  // Расходы за месяц
        public int ActiveRentalsCount { get; set; }   // Активные (в т.ч. просроченные)
        public int TotalCars { get; set; }            // Всего машин
        public int CarsInRepair { get; set; }         // Машин в ремонте

        // 2. График (По дням месяца)
        public List<ChartValue> IncomeChart { get; set; } = new();
        public List<ChartValue> ExpenseChart { get; set; } = new();

        // 3. Диаграмма статусов
        public List<ChartValue> CarStatusChart { get; set; } = new();
    }

    public class ChartValue
    {
        public string Label { get; set; } // Например, "01.05" или "Свободен"
        public double Value { get; set; } // Сумма или Количество
    }
}
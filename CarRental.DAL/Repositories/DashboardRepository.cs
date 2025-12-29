using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CarRental.Domain.DTO;

namespace CarRental.DAL.Repositories
{
    public class DashboardRepository : BaseRepository
    {
        public DashboardStats GetStats()
        {
            var stats = new DashboardStats();
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            using var conn = GetConnection();
            conn.Open();

            // 1. АКТИВНЫЕ АРЕНДЫ (Где не проставлена фактическая дата возврата)
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Аренда WHERE ДатаОкончанияФактическая IS NULL", conn))
            {
                stats.ActiveRentalsCount = (int)cmd.ExecuteScalar();
            }

            // 2. ВСЕГО АВТО
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Автомобиль WHERE IDСтатуса != 6", conn)) // Исключаем списанные
            {
                stats.TotalCars = (int)cmd.ExecuteScalar();
            }

            // 3. АВТО В РЕМОНТЕ
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Автомобиль WHERE IDСтатуса = 5", conn))
            {
                stats.CarsInRepair = (int)cmd.ExecuteScalar();
            }

            // 4. ДОХОДЫ (Платежи за текущий месяц)
            string sqlIncome = @"
                SELECT DAY(ДатаПлатежа) as D, SUM(Сумма) as S 
                FROM Платеж 
                WHERE ДатаПлатежа BETWEEN @Start AND @End 
                GROUP BY DAY(ДатаПлатежа) ORDER BY D";

            using (var cmd = new SqlCommand(sqlIncome, conn))
            {
                cmd.Parameters.AddWithValue("@Start", startOfMonth);
                cmd.Parameters.AddWithValue("@End", endOfMonth);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    decimal val = (decimal)r["S"];
                    stats.MonthlyIncome += val;
                    stats.IncomeChart.Add(new ChartValue { Label = r["D"].ToString(), Value = (double)val });
                }
            }

            // 5. РАСХОДЫ (Страховки + ТО за текущий месяц)
            // Объединяем две таблицы через UNION ALL
            string sqlExpenses = @"
                SELECT DAY(Dt) as D, SUM(Cost) as S FROM (
                    SELECT ДатаНачала as Dt, Стоимость as Cost FROM Страховка WHERE ДатаНачала BETWEEN @Start AND @End
                    UNION ALL
                    SELECT ДатаНачала as Dt, ISNULL(Стоимость, 0) as Cost FROM Обслуживание WHERE ДатаНачала BETWEEN @Start AND @End
                ) as Combined
                GROUP BY DAY(Dt) ORDER BY D";

            using (var cmd = new SqlCommand(sqlExpenses, conn))
            {
                cmd.Parameters.AddWithValue("@Start", startOfMonth);
                cmd.Parameters.AddWithValue("@End", endOfMonth);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    decimal val = (decimal)r["S"];
                    stats.MonthlyExpenses += val;
                    stats.ExpenseChart.Add(new ChartValue { Label = r["D"].ToString(), Value = (double)val });
                }
            }

            // 6. ДИАГРАММА СТАТУСОВ АВТО
            string sqlStatuses = @"
                SELECT s.Название, COUNT(a.ID) as Cnt
                FROM Автомобиль a
                JOIN СтатусАвто s ON a.IDСтатуса = s.ID
                WHERE s.Название != 'Списан'
                GROUP BY s.Название";

            using (var cmd = new SqlCommand(sqlStatuses, conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    stats.CarStatusChart.Add(new ChartValue
                    {
                        Label = r["Название"].ToString(),
                        Value = (int)r["Cnt"]
                    });
                }
            }

            return stats;
        }
    }
}
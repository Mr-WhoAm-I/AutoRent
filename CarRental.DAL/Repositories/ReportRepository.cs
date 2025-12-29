using System;
using System.Collections.Generic;
using System.Linq; // Важно для Contains
using Microsoft.Data.SqlClient;
using CarRental.Domain.DTO;

namespace CarRental.DAL.Repositories
{
    public class ReportRepository : BaseRepository
    {
        // 1. КЛИЕНТЫ (Без изменений)
        public List<ClientReportItem> GetClientsReport()
        {
            // ... (оставьте старый код) ...
            var list = new List<ClientReportItem>();
            string sql = @"
                SELECT 
                    Фамилия + ' ' + Имя + ' ' + ISNULL(Отчество, '') AS FIO,
                    Телефон, Почта,
                    (SELECT COUNT(*) FROM Аренда WHERE IDКлиента = K.ID) as Cnt
                FROM Клиент K WHERE ВАрхиве = 0 ORDER BY Фамилия";

            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ClientReportItem
                {
                    FullName = reader["FIO"].ToString() ?? "",
                    Phone = reader["Телефон"].ToString() ?? "",
                    Email = reader["Почта"] as string ?? "Нет",
                    RentalsCount = (int)reader["Cnt"]
                });
            }
            return list;
        }

        // 2. АВТОМОБИЛИ (Множественный выбор)
        public List<CarPerformanceItem> GetCarPerformance(List<int> classIds)
        {
            var list = new List<CarPerformanceItem>();

            // Формируем SQL фильтр: IDКласса IN (1, 2, 5)
            string filter = "";
            if (classIds != null && classIds.Any())
            {
                string ids = string.Join(",", classIds);
                filter = $"AND A.IDКласса IN ({ids})";
            }

            string sql = $@"
                SELECT 
                    M.Название + ' ' + A.Модель AS Car,
                    A.ГосНомер,
                    K.Название AS ClassName,
                    ISNULL((SELECT SUM(P.Сумма) FROM Платеж P JOIN Аренда R ON P.IDАренды = R.ID WHERE R.IDАвтомобиля = A.ID), 0) AS Rev,
                    ISNULL((SELECT SUM(S.Стоимость) FROM Страховка S WHERE S.IDАвтомобиля = A.ID), 0) +
                    ISNULL((SELECT SUM(O.Стоимость) FROM Обслуживание O WHERE O.IDАвтомобиля = A.ID), 0) AS Exp
                FROM Автомобиль A
                JOIN Марка M ON A.IDМарки = M.ID
                JOIN КлассАвтомобиля K ON A.IDКласса = K.ID
                WHERE A.IDСтатуса != 6 {filter}
                ORDER BY Rev DESC";

            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            // Параметры не нужны, мы вставили ID прямо в строку (это безопасно, т.к. int)

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CarPerformanceItem
                {
                    CarName = reader["Car"].ToString() ?? "",
                    PlateNumber = reader["ГосНомер"].ToString() ?? "",
                    ClassName = reader["ClassName"].ToString() ?? "",
                    Revenue = (decimal)reader["Rev"],
                    Expenses = (decimal)reader["Exp"]
                });
            }
            return list;
        }

        // 3. ФИНАНСЫ (Детальный список)
        public List<PaymentReportItem> GetPaymentDetails(DateTime start, DateTime end)
        {
            var list = new List<PaymentReportItem>();
            string sql = @"
                SELECT 
                    P.ID, P.ДатаПлатежа, P.Сумма, P.ТипПлатежа,
                    M.Название + ' ' + A.Модель + ' (' + A.ГосНомер + ')' AS CarInfo
                FROM Платеж P
                JOIN Аренда R ON P.IDАренды = R.ID
                JOIN Автомобиль A ON R.IDАвтомобиля = A.ID
                JOIN Марка M ON A.IDМарки = M.ID
                WHERE P.ДатаПлатежа BETWEEN @Start AND @End
                ORDER BY P.ДатаПлатежа"; // Сортировка по дате обязательна для группировки

            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PaymentReportItem
                {
                    Id = (int)reader["ID"],
                    Date = (DateTime)reader["ДатаПлатежа"],
                    Amount = (decimal)reader["Сумма"],
                    Type = reader["ТипПлатежа"].ToString() ?? "",
                    CarInfo = reader["CarInfo"].ToString() ?? ""
                });
            }
            return list;
        }
    }
}
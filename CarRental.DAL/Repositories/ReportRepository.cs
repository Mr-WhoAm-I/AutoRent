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
        // 1. КЛИЕНТЫ (Оптимизировано через View v_Clients_Report)
        public List<ClientReportItem> GetClientsReport()
        {
            var list = new List<ClientReportItem>();
            string sql = "SELECT * FROM Отчет_По_Клиентам ORDER BY FIO";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ClientReportItem
                {
                    FullName = reader["FIO"].ToString() ?? "",
                    Phone = reader["Телефон"].ToString() ?? "",
                    Email = reader["Почта"].ToString() ?? "",
                    RentalsCount = (int)reader["ВсегоАренд"]
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
        // 3. ФИНАНСЫ (Детальный список через MSTVF-функцию)
        public List<PaymentReportItem> GetPaymentDetails(DateTime start, DateTime end)
        {
            var list = new List<PaymentReportItem>();

            // Вызываем нашу новую функцию
            string sql = "SELECT * FROM fn_Финансовый_Отчет(@Start, @End) ORDER BY Дата DESC";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PaymentReportItem
                {
                    Id = (int)reader["ID"],
                    Date = (DateTime)reader["Дата"],
                    Amount = (decimal)reader["Сумма"],
                    // В функции мы назвали колонку 'ТипОперации', а в классе свойство Type
                    Type = reader["ТипОперации"].ToString() ?? "",
                    // В функции 'АвтоИнфо' -> CarInfo
                    CarInfo = reader["АвтоИнфо"].ToString() ?? ""
                });
            }
            return list;
        }
    }
}
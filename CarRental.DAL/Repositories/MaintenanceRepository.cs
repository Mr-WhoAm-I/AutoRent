using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;
using System.Collections.Generic;
using System;

namespace CarRental.DAL.Repositories
{
    public class MaintenanceRepository : BaseRepository
    {
        // 1. ЧТЕНИЕ ДАННЫХ ЧЕРЕЗ ПРЕДСТАВЛЕНИЕ (VIEW)
        // Вместо дублирования JOIN-ов мы просто фильтруем View

        public List<Maintenance> GetActiveMaintenances()
        {
            return GetMaintenances("WHERE ДатаОкончания IS NULL AND ВАрхиве = 0");
        }

        public List<Maintenance> GetArchivedMaintenance()
        {
            return GetMaintenances("WHERE ВАрхиве = 1 ORDER BY ДатаНачала DESC");
        }

        public List<Maintenance> GetAllHistory()
        {
            return GetMaintenances("WHERE ВАрхиве = 0 ORDER BY ДатаНачала DESC");
        }

        public List<Maintenance> GetHistoryByCarId(int carId)
        {
            return GetMaintenances($"WHERE IDАвтомобиля = {carId} AND ВАрхиве = 0 ORDER BY ДатаНачала DESC");
        }

        // Универсальный метод чтения из v_Maintenance_Journal
        private List<Maintenance> GetMaintenances(string whereClause = "")
        {
            var list = new List<Maintenance>();
            string sql = $"SELECT * FROM Журнал_Обслуживаний {whereClause}";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Maintenance
                {
                    Id = (int)reader["ID"],
                    CarId = (int)reader["IDАвтомобиля"],
                    EmployeeId = (int)reader["IDСотрудника"],
                    ServiceType = reader["ТипОбслуживания"].ToString() ?? "",
                    Description = reader["Описание"] as string,
                    DateStart = (DateTime)reader["ДатаНачала"],
                    DateEnd = reader["ДатаОкончания"] as DateTime?,
                    Cost = reader["Стоимость"] as decimal?,
                    IsArchived = (bool)reader["ВАрхиве"],

                    // Данные из VIEW
                    CarName = reader["АвтоНазвание"].ToString() ?? "",
                    PlateNumber = reader["ГосНомер"].ToString() ?? "",
                    MechanicName = reader["МеханикФИО"].ToString() ?? "",
                    MechanicPosition = reader["Должность"].ToString() ?? ""
                });
            }
            return list;
        }

        // 2. ОТПРАВКА НА СЕРВИС ЧЕРЕЗ ХРАНИМУЮ ПРОЦЕДУРУ
        public void AddMaintenance(Maintenance m)
        {
            using var conn = GetConnection();
            conn.Open();

            // Вызываем процедуру вместо ручной транзакции
            using var cmd = new SqlCommand("sp_ДобавитьМашинуНаОбслуживание", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CarId", m.CarId);
            cmd.Parameters.AddWithValue("@EmployeeId", m.EmployeeId);
            cmd.Parameters.AddWithValue("@ServiceType", m.ServiceType);
            cmd.Parameters.AddWithValue("@Description", m.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", m.DateStart);

            cmd.ExecuteNonQuery();
        }

        // === ОСТАЛЬНЫЕ МЕТОДЫ (Оставляем как есть или оптимизируем позже) ===

        public void RestoreMaintenance(int id)
        {
            string sql = "UPDATE Обслуживание SET ВАрхиве = 0 WHERE ID = @Id";
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            string sql = @"
                UPDATE Обслуживание 
                SET ВАрхиве = 1, 
                    ДатаОкончания = ISNULL(ДатаОкончания, GETDATE()) 
                WHERE ID = @Id";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public void Update(Maintenance m)
        {
            string sql = @"
                UPDATE Обслуживание
                SET IDСотрудника = @EmpId,
                    ТипОбслуживания = @Type,
                    Описание = @Desc,
                    ДатаНачала = @Start,
                    ДатаОкончания = @End,
                    Стоимость = @Cost
                WHERE ID = @Id";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", m.Id);
            cmd.Parameters.AddWithValue("@EmpId", m.EmployeeId);
            cmd.Parameters.AddWithValue("@Type", m.ServiceType);
            cmd.Parameters.AddWithValue("@Desc", m.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Start", m.DateStart);
            cmd.Parameters.AddWithValue("@End", m.DateEnd ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Cost", m.Cost ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        // Завершение ремонта (пока оставим старую логику с транзакцией C#, 
        // так как мы не делали под неё отдельную процедуру в этом шаге, но она простая)
        public void CompleteMaintenance(int maintenanceId, DateTime endDate, decimal cost)
        {
            using var conn = GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                string sqlUpdateM = "UPDATE Обслуживание SET ДатаОкончания = @End, Стоимость = @Cost WHERE ID = @Id";
                using var cmd1 = new SqlCommand(sqlUpdateM, conn, transaction);
                cmd1.Parameters.AddWithValue("@End", endDate);
                cmd1.Parameters.AddWithValue("@Cost", cost);
                cmd1.Parameters.AddWithValue("@Id", maintenanceId);
                cmd1.ExecuteNonQuery();

                string sqlGetCar = "SELECT IDАвтомобиля FROM Обслуживание WHERE ID = @Id";
                using var cmdGet = new SqlCommand(sqlGetCar, conn, transaction);
                cmdGet.Parameters.AddWithValue("@Id", maintenanceId);
                int carId = (int)cmdGet.ExecuteScalar();

                string sqlUpdateCar = "UPDATE Автомобиль SET IDСтатуса = 1 WHERE ID = @CarId";
                using var cmd2 = new SqlCommand(sqlUpdateCar, conn, transaction);
                cmd2.Parameters.AddWithValue("@CarId", carId);
                cmd2.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<CalendarItem> GetCalendarItems(int carId)
        {
            var list = new List<CalendarItem>();
            string sql = @"
                SELECT ДатаНачала, 
                       ISNULL(ДатаОкончания, DATEADD(year, 100, GETDATE())) as Конец, 
                       ТипОбслуживания
                FROM Обслуживание
                WHERE IDАвтомобиля = @CarId 
                  AND ВАрхиве = 0";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CarId", carId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CalendarItem
                {
                    Start = (DateTime)reader["ДатаНачала"],
                    End = (DateTime)reader["Конец"],
                    Type = "Maintenance",
                    TooltipText = $"Обслуживание: {reader["ТипОбслуживания"]}"
                });
            }
            return list;
        }

        public List<int> GetOccupiedCarIds(DateTime start, DateTime end)
        {
            var ids = new List<int>();
            string sql = @"
                SELECT DISTINCT IDАвтомобиля 
                FROM Обслуживание 
                WHERE ДатаНачала <= @End 
                  AND (ДатаОкончания IS NULL OR ДатаОкончания >= @Start)";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ids.Add((int)reader["IDАвтомобиля"]);
            }
            return ids;
        }
    }
}
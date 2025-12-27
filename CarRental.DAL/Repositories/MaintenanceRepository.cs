using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;
using System.Collections.Generic;
using System;

namespace CarRental.DAL.Repositories
{
    public class MaintenanceRepository : BaseRepository
    {
        // 1. Получить активные ремонты (где дата окончания NULL)
        public List<Maintenance> GetActiveMaintenances()
        {
            return GetMaintenances("WHERE o.ДатаОкончания IS NULL");
        }

        // 2. Получить историю (все ремонты)
        public List<Maintenance> GetAllHistory()
        {
            return GetMaintenances("ORDER BY o.ДатаНачала DESC");
        }

        // Вспомогательный метод, чтобы не дублировать код чтения
        private List<Maintenance> GetMaintenances(string whereClause = "")
        {
            var list = new List<Maintenance>();
            string sql = $@"
                SELECT o.ID, o.IDАвтомобиля, o.IDСотрудника,
                       o.ТипОбслуживания, o.Описание, o.ДатаНачала, o.ДатаОкончания, o.Стоимость,
                       a.Модель, m.Название as Марка, a.ГосНомер,
                       s.Фамилия, s.Имя
                  FROM Обслуживание o
                    INNER JOIN Автомобиль a ON o.IDАвтомобиля = a.ID
                    INNER JOIN Марка m ON a.IDМарки = m.ID
                    INNER JOIN Сотрудник s ON o.IDСотрудника = s.ID
                  {whereClause}";

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

                    CarName = $"{reader["Марка"]} {reader["Модель"]}",
                    PlateNumber = reader["ГосНомер"].ToString() ?? "",
                    MechanicName = $"{reader["Фамилия"]} {reader["Имя"]}"
                });
            }
            return list;
        }

        // 3. Добавить запись о ремонте (Отправить в сервис)
        public void AddMaintenance(Maintenance m)
        {
            using var conn = GetConnection();
            conn.Open();

            // Транзакция: Добавить запись + Сменить статус авто
            using var transaction = conn.BeginTransaction();
            try
            {
                // А. Вставка записи
                string sqlInsert = @"
                    INSERT INTO Обслуживание (IDАвтомобиля, IDСотрудника, ТипОбслуживания, Описание, ДатаНачала)
                    VALUES (@CarId, @EmpId, @Type, @Desc, @Date)";

                using var cmd1 = new SqlCommand(sqlInsert, conn, transaction);
                cmd1.Parameters.AddWithValue("@CarId", m.CarId);
                cmd1.Parameters.AddWithValue("@EmpId", m.EmployeeId);
                cmd1.Parameters.AddWithValue("@Type", m.ServiceType);
                cmd1.Parameters.AddWithValue("@Desc", m.Description ?? (object)DBNull.Value);
                cmd1.Parameters.AddWithValue("@Date", m.DateStart);
                cmd1.ExecuteNonQuery();

                // Б. Смена статуса авто на "В ремонте" (ID = 5) или "На обслуживании" (ID = 4)
                // Допустим, мы ставим 5 (В ремонте)
                string sqlUpdate = "UPDATE Автомобиль SET IDСтатуса = 5 WHERE ID = @CarId";
                using var cmd2 = new SqlCommand(sqlUpdate, conn, transaction);
                cmd2.Parameters.AddWithValue("@CarId", m.CarId);
                cmd2.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // 4. Завершить ремонт
        public void CompleteMaintenance(int maintenanceId, DateTime endDate, decimal cost)
        {
            using var conn = GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                // А. Обновляем запись (ставим дату и цену)
                string sqlUpdateM = @"
                    UPDATE Обслуживание 
                      SET ДатаОкончания = @End, Стоимость = @Cost 
                      WHERE ID = @Id";

                using var cmd1 = new SqlCommand(sqlUpdateM, conn, transaction);
                cmd1.Parameters.AddWithValue("@End", endDate);
                cmd1.Parameters.AddWithValue("@Cost", cost);
                cmd1.Parameters.AddWithValue("@Id", maintenanceId);
                cmd1.ExecuteNonQuery();

                // Б. Узнаем ID машины
                // (Тут можно было бы передать CarId параметром, но надежнее найти его в БД)
                string sqlGetCar = "SELECT IDАвтомобиля FROM Обслуживание WHERE ID = @Id";
                using var cmdGet = new SqlCommand(sqlGetCar, conn, transaction);
                cmdGet.Parameters.AddWithValue("@Id", maintenanceId);
                int carId = (int)cmdGet.ExecuteScalar();

                // В. Меняем статус авто на "Свободен" (ID = 1)
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
    }
}
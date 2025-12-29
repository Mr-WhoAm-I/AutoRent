using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CarRental.Domain.DTO;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class ManagerRepository : BaseRepository
    {
        public ManagerDashboardDTO GetData()
        {
            var data = new ManagerDashboardDTO();

            using var conn = GetConnection();
            conn.Open();

            // Вызываем нашу новую процедуру
            using var cmd = new SqlCommand("sp_GetManagerDashboardData", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            using var r = cmd.ExecuteReader();

            // 1. Читаем первый набор данных (Задачи из #TempTable)
            while (r.Read())
            {
                var type = r["TaskType"].ToString();
                var item = new RentalViewItem
                {
                    Id = (int)r["RentalID"],
                    CarId = (int)r["CarID"],
                    ClientId = (int)r["ClientID"],
                    DateStart = (DateTime)r["StartDate"],
                    DateEndPlanned = (DateTime)r["EndDate"],
                    ClientFullName = r["ClientName"].ToString(),
                    ClientPhone = r["ClientPhone"].ToString(),
                    CarName = r["CarName"].ToString(),
                    CarPlate = r["CarPlate"].ToString(),
                    Status = r["Status"].ToString()
                };

                // Распределяем по спискам в зависимости от типа, который вернула процедура
                switch (type)
                {
                    case "Issue": data.IssuesToday.Add(item); break;
                    case "Return": data.ReturnsToday.Add(item); break;
                    case "Overdue": data.OverdueRentals.Add(item); break;
                }
            }

            // 2. Переходим ко второму набору данных (Сервис)
            if (r.NextResult())
            {
                while (r.Read())
                {
                    data.CarsInService.Add(new Maintenance
                    {
                        Id = (int)r["ID"],
                        ServiceType = r["ТипОбслуживания"].ToString() ?? "",
                        CarName = $"{r["Марка"]} {r["Модель"]}",
                        PlateNumber = r["ГосНомер"].ToString() ?? ""
                    });
                }
            }

            return data;
        }
    }
}
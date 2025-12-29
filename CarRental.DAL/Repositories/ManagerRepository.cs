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
            var today = DateTime.Today;

            using var conn = GetConnection();
            conn.Open();

            // 1. ПОЛУЧАЕМ АРЕНДЫ (Используем View для удобства)
            string sqlRentals = @"SELECT * FROM Представление_Аренды 
                                  WHERE (CAST(ДатаНачала AS DATE) = @Today) -- Выдача сегодня
                                     OR (CAST(ДатаОкончанияПлановая AS DATE) <= @Today AND ДатаОкончанияФактическая IS NULL) -- Возврат сегодня или просрочка";

            using (var cmd = new SqlCommand(sqlRentals, conn))
            {
                cmd.Parameters.AddWithValue("@Today", today);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var item = new RentalViewItem
                    {
                        Id = (int)r["ID"],
                        CarId = (int)r["IDАвтомобиля"],
                        ClientId = (int)r["IDКлиента"],
                        DateStart = (DateTime)r["ДатаНачала"],
                        DateEndPlanned = (DateTime)r["ДатаОкончанияПлановая"],
                        ClientFullName = r["КлиентФИО"].ToString(),
                        ClientPhone = r["КлиентТелефон"].ToString(),
                        CarName = r["АвтоНазвание"].ToString(),
                        CarPlate = r["ГосНомер"].ToString(),
                        Status = r["СтатусАренды"].ToString()
                    };

                    // Распределяем по спискам
                    if (item.DateStart.Date == today)
                        data.IssuesToday.Add(item); // Выдача сегодня

                    if (item.DateEndPlanned.Date == today && item.DateEndActual == null)
                        data.ReturnsToday.Add(item); // Возврат сегодня

                    if (item.DateEndPlanned.Date < today && item.DateEndActual == null)
                        data.OverdueRentals.Add(item); // Просрочено
                }
            }

            // 2. ПОЛУЧАЕМ АВТО В РЕМОНТЕ
            string sqlService = @"
                SELECT m.ID, m.ТипОбслуживания, m.ДатаОкончания, 
                       a.Модель, mk.Название as Марка, a.ГосНомер
                FROM Обслуживание m
                JOIN Автомобиль a ON m.IDАвтомобиля = a.ID
                JOIN Марка mk ON a.IDМарки = mk.ID
                WHERE m.ДатаОкончания IS NULL"; // Активные ремонты

            using (var cmd = new SqlCommand(sqlService, conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    data.CarsInService.Add(new Maintenance
                    {
                        Id = (int)r["ID"],
                        ServiceType = r["ТипОбслуживания"].ToString() ?? "",
                        CarName = $"{r["Марка"]} {r["Модель"]}",
                        PlateNumber = r["ГосНомер"].ToString() ?? ""
                        // Остальные поля можно не читать, для виджета хватит этого
                    });
                }
            }

            return data;
        }
    }
}
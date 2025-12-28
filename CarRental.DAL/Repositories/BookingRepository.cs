using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class BookingRepository : BaseRepository
    {
        // 1. Данные для календаря (с ФИО клиента)
        public List<CalendarItem> GetCalendarItems(int carId)
        {
            var list = new List<CalendarItem>();
            string sql = @"
                SELECT b.ДатаНачала, b.ДатаОкончания, k.Фамилия, k.Имя
                FROM Бронирование b
                JOIN Клиент k ON b.IDКлиента = k.ID
                WHERE b.IDАвтомобиля = @CarId";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CarId", carId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string clientName = $"{reader["Фамилия"]} {reader["Имя"]}";
                list.Add(new CalendarItem
                {
                    Start = (DateTime)reader["ДатаНачала"],
                    End = (DateTime)reader["ДатаОкончания"],
                    Type = "Booking", // Тип для календаря
                    TooltipText = $"Забронировал: {clientName}"
                });
            }
            return list;
        }

        // 2. ID занятых машин (для фильтра поиска)
        public List<int> GetOccupiedCarIds(DateTime start, DateTime end)
        {
            var ids = new List<int>();
            string sql = @"
                SELECT DISTINCT IDАвтомобиля 
                FROM Бронирование 
                WHERE ДатаНачала <= @End AND ДатаОкончания >= @Start";

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
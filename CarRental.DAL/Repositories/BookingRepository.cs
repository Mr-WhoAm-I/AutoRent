using CarRental.Domain.DTO;
using CarRental.Domain.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

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

        public List<Booking> GetByClientId(int clientId)
        {
            var list = new List<Booking>();
            string sql = @"
                SELECT b.ID, b.IDАвтомобиля, b.ДатаНачала, b.ДатаОкончания, b.Комментарий,
                       a.Модель, a.ГосНомер, a.Фото,
                       m.Название as Марка, c.Название as Класс, c.ID as IDКласса
                FROM Бронирование b
                JOIN Автомобиль a ON b.IDАвтомобиля = a.ID
                JOIN Марка m ON a.IDМарки = m.ID
                JOIN КлассАвтомобиля c ON a.IDКласса = c.ID
                WHERE b.IDКлиента = @ClientId";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ClientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var booking = new Booking
                {
                    Id = (int)reader["ID"],
                    CarId = (int)reader["IDАвтомобиля"],
                    StartDate = (DateTime)reader["ДатаНачала"],
                    EndDate = (DateTime)reader["ДатаОкончания"],
                    Comment = reader["Комментарий"] as string
                };

                // ЗАПОЛНЯЕМ CAR
                booking.Car = new Car
                {
                    Id = booking.CarId,
                    BrandName = reader["Марка"].ToString() ?? "",
                    Model = reader["Модель"].ToString() ?? "",
                    PlateNumber = reader["ГосНомер"].ToString() ?? "",
                    ClassName = reader["Класс"].ToString() ?? "",
                    ClassId = (int)reader["IDКласса"],
                    ImagePath = reader["Фото"] as string
                };

                list.Add(booking);
            }
            return list;
        }
        public List<BookingViewItem> GetBookingsView()
        {
            var list = new List<BookingViewItem>();
            string sql = "SELECT * FROM Представление_Бронирования ORDER BY ДатаНачала";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new BookingViewItem
                {
                    Id = (int)reader["ID"],
                    ClientId = (int)reader["IDКлиента"],
                    CarId = (int)reader["IDАвтомобиля"],

                    DateStart = (DateTime)reader["ДатаНачала"],
                    DateEnd = (DateTime)reader["ДатаОкончания"],
                    DateCreated = (DateTime)reader["ДатаСоздания"],

                    Status = reader["СтатусБрони"].ToString() ?? "",
                    Comment = reader["Комментарий"] as string ?? "",

                    ClientFullName = reader["КлиентФИО"].ToString() ?? "",
                    ClientPhone = reader["КлиентТелефон"].ToString() ?? "",

                    CarName = reader["АвтоНазвание"].ToString() ?? "",
                    CarPlate = reader["ГосНомер"].ToString() ?? ""
                });
            }
            return list;
        }

        public Booking? GetBookingById(int id)
        {
            string sql = "SELECT * FROM Бронирование WHERE ID = @Id";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Booking
                {
                    Id = (int)reader["ID"],
                    ClientId = (int)reader["IDКлиента"],
                    CarId = (int)reader["IDАвтомобиля"],
                    CreatedDate = (DateTime)reader["ДатаСоздания"],
                    StartDate = (DateTime)reader["ДатаНачала"],
                    EndDate = (DateTime)reader["ДатаОкончания"],
                    Comment = reader["Комментарий"] as string
                };
            }
            return null;
        }

        public void AddBooking(Booking booking)
        {
            string sql = @"
                INSERT INTO Бронирование (IDКлиента, IDАвтомобиля, ДатаНачала, ДатаОкончания, Комментарий)
                VALUES (@ClientId, @CarId, @Start, @End, @Comment)";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ClientId", booking.ClientId);
            cmd.Parameters.AddWithValue("@CarId", booking.CarId);
            cmd.Parameters.AddWithValue("@Start", booking.StartDate);
            cmd.Parameters.AddWithValue("@End", booking.EndDate);
            cmd.Parameters.AddWithValue("@Comment", booking.Comment as object ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void UpdateBooking(Booking booking)
        {
            string sql = @"
                UPDATE Бронирование
                SET IDКлиента = @ClientId,
                    IDАвтомобиля = @CarId,
                    ДатаНачала = @Start,
                    ДатаОкончания = @End,
                    Комментарий = @Comment
                WHERE ID = @Id";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", booking.Id);
            cmd.Parameters.AddWithValue("@ClientId", booking.ClientId);
            cmd.Parameters.AddWithValue("@CarId", booking.CarId);
            cmd.Parameters.AddWithValue("@Start", booking.StartDate);
            cmd.Parameters.AddWithValue("@End", booking.EndDate);
            cmd.Parameters.AddWithValue("@Comment", booking.Comment as object ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }
    }
}
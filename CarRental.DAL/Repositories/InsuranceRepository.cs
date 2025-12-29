using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class InsuranceRepository : BaseRepository
    {
        public List<Insurance> GetByCarId(int carId)
        {
            var list = new List<Insurance>();
            string sql = @"
                SELECT ID, НомерПолиса, ТипСтраховки, ДатаНачала, ДатаОкончания, Стоимость
                FROM Страховка
                WHERE IDАвтомобиля = @CarId AND ВАрхиве = 0
                ORDER BY ДатаОкончания DESC"; // Сначала свежие

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CarId", carId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Insurance
                {
                    Id = (int)reader["ID"],
                    CarId = carId,
                    PolicyNumber = reader["НомерПолиса"].ToString() ?? "",
                    Type = reader["ТипСтраховки"].ToString() ?? "",
                    StartDate = (DateTime)reader["ДатаНачала"],
                    EndDate = (DateTime)reader["ДатаОкончания"],
                    Cost = (decimal)reader["Стоимость"]
                });
            }
            return list;
        }
        public List<Insurance> GetArchivedInsurances()
        {
            var list = new List<Insurance>();
            string sql = @"
                SELECT I.ID, I.IDАвтомобиля, I.НомерПолиса, I.ТипСтраховки, I.ДатаНачала, I.ДатаОкончания, I.Стоимость, I.ВАрхиве,
                       M.Название + ' ' + A.Модель AS Авто
                FROM Страховка I
                JOIN Автомобиль A ON I.IDАвтомобиля = A.ID
                JOIN Марка M ON A.IDМарки = M.ID
                WHERE I.ВАрхиве = 1
                ORDER BY I.ДатаОкончания DESC";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) list.Add(MapInsurance(reader));
                }
            }
            return list;
        }

        private Insurance MapInsurance(SqlDataReader reader)
        {
            return new Insurance
            {
                Id = (int)reader["ID"],
                CarId = (int)reader["IDАвтомобиля"],
                PolicyNumber = reader["НомерПолиса"].ToString() ?? "",
                Type = reader["ТипСтраховки"].ToString() ?? "",
                StartDate = (DateTime)reader["ДатаНачала"],
                EndDate = (DateTime)reader["ДатаОкончания"],
                Cost = (decimal)reader["Стоимость"],
                IsArchived = (bool)reader["ВАрхиве"],
                CarName = reader["Авто"].ToString() ?? ""
            };
        }
        public void Add(Insurance ins)
        {
            string sql = @"
                INSERT INTO Страховка (IDАвтомобиля, НомерПолиса, ТипСтраховки, ДатаНачала, ДатаОкончания, Стоимость)
                VALUES (@CarId, @Policy, @Type, @Start, @End, @Cost)";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CarId", ins.CarId);
            cmd.Parameters.AddWithValue("@Policy", ins.PolicyNumber);
            cmd.Parameters.AddWithValue("@Type", ins.Type);
            cmd.Parameters.AddWithValue("@Start", ins.StartDate);
            cmd.Parameters.AddWithValue("@End", ins.EndDate);
            cmd.Parameters.AddWithValue("@Cost", ins.Cost);
            cmd.ExecuteNonQuery();
        }

        public void Update(Insurance ins)
        {
            // Номер полиса и ID Автомобиля НЕ обновляем (бизнес-логика)
            string sql = @"
                UPDATE Страховка 
                SET ТипСтраховки = @Type, 
                    ДатаНачала = @Start, 
                    ДатаОкончания = @End, 
                    Стоимость = @Cost
                WHERE ID = @Id";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", ins.Id);
            cmd.Parameters.AddWithValue("@Type", ins.Type);
            cmd.Parameters.AddWithValue("@Start", ins.StartDate);
            cmd.Parameters.AddWithValue("@End", ins.EndDate);
            cmd.Parameters.AddWithValue("@Cost", ins.Cost);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            string sql = "UPDATE Страховка SET ВАрхиве = 1 WHERE ID = @Id";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
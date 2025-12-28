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
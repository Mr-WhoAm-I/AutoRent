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
                WHERE IDАвтомобиля = @CarId
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
    }
}
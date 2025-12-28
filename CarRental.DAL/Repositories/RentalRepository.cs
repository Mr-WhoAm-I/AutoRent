using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class RentalRepository : BaseRepository
    {
        // Получить аренды конкретной машины (для календаря)
        public List<Rental> GetByCarId(int carId)
        {
            var list = new List<Rental>();
            string sql = @"
                SELECT a.ID, a.ДатаНачала, a.ДатаОкончанияПлановая, a.ДатаОкончанияФактическая
                FROM Аренда a
                WHERE a.IDАвтомобиля = @CarId";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CarId", carId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Rental
                {
                    Id = (int)reader["ID"],
                    StartDate = (DateTime)reader["ДатаНачала"],
                    PlannedEndDate = (DateTime)reader["ДатаОкончанияПлановая"],
                    ActualEndDate = reader["ДатаОкончанияФактическая"] as DateTime?
                });
            }
            return list;
        }

        public List<int> GetOccupiedCarIds(DateTime start, DateTime end)
        {
            var ids = new List<int>();
            // Пересечение интервалов: (StartA <= EndB) and (EndA >= StartB)
            // У нас: (RentalStart <= FilterEnd) AND (RentalEnd >= FilterStart)
            string sql = @"
                SELECT DISTINCT IDАвтомобиля 
                FROM Аренда 
                WHERE ДатаНачала <= @End AND (ISNULL(ДатаОкончанияФактическая, ДатаОкончанияПлановая) >= @Start)";

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
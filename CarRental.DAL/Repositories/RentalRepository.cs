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

        // ... (Остальные методы GetActive и т.д. у нас уже должны быть или будут позже)
    }
}
using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class ReferenceRepository : BaseRepository
    {
        // Получить список ролей
        public List<Role> GetRoles()
        {
            var list = new List<Role>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM Роль", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Role { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
                }
            }
            return list;
        }

        public List<CarBrand> GetBrands()
        {
            var list = new List<CarBrand>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM Марка ORDER BY Название", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new CarBrand
                    {
                        Id = (int)reader["ID"],
                        Name = reader["Название"].ToString() ?? ""
                    });
                }
            }
            return list;
        }

        // Получить классы авто
        public List<CarClass> GetClasses()
        {
            var list = new List<CarClass>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название, Описание FROM КлассАвтомобиля", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new CarClass
                    {
                        Id = (int)reader["ID"],
                        Name = reader["Название"].ToString() ?? "",
                        Description = reader["Описание"] as string // Может быть null
                    });
                }
            }
            return list;
        }

        // Получить типы кузова
        public List<BodyType> GetBodyTypes()
        {
            var list = new List<BodyType>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM ТипКузова", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new BodyType { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
                }
            }
            return list;
        }

        // Получить типы топлива
        public List<FuelType> GetFuelTypes()
        {
            var list = new List<FuelType>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM ТипТоплива", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new FuelType { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
                }
            }
            return list;
        }

        // Получить типы КПП
        public List<TransmissionType> GetTransmissionTypes()
        {
            var list = new List<TransmissionType>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM ТипТрансмиссии", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new TransmissionType { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
                }
            }
            return list;
        }

        // Получить статусы
        public List<CarStatus> GetStatuses()
        {
            var list = new List<CarStatus>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM СтатусАвто", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new CarStatus { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
                }
            }
            return list;
        }
    }
}
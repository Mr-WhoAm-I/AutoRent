using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class ReferenceRepository : BaseRepository
    {
        // === СУЩЕСТВУЮЩИЕ МЕТОДЫ (ОСТАВЛЯЕМ КАК БЫЛО) ===
        // Они нужны для выпадающих списков в других окнах

        public List<Role> GetRoles()
        {
            var list = new List<Role>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM Роль", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(new Role { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
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
                    list.Add(new CarBrand { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
            }
            return list;
        }

        public List<CarClass> GetClasses()
        {
            var list = new List<CarClass>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название, Описание FROM КлассАвтомобиля", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(new CarClass { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "", Description = reader["Описание"] as string });
            }
            return list;
        }

        public List<BodyType> GetBodyTypes()
        {
            var list = new List<BodyType>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM ТипКузова", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(new BodyType { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
            }
            return list;
        }

        public List<TransmissionType> GetTransmissionTypes()
        {
            var list = new List<TransmissionType>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM ТипТрансмиссии", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(new TransmissionType { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
            }
            return list;
        }

        public List<FuelType> GetFuelTypes()
        {
            var list = new List<FuelType>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM ТипТоплива", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(new FuelType { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
            }
            return list;
        }

        public List<CarStatus> GetStatuses()
        {
            var list = new List<CarStatus>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT ID, Название FROM СтатусАвто", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(new CarStatus { Id = (int)reader["ID"], Name = reader["Название"].ToString() ?? "" });
            }
            return list;
        }

        // =========================================================
        // === НОВЫЕ МЕТОДЫ ДЛЯ СТРАНИЦЫ "СПРАВОЧНИКИ" (GENERIC) ===
        // =========================================================

        private readonly HashSet<string> _allowedTables = new()
        {
            "Марка", "КлассАвтомобиля", "ТипКузова", "ТипТрансмиссии",
            "ТипТоплива", "СтатусАвто", "Роль"
        };

        public List<ReferenceItem> GetItems(string tableName)
        {
            if (!_allowedTables.Contains(tableName)) return new List<ReferenceItem>();

            var list = new List<ReferenceItem>();
            bool hasDesc = HasDescription(tableName);

            string sql = hasDesc
                ? $"SELECT ID, Название, Описание FROM {tableName} ORDER BY Название"
                : $"SELECT ID, Название FROM {tableName} ORDER BY Название";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ReferenceItem
                {
                    Id = (int)reader["ID"],
                    Name = reader["Название"].ToString() ?? "",
                    Description = hasDesc ? reader["Описание"] as string : null,
                    TableName = tableName,
                    HasDescription = hasDesc
                });
            }
            return list;
        }

        public void SaveChanges(string tableName, List<ReferenceItem> items, List<int> deletedIds)
        {
            if (!_allowedTables.Contains(tableName)) return;
            bool hasDesc = HasDescription(tableName);

            using var conn = GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Удаление
                foreach (int id in deletedIds)
                {
                    string delSql = $"DELETE FROM {tableName} WHERE ID = @Id";
                    using var cmd = new SqlCommand(delSql, conn, transaction);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                // 2. Вставка и Обновление
                foreach (var item in items)
                {
                    if (string.IsNullOrWhiteSpace(item.Name)) continue;

                    if (item.Id == 0) // INSERT
                    {
                        string insertSql = hasDesc
                            ? $"INSERT INTO {tableName} (Название, Описание) VALUES (@Name, @Desc)"
                            : $"INSERT INTO {tableName} (Название) VALUES (@Name)";

                        using var cmd = new SqlCommand(insertSql, conn, transaction);
                        cmd.Parameters.AddWithValue("@Name", item.Name);
                        if (hasDesc) cmd.Parameters.AddWithValue("@Desc", item.Description ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    else // UPDATE
                    {
                        string updateSql = hasDesc
                            ? $"UPDATE {tableName} SET Название = @Name, Описание = @Desc WHERE ID = @Id"
                            : $"UPDATE {tableName} SET Название = @Name WHERE ID = @Id";

                        using var cmd = new SqlCommand(updateSql, conn, transaction);
                        cmd.Parameters.AddWithValue("@Id", item.Id);
                        cmd.Parameters.AddWithValue("@Name", item.Name);
                        if (hasDesc) cmd.Parameters.AddWithValue("@Desc", item.Description ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool HasDescription(string tableName)
        {
            return tableName == "КлассАвтомобиля" || tableName == "Роль";
        }
    }
}
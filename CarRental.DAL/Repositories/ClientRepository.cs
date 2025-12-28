using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;
using System;
using System.Collections.Generic;

namespace CarRental.DAL.Repositories
{
    public class ClientRepository : BaseRepository
    {
        public List<Client> GetAllClients()
        {
            var list = new List<Client>();
            // Добавили ДатаРождения в выборку
            string sql = @"
                SELECT ID, Фамилия, Имя, Отчество, Телефон, Почта, ДатаРождения, Возраст, СтажВождения, ВАрхиве
                  FROM Клиент
                  WHERE ВАрхиве = 0
                  ORDER BY Фамилия";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Client
                        {
                            Id = (int)reader["ID"],
                            Surname = reader["Фамилия"].ToString() ?? "",
                            Name = reader["Имя"].ToString() ?? "",
                            Patronymic = reader["Отчество"] as string,
                            Phone = reader["Телефон"].ToString() ?? "",
                            Email = reader["Почта"] as string,
                            DateOfBirth = reader["ДатаРождения"] as DateTime?, // Читаем дату
                            Age = reader["Возраст"] as int?,
                            DrivingExperience = reader["СтажВождения"] as int?,
                            IsArchived = (bool)reader["ВАрхиве"]
                        });
                    }
                }
            }
            return list;
        }

        public void AddClient(Client client)
        {
            // Добавляем запись с Датой Рождения
            string sql = @"
                INSERT INTO Клиент (Фамилия, Имя, Отчество, Телефон, Почта, ДатаРождения, СтажВождения, ВАрхиве)
                VALUES (@Surname, @Name, @Patronymic, @Phone, @Email, @Dob, @Exp, 0)";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Surname", client.Surname);
            cmd.Parameters.AddWithValue("@Name", client.Name);
            cmd.Parameters.AddWithValue("@Patronymic", client.Patronymic as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", client.Phone);
            cmd.Parameters.AddWithValue("@Email", client.Email as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Dob", client.DateOfBirth as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Exp", client.DrivingExperience as object ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        // НОВЫЙ МЕТОД: Редактирование
        public void UpdateClient(Client client)
        {
            string sql = @"
                UPDATE Клиент 
                SET Фамилия = @Surname, 
                    Имя = @Name, 
                    Отчество = @Patronymic,
                    Телефон = @Phone, 
                    Почта = @Email, 
                    ДатаРождения = @Dob, 
                    СтажВождения = @Exp
                WHERE ID = @Id";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", client.Id);
            cmd.Parameters.AddWithValue("@Surname", client.Surname);
            cmd.Parameters.AddWithValue("@Name", client.Name);
            cmd.Parameters.AddWithValue("@Patronymic", client.Patronymic as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", client.Phone);
            cmd.Parameters.AddWithValue("@Email", client.Email as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Dob", client.DateOfBirth as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Exp", client.DrivingExperience as object ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Archive(int id)
        {
            string sql = "UPDATE Клиент SET ВАрхиве = 1 WHERE ID = @Id";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
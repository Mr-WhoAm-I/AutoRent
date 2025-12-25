using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class ClientRepository : BaseRepository
    {
        public List<Client> GetAllClients()
        {
            var list = new List<Client>();
            // Выбираем только тех, кто не в архиве (IsArchived = 0)
            string sql = @"
                SELECT ID, Фамилия, Имя, Отчество, Телефон, Почта, Возраст, СтажВождения, ВАрхиве
                  FROM Клиент
                  WHERE ВАрхиве = 0";

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
            string sql = @"
                INSERT INTO Клиент (Фамилия, Имя, Отчество, Телефон, Почта, Возраст, СтажВождения, ВАрхиве)
                VALUES (@Surname, @Name, @Patronymic, @Phone, @Email, @Age, @Exp, 0)";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Surname", client.Surname);
            cmd.Parameters.AddWithValue("@Name", client.Name);
            cmd.Parameters.AddWithValue("@Phone", client.Phone);

            // Обработка NULL значений
            cmd.Parameters.AddWithValue("@Patronymic", client.Patronymic as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", client.Email as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Age", client.Age as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Exp", client.DrivingExperience as object ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }
    }
}
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class PaymentRepository : BaseRepository
    {
        public List<Payment> GetByRentalId(int rentalId)
        {
            var list = new List<Payment>();
            string sql = "SELECT * FROM Платеж WHERE IDАренды = @Rid ORDER BY ДатаПлатежа DESC";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Rid", rentalId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Payment
                {
                    Id = (int)reader["ID"],
                    RentalId = (int)reader["IDАренды"],
                    Date = (System.DateTime)reader["ДатаПлатежа"],
                    Amount = (decimal)reader["Сумма"],
                    Type = reader["ТипПлатежа"].ToString()
                });
            }
            return list;
        }

        public void Add(Payment p)
        {
            string sql = "INSERT INTO Платеж (IDАренды, ДатаПлатежа, Сумма, ТипПлатежа) VALUES (@Rid, @Date, @Sum, @Type)";
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Rid", p.RentalId);
            cmd.Parameters.AddWithValue("@Date", p.Date);
            cmd.Parameters.AddWithValue("@Sum", p.Amount);
            cmd.Parameters.AddWithValue("@Type", p.Type);
            cmd.ExecuteNonQuery();
        }

        public void Update(Payment p)
        {
            string sql = "UPDATE Платеж SET ДатаПлатежа=@Date, Сумма=@Sum, ТипПлатежа=@Type WHERE ID=@Id";
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", p.Id);
            cmd.Parameters.AddWithValue("@Date", p.Date);
            cmd.Parameters.AddWithValue("@Sum", p.Amount);
            cmd.Parameters.AddWithValue("@Type", p.Type);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM Платеж WHERE ID = @Id";
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
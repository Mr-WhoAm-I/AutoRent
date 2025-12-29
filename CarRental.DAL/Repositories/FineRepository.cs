using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class FineRepository : BaseRepository
    {
        public List<Fine> GetByRentalId(int rentalId)
        {
            var list = new List<Fine>();
            string sql = "SELECT * FROM Штраф WHERE IDАренды = @Rid ORDER BY ДатаВыставления DESC";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Rid", rentalId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Fine
                {
                    Id = (int)reader["ID"],
                    RentalId = (int)reader["IDАренды"],
                    Date = (System.DateTime)reader["ДатаВыставления"],
                    Amount = (decimal)reader["Сумма"],
                    Reason = reader["Причина"].ToString(),
                    IsPaid = (bool)reader["Оплачен"]
                });
            }
            return list;
        }

        public void Add(Fine fine)
        {
            string sql = "INSERT INTO Штраф (IDАренды, ДатаВыставления, Сумма, Причина, Оплачен) VALUES (@Rid, @Date, @Sum, @Reason, @Paid)";
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Rid", fine.RentalId);
            cmd.Parameters.AddWithValue("@Date", fine.Date);
            cmd.Parameters.AddWithValue("@Sum", fine.Amount);
            cmd.Parameters.AddWithValue("@Reason", fine.Reason);
            cmd.Parameters.AddWithValue("@Paid", fine.IsPaid);
            cmd.ExecuteNonQuery();
        }

        public void Update(Fine fine)
        {
            string sql = "UPDATE Штраф SET ДатаВыставления=@Date, Сумма=@Sum, Причина=@Reason, Оплачен=@Paid WHERE ID=@Id";
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", fine.Id);
            cmd.Parameters.AddWithValue("@Date", fine.Date);
            cmd.Parameters.AddWithValue("@Sum", fine.Amount);
            cmd.Parameters.AddWithValue("@Reason", fine.Reason);
            cmd.Parameters.AddWithValue("@Paid", fine.IsPaid);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM Штраф WHERE ID = @Id";
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        // Вызов процедуры с КУРСОРОМ для массового начисления штрафов
        public int ProcessOverdueRentals()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("sp_НачислитьШтрафыЗаПросрочку", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            object result = cmd.ExecuteScalar();
            return result != null ? (int)result : 0;
        }
    }
}
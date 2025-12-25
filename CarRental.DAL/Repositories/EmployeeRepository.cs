using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class EmployeeRepository : BaseRepository
    {
        // Поиск сотрудника по Логину
        public Employee? GetByLogin(string login)
        {
            // Используем JOIN, чтобы сразу получить название роли (Администратор/Менеджер)
            string sql = @"
                SELECT s.ID, Фамилия, Имя, Логин, Пароль, 
                       IDРоли, Название AS НазваниеРоли
                  FROM Сотрудник s
                    INNER JOIN Роль r ON s.IDРоли = r.ID
                  WHERE s.Логин = @Login";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Login", login);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Employee
                {
                    Id = (int)reader["ID"],
                    Surname = reader["Фамилия"].ToString() ?? "",
                    Name = reader["Имя"].ToString() ?? "",
                    Login = reader["Логин"].ToString() ?? "",
                    Password = reader["Пароль"].ToString() ?? "",
                    RoleId = (int)reader["IDРоли"],
                    RoleName = reader["НазваниеРоли"].ToString() ?? ""
                };
            }
            return null; // Если пользователь не найден
        }
    }
}
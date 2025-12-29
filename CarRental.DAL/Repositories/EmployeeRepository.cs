using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Repositories
{
    public class EmployeeRepository : BaseRepository
    {
        // Поиск сотрудника по Логину
        public Employee? GetByLogin(string login)
        {
            string sql = @"
                SELECT s.ID, Фамилия, Имя, Логин, Пароль, 
                       IDРоли, Название AS НазваниеРоли
                  FROM Сотрудник s
                    INNER JOIN Роль r ON s.IDРоли = r.ID
                  WHERE s.Логин = @Login AND s.ВАрхиве = 0";

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

        public List<Employee> GetAll()
        {
            var list = new List<Employee>();
            string sql = @"
                SELECT s.ID, s.Фамилия, s.Имя, s.Логин, s.Должность, s.IDРоли, r.Название as Роль
                FROM Сотрудник s
                JOIN Роль r ON s.IDРоли = r.ID
                WHERE s.ВАрхиве = 0
                ORDER BY s.Фамилия";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Employee
                {
                    Id = (int)reader["ID"],
                    Surname = reader["Фамилия"].ToString() ?? "",
                    Name = reader["Имя"].ToString() ?? "",
                    Login = reader["Логин"].ToString() ?? "",
                    Position = reader["Должность"].ToString() ?? "",
                    RoleId = (int)reader["IDРоли"],
                    RoleName = reader["Роль"].ToString() ?? ""
                    // Пароль тут читать не нужно в целях безопасности
                });
            }
            return list;
        }

        // Получение уволенных сотрудников
        public List<Employee> GetArchivedEmployees()
        {
            var list = new List<Employee>();
            string sql = @"
                SELECT s.ID, s.Фамилия, s.Имя, s.Логин, s.Должность, s.IDРоли, r.Название as Роль
                FROM Сотрудник s
                JOIN Роль r ON s.IDРоли = r.ID
                WHERE s.ВАрхиве = 1
                ORDER BY s.Фамилия";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Employee
                {
                    Id = (int)reader["ID"],
                    Surname = reader["Фамилия"].ToString() ?? "",
                    Name = reader["Имя"].ToString() ?? "",
                    Login = reader["Логин"].ToString() ?? "",
                    Position = reader["Должность"].ToString() ?? "",
                    RoleId = (int)reader["IDРоли"],
                    RoleName = reader["Роль"].ToString() ?? ""
                });
            }
            return list;
        }
        // Получить активных сотрудников по названию роли
        public List<Employee> GetByRole(string roleName)
        {
            var list = new List<Employee>();
            string sql = @"
                SELECT s.ID, s.Фамилия, s.Имя, s.Логин, s.Должность, r.Название as Роль, s.IDРоли
                FROM Сотрудник s
                JOIN Роль r ON s.IDРоли = r.ID
                WHERE r.Название = @RoleName AND s.ВАрхиве = 0
                ORDER BY s.Фамилия";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@RoleName", roleName);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Employee
                {
                    Id = (int)reader["ID"],
                    Surname = reader["Фамилия"].ToString() ?? "",
                    Name = reader["Имя"].ToString() ?? "",
                    RoleName = reader["Роль"].ToString() ?? "",
                    Position = reader["Должность"].ToString() ?? "",
                    RoleId = (int)reader["IDРоли"]
                });
            }
            return list;
        }

        public string GetPasswordHash(int id)
        {
            string sql = "SELECT Пароль FROM Сотрудник WHERE ID = @Id";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteScalar()?.ToString() ?? "";
        }

        // 3. Добавление
        public void Add(Employee emp)
        {
            string sql = @"
                INSERT INTO Сотрудник (IDРоли, Фамилия, Имя, Логин, Пароль, Должность, ВАрхиве)
                VALUES (@RoleId, @Surname, @Name, @Login, @Password, @Position, 0)";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@RoleId", emp.RoleId);
            cmd.Parameters.AddWithValue("@Surname", emp.Surname);
            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Login", emp.Login);
            cmd.Parameters.AddWithValue("@Password", emp.Password); // Уже хешированный
            cmd.Parameters.AddWithValue("@Position", emp.Position);
            cmd.ExecuteNonQuery();
        }

        // 4. Обновление
        public void Update(Employee emp)
        {
            // Если пароль пустой, не обновляем его
            string passwordClause = string.IsNullOrEmpty(emp.Password) ? "" : ", Пароль = @Password";

            string sql = $@"
                UPDATE Сотрудник
                SET IDРоли = @RoleId,
                    Фамилия = @Surname,
                    Имя = @Name,
                    Логин = @Login,
                    Должность = @Position
                    {passwordClause}
                WHERE ID = @Id";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", emp.Id);
            cmd.Parameters.AddWithValue("@RoleId", emp.RoleId);
            cmd.Parameters.AddWithValue("@Surname", emp.Surname);
            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Login", emp.Login);
            cmd.Parameters.AddWithValue("@Position", emp.Position);

            if (!string.IsNullOrEmpty(emp.Password))
                cmd.Parameters.AddWithValue("@Password", emp.Password);

            cmd.ExecuteNonQuery();
        }

        // 5. Удаление (Архивация)
        public void Archive(int id)
        {
            string sql = "UPDATE Сотрудник SET ВАрхиве = 1 WHERE ID = @Id";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
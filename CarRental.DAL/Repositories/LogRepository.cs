using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace CarRental.DAL.Repositories
{
    public class LogRepository : BaseRepository
    {
        // Список таблиц, для которых включены логи
        public List<string> GetLogTables()
        {
            return new List<string> { "Сотрудник", "Аренда", "Платеж", "Штраф" };
        }

        public DataTable GetLogs(string tableName)
        {
            var dt = new DataTable();
            // Защита от SQL-инъекций: проверяем, есть ли таблица в нашем белом списке
            if (!GetLogTables().Contains(tableName)) return dt;

            string logTableName = tableName + "Log";
            string sql = $"SELECT * FROM {logTableName} ORDER BY dateLog DESC";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var adapter = new SqlDataAdapter(cmd);

            adapter.Fill(dt);
            return dt;
        }

        // Метод очистки логов (опционально, для кнопки "Очистить")
        public void ClearLogs(string tableName)
        {
            if (!GetLogTables().Contains(tableName)) return;
            string logTableName = tableName + "Log";
            string sql = $"TRUNCATE TABLE {logTableName}";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public void RestoreVersion(string tableName, int logId)
        {
            // Защита: разрешаем только таблицы из белого списка
            if (!GetLogTables().Contains(tableName)) return;

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("RestoreFromLog", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@TableName", tableName);
            cmd.Parameters.AddWithValue("@LogId", logId);

            cmd.ExecuteNonQuery();
        }
    }
}
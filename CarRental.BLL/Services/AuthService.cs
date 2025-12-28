using System.Security.Cryptography;
using System.Text;
using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class AuthService
    {
        private readonly EmployeeRepository _repository;

        // Статическое свойство, чтобы хранить того, кто сейчас вошел в систему
        // Доступно во всем приложении через AuthService.CurrentUser
        public static Employee? CurrentUser { get; private set; }

        public AuthService()
        {
            _repository = new EmployeeRepository();
        }

        public bool Login(string login, string password)
        {
            // 1. Ищем пользователя в базе
            var employee = _repository.GetByLogin(login);

            if (employee == null)
                return false; // Пользователь не найден

            // 2. Хешируем введенный пароль
            string inputHash = ComputeSha256Hash(password);

            // 3. Сравниваем хеши (OrdinalIgnoreCase игнорирует регистр букв в хеше)
            // TrimEnd() нужен, т.к. тип CHAR(64) в SQL добивает строку пробелами до 64 символов
            if (string.Equals(employee.Password.TrimEnd(), inputHash, StringComparison.OrdinalIgnoreCase))
            {
                CurrentUser = employee; // Запоминаем сотрудника
                return true; // Успех
            }

            return false; // Неверный пароль
        }

        // Вспомогательный метод хеширования
        public static string ComputeSha256Hash(string rawData)
        {
            // Преобразуем строку в байты
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

            // Преобразуем байты в строку (hex)
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
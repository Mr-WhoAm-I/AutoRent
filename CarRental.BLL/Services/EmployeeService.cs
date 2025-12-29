using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;
using System;
using System.Collections.Generic;

namespace CarRental.BLL.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _repo = new();

        public List<Employee> GetAll() => _repo.GetAll();
        public List<Employee> GetArchivedEmployees() => _repo.GetArchivedEmployees();
        public List<Employee> GetMechanics() => _repo.GetByRole("Механик");

        // Получить всех, кто может оформлять аренду (Админы + Менеджеры)
        public List<Employee> GetManagersAndAdmins()
        {
            var admins = _repo.GetByRole("Администратор");
            var managers = _repo.GetByRole("Менеджер");

            var result = new List<Employee>();
            result.AddRange(admins);
            result.AddRange(managers);

            // Можно отсортировать по фамилии
            return result.OrderBy(e => e.Surname).ToList();
        }
        // Метод сохранения (УБРАН oldPassword)
        public void Save(Employee emp, string? password)
        {
            // 1. Валидация
            if (string.IsNullOrWhiteSpace(emp.Surname) || string.IsNullOrWhiteSpace(emp.Name))
                throw new Exception("Введите Имя и Фамилию.");
            if (string.IsNullOrWhiteSpace(emp.Login))
                throw new Exception("Введите Логин.");
            if (string.IsNullOrWhiteSpace(emp.Position))
                throw new Exception("Укажите Должность.");

            // 2. Логика паролей
            if (emp.Id == 0) // НОВЫЙ СОТРУДНИК
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new Exception("Для нового сотрудника пароль обязателен.");

                // Хешируем и сохраняем
                emp.Password = AuthService.ComputeSha256Hash(password);
                _repo.Add(emp);
            }
            else // РЕДАКТИРОВАНИЕ
            {
                // Если админ ввел что-то в поле пароля - обновляем его
                if (!string.IsNullOrWhiteSpace(password))
                {
                    emp.Password = AuthService.ComputeSha256Hash(password);
                }
                else
                {
                    emp.Password = ""; // Пустой пароль = не обновлять в БД
                }

                _repo.Update(emp);
            }
        }

        public void Archive(int id)
        {
            if (id == AuthService.CurrentUser?.Id)
                throw new Exception("Нельзя удалить самого себя!");

            _repo.Archive(id);
        }
    }
}
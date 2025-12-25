using System;
using System.Collections.Generic;
using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class ClientService
    {
        private readonly ClientRepository _repository;

        public ClientService()
        {
            _repository = new ClientRepository();
        }

        public List<Client> GetClients()
        {
            return _repository.GetAllClients();
        }

        public void AddClient(Client client)
        {
            // Валидация (бизнес-правила)
            if (string.IsNullOrWhiteSpace(client.Surname) || string.IsNullOrWhiteSpace(client.Name))
                throw new Exception("Имя и Фамилия обязательны.");

            if (string.IsNullOrWhiteSpace(client.Phone))
                throw new Exception("Телефон обязателен.");

            if (client.Age < 18)
                throw new Exception("Клиент должен быть совершеннолетним.");

            _repository.AddClient(client);
        }
    }
}
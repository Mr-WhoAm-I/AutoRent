using CarRental.DAL.Repositories;
using CarRental.Domain.DTO;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class ClientService
    {
        private readonly ClientRepository _repository;
        private readonly RentalRepository _rentalRepo = new();
        private readonly BookingRepository _bookingRepo = new();

        public ClientService()
        {
            _repository = new ClientRepository();
        }

        public List<Client> GetClients() => _repository.GetAllClients();

        public void AddClient(Client client)
        {
            Validate(client);
            _repository.AddClient(client);
        }

        public void UpdateClient(Client client)
        {
            Validate(client);
            _repository.UpdateClient(client);
        }

        public void SaveClient(Client client)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(client.Surname) || string.IsNullOrWhiteSpace(client.Name))
                throw new Exception("Имя и Фамилия обязательны.");

            if (string.IsNullOrWhiteSpace(client.Phone))
                throw new Exception("Телефон обязателен.");

            // Проверка возраста (если указана дата рождения)
            if (client.DateOfBirth.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - client.DateOfBirth.Value.Year;
                if (client.DateOfBirth.Value.Date > today.AddYears(-age)) age--;

                if (age < 18) throw new Exception("Клиент должен быть совершеннолетним (18+).");
            }

            if (client.Id == 0)
                _repository.AddClient(client);
            else
                _repository.UpdateClient(client);
        }

        public void Archive(int id) => _repository.Archive(id);

        private void Validate(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.Surname) || string.IsNullOrWhiteSpace(client.Name))
                throw new Exception("Имя и Фамилия обязательны.");

            if (string.IsNullOrWhiteSpace(client.Phone))
                throw new Exception("Телефон обязателен.");

            if (client.DateOfBirth.HasValue)
            {
                if (client.Age < 18) throw new Exception("Клиент должен быть совершеннолетним (18+).");
            }
        }

        // Добавьте этот метод в ClientService
        public List<ClientHistoryItem> GetClientHistory(int clientId)
        {
            var history = new List<ClientHistoryItem>();
            var rentalRepo = new RentalRepository(); // Или через DI
            var bookingRepo = new BookingRepository();

            var rentals = rentalRepo.GetByClientId(clientId);
            var bookings = bookingRepo.GetByClientId(clientId);

            // 1. Добавляем Аренды
            foreach (var r in rentals)
            {
                var item = new ClientHistoryItem
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    Type = "Rental",
                    StartDate = r.StartDate,
                    EndDate = r.ActualEndDate ?? r.PlannedEndDate, // Если не вернул, показываем плановую или текущую
                    CarTitle = r.CarDisplayName,
                    CarDetails = r.CarSubInfo,
                    Note = r.Review,
                    Cost = r.TotalPrice
                };

                if (r.ActualEndDate == null)
                {
                    item.Status = "В аренде";
                    item.SortOrder = 1; // Самый высокий приоритет
                }
                else
                {
                    item.Status = "Завершена";
                    item.SortOrder = 3;
                }
                history.Add(item);
            }

            // 2. Добавляем Брони
            foreach (var b in bookings)
            {
                history.Add(new ClientHistoryItem
                {
                    Id = b.Id,
                    CarId = b.CarId,
                    Type = "Booking",
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    CarTitle = b.CarDisplayName,
                    CarDetails = b.CarSubInfo,
                    Note = b.Comment,
                    Status = "Забронирован",
                    SortOrder = 2 // Средний приоритет
                });
            }

            // Сортировка: Сначала активные, потом брони, потом история (по дате убывания)
            return history.OrderBy(x => x.SortOrder).ThenByDescending(x => x.StartDate).ToList();
        }
    }
}
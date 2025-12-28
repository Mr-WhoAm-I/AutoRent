using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class CarService
    {
        private readonly CarRepository _carRepo = new();
        private readonly RentalRepository _rentalRepo = new();
        private readonly MaintenanceRepository _maintRepo = new();
        private readonly BookingRepository _bookingRepo = new();

        public List<Car> GetCars() => _carRepo.GetAllCars();


        public List<int> GetOccupiedCarIds(DateTime start, DateTime end)
        {
            var busyIds = new List<int>();

            // Собираем ID из всех источников
            busyIds.AddRange(_rentalRepo.GetOccupiedCarIds(start, end));
            busyIds.AddRange(_bookingRepo.GetOccupiedCarIds(start, end)); // Бронь
            busyIds.AddRange(_maintRepo.GetOccupiedCarIds(start, end));   // Ремонт

            // Возвращаем уникальные ID
            return busyIds.Distinct().ToList();
        }

        public void AddCar(Car car) => _carRepo.AddCar(car);

        // Метод обновления (понадобится для редактирования)
        // (Убедись, что в CarRepository есть UpdateCar, если нет — пока оставь пустым или добавь позже)
        public void UpdateCar(Car car) => _carRepo.UpdateCar(car);

        public List<CalendarItem> GetCarSchedule(int carId)
        {
            var schedule = new List<CalendarItem>();

            // 1. Аренды
            schedule.AddRange(_rentalRepo.GetCalendarItems(carId));

            // 2. Ремонты
            schedule.AddRange(_maintRepo.GetCalendarItems(carId));

            // 3. Брони (если реализуете BookingRepository)
            schedule.AddRange(_bookingRepo.GetCalendarItems(carId));

            return schedule;
        }
    }

    // Вспомогательный класс для календаря
    public class DateRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Type { get; set; } = "";
        public string ColorHex { get; set; } = "";
    }
}
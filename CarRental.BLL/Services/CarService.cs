using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class CarService
    {
        private readonly CarRepository _carRepo = new();
        private readonly RentalRepository _rentalRepo = new();
        private readonly MaintenanceRepository _maintRepo = new();
        // Repository страховок пока опустим для краткости, логика та же

        public List<Car> GetCars() => _carRepo.GetAllCars();

        // Метод для календаря: Получить занятые диапазоны
        public List<DateRange> GetCarSchedule(int carId)
        {
            var schedule = new List<DateRange>();

            // 1. Аренды (Синий цвет)
            var rentals = _rentalRepo.GetByCarId(carId);
            foreach (var r in rentals)
            {
                // Если машина возвращена, берем фактическую дату, иначе плановую
                var end = r.ActualEndDate ?? r.PlannedEndDate;
                schedule.Add(new DateRange
                {
                    Start = r.StartDate,
                    End = end,
                    Type = "Rental",
                    ColorHex = "#4F46E5" // Индиго
                });
            }

            // 2. Ремонты (Оранжевый цвет)
            // Предполагаем, что метод GetByCarId есть в MaintenanceRepository
            // var maints = _maintRepo.GetByCarId(carId); 
            // ... аналогичное добавление с Type="Maintenance" и цветом Orange

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
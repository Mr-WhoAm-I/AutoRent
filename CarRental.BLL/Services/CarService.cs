using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class CarService
    {
        private readonly CarRepository _repository;

        public CarService()
        {
            _repository = new CarRepository();
        }

        public List<Car> GetCars()
        {
            return _repository.GetAllCars();
        }
        
        public void CreateCar(Car car)
        {
            if (car.Year < 1990)
                throw new Exception("Нельзя добавлять ретро-автомобили старше 1990 года.");

            if (car.PricePerDay <= 0)
                throw new Exception("Цена аренды должна быть положительной.");

            _repository.AddCar(car);
        }
    }
}
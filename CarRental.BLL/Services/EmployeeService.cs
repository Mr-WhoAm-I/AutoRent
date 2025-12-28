using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;


namespace CarRental.BLL.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _repo = new();

        public List<Employee> GetMechanics()
        {
            return _repo.GetByRole("Механик");
        }
    }
}

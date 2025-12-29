using System.Collections.Generic;
using CarRental.DAL.Repositories;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Services
{
    public class FinanceService
    {
        private readonly FineRepository _fineRepo = new();
        private readonly PaymentRepository _payRepo = new();

        // ШТРАФЫ
        public List<Fine> GetFines(int rentalId) => _fineRepo.GetByRentalId(rentalId);
        public void SaveFine(Fine fine)
        {
            if (fine.Id == 0) _fineRepo.Add(fine);
            else _fineRepo.Update(fine);
        }
        // Автоматическое начисление штрафов (вызывает процедуру с курсором)
        public int AutoChargeFines()
        {
            return _fineRepo.ProcessOverdueRentals();
        }
        public void DeleteFine(int id) => _fineRepo.Delete(id);

        // ПЛАТЕЖИ
        public List<Payment> GetPayments(int rentalId) => _payRepo.GetByRentalId(rentalId);
        public void SavePayment(Payment pay)
        {
            if (pay.Id == 0) _payRepo.Add(pay);
            else _payRepo.Update(pay);
        }
        public void DeletePayment(int id) => _payRepo.Delete(id);
    }
}
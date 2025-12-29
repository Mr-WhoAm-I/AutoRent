using System;
using System.Collections.Generic;
using CarRental.DAL.Repositories;
using CarRental.Domain.DTO;

namespace CarRental.BLL.Services
{
    public class ReportService
    {
        private readonly ReportRepository _repo = new();

        public List<ClientReportItem> GetClientsReport() => _repo.GetClientsReport();

        // Теперь принимает список
        public List<CarPerformanceItem> GetCarPerformance(List<int> classIds)
            => _repo.GetCarPerformance(classIds);

        // Теперь возвращает детали
        public List<PaymentReportItem> GetPaymentDetails(DateTime start, DateTime end)
            => _repo.GetPaymentDetails(start, end);
    }
}
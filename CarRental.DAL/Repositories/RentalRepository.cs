using CarRental.Domain.DTO;
using CarRental.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace CarRental.DAL.Repositories
{
    public class RentalRepository : BaseRepository
    {
        // Получить аренды конкретной машины (для календаря)
        public List<Rental> GetByCarId(int carId)
        {
            var list = new List<Rental>();
            string sql = @"
                SELECT a.ID, a.ДатаНачала, a.ДатаОкончанияПлановая, a.ДатаОкончанияФактическая
                FROM Аренда a
                WHERE a.IDАвтомобиля = @CarId";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CarId", carId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Rental
                {
                    Id = (int)reader["ID"],
                    StartDate = (DateTime)reader["ДатаНачала"],
                    PlannedEndDate = (DateTime)reader["ДатаОкончанияПлановая"],
                    ActualEndDate = reader["ДатаОкончанияФактическая"] as DateTime?
                });
            }
            return list;
        }

        public List<int> GetOccupiedCarIds(DateTime start, DateTime end)
        {
            var ids = new List<int>();
            // Пересечение интервалов: (StartA <= EndB) and (EndA >= StartB)
            // У нас: (RentalStart <= FilterEnd) AND (RentalEnd >= FilterStart)
            string sql = @"
                SELECT DISTINCT IDАвтомобиля 
                FROM Аренда 
                WHERE ДатаНачала <= @End AND (ISNULL(ДатаОкончанияФактическая, ДатаОкончанияПлановая) >= @Start)";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ids.Add((int)reader["IDАвтомобиля"]);
            }
            return ids;
        }

        // Получить список аренд для календаря
        public List<CalendarItem> GetCalendarItems(int carId)
        {
            var list = new List<CalendarItem>();
            string sql = @"
                SELECT a.ДатаНачала, 
                       ISNULL(a.ДатаОкончанияФактическая, a.ДатаОкончанияПлановая) AS Конец,
                       k.Фамилия, k.Имя
                FROM Аренда a
                JOIN Клиент k ON a.IDКлиента = k.ID
                WHERE a.IDАвтомобиля = @CarId";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CarId", carId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string clientName = $"{reader["Фамилия"]} {reader["Имя"]}";
                list.Add(new CalendarItem
                {
                    Start = (DateTime)reader["ДатаНачала"],
                    End = (DateTime)reader["Конец"],
                    Type = "Rental",
                    TooltipText = $"В аренде у: {clientName}"
                });
            }
            return list;
        }

        public List<Rental> GetByClientId(int clientId)
        {
            var list = new List<Rental>();
            string sql = @"
                SELECT r.ID, r.IDАвтомобиля, r.ДатаНачала, r.ДатаОкончанияПлановая, r.ДатаОкончанияФактическая, 
                       r.ИтоговаяСтоимость, r.СтоимостьПриАренде, r.Отзыв,
                       
                       -- Данные машины для объекта Car
                       a.Модель, a.ГосНомер, a.Фото,
                       m.Название as Марка, c.Название as Класс, c.ID as IDКласса
                FROM Аренда r
                JOIN Автомобиль a ON r.IDАвтомобиля = a.ID
                JOIN Марка m ON a.IDМарки = m.ID
                JOIN КлассАвтомобиля c ON a.IDКласса = c.ID
                WHERE r.IDКлиента = @ClientId";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ClientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var rental = new Rental
                {
                    Id = (int)reader["ID"],
                    CarId = (int)reader["IDАвтомобиля"],
                    StartDate = (DateTime)reader["ДатаНачала"],
                    PlannedEndDate = (DateTime)reader["ДатаОкончанияПлановая"],
                    ActualEndDate = reader["ДатаОкончанияФактическая"] as DateTime?,
                    TotalPrice = reader["ИтоговаяСтоимость"] as decimal?,
                    PriceAtRentalMoment = (decimal)reader["СтоимостьПриАренде"],
                    Review = reader["Отзыв"] as string
                };

                // ЗАПОЛНЯЕМ ВЛОЖЕННЫЙ ОБЪЕКТ CAR
                rental.Car = new Car
                {
                    Id = rental.CarId,
                    BrandName = reader["Марка"].ToString() ?? "",
                    Model = reader["Модель"].ToString() ?? "",
                    PlateNumber = reader["ГосНомер"].ToString() ?? "",
                    ClassName = reader["Класс"].ToString() ?? "",
                    ClassId = (int)reader["IDКласса"],
                    ImagePath = reader["Фото"] as string
                };

                list.Add(rental);
            }
            return list;
        }

        public List<RentalViewItem> GetRentalsView()
        {
            var list = new List<RentalViewItem>();

            // ВАЖНО: Убедитесь, что представление в базе называется именно так
            string sql = "SELECT * FROM Представление_Аренды ORDER BY ДатаНачала DESC";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new RentalViewItem
                {
                    Id = (int)reader["ID"],
                    ClientId = (int)reader["IDКлиента"],
                    CarId = (int)reader["IDАвтомобиля"],

                    DateStart = (DateTime)reader["ДатаНачала"],
                    DateEndPlanned = (DateTime)reader["ДатаОкончанияПлановая"],
                    DateEndActual = reader["ДатаОкончанияФактическая"] as DateTime?,

                    Status = reader["СтатусАренды"].ToString() ?? "",

                    // Внимательно с именами колонок из VIEW:
                    ClientFullName = reader["КлиентФИО"].ToString() ?? "",
                    ClientPhone = reader["КлиентТелефон"].ToString() ?? "",

                    CarName = reader["АвтоНазвание"].ToString() ?? "",
                    CarPlate = reader["ГосНомер"].ToString() ?? "",
                    CarPhoto = reader["АвтоФото"] as string,

                    EmployeeName = reader["СотрудникФИО"].ToString() ?? "",
                    EmployeePosition = reader["СотрудникДолжность"].ToString() ?? "",

                    TotalCost = (decimal)reader["ИтогоНачислено"],
                    Paid = (decimal)reader["Оплачено"],
                    Debt = (decimal)reader["Долг"]
                });
            }
            return list;
        }
    }
}
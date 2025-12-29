using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities; // Подключаем наши сущности

namespace CarRental.DAL.Repositories
{
    public class CarRepository : BaseRepository
    {
        // Метод для получения ВСЕХ автомобилей
        // Метод для получения ВСЕХ автомобилей (использует представление v_Cars_Catalog)
        public List<Car> GetAllCars()
        {
            var cars = new List<Car>();

            string sql = "SELECT * FROM Каталог_Машин ORDER BY МаркаНазвание, Модель";

            using (var connection = GetConnection())
            {
                connection.Open();
                using var command = new SqlCommand(sql, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    cars.Add(MapCar(reader));
                }
            }
            return cars;
        }

        // Получение списанных автомобилей
        // Получение списанных (Ищем статус 'Списан')
        public List<Car> GetWrittenOffCars()
        {
            var cars = new List<Car>();
            string sql = @"
                SELECT 
                    a.ID, a.Модель, a.ГосНомер, a.ГодВыпуска, a.Пробег, a.СтоимостьВСутки, a.Фото,
                    a.IDМарки, m.Название AS МаркаНазвание,
                    a.IDКласса, c.Название AS КлассНазвание,
                    a.IDСтатуса, s.Название AS СтатусНазвание,
                    a.IDТрансмиссии, t.Название AS КППНазвание,
                    a.IDТоплива, f.Название AS ТопливоНазвание,
                    a.IDКузова, b.Название AS КузовНазвание,
                    NULL AS ДатаСтраховки, NULL AS ДатаТО, NULL AS ТипТО
                FROM Автомобиль a
                    INNER JOIN Марка m ON a.IDМарки = m.ID
                    INNER JOIN КлассАвтомобиля c ON a.IDКласса = c.ID
                    INNER JOIN СтатусАвто s ON a.IDСтатуса = s.ID
                    INNER JOIN ТипТрансмиссии t ON a.IDТрансмиссии = t.ID
                    INNER JOIN ТипТоплива f ON a.IDТоплива = f.ID
                    INNER JOIN ТипКузова b ON a.IDКузова = b.ID
                WHERE s.Название = 'Списан'"; // Убедитесь, что в БД статус называется именно так

            using (var conn = GetConnection())
            {
                conn.Open();
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) cars.Add(MapCar(reader));
            }
            return cars;
        }

        // ВОССТАНОВЛЕНИЕ АВТО (Ставим статус "Свободен")
        public void RestoreCar(int carId)
        {
            // Находим ID статуса "Свободен" (обычно 1) или ставим хардкодом, если уверены
            string sql = @"
                UPDATE Автомобиль 
                SET IDСтатуса = (SELECT TOP 1 ID FROM СтатусАвто WHERE Название = 'Свободен') 
                WHERE ID = @Id";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", carId);
            cmd.ExecuteNonQuery();
        }

        // Метод добавления автомобиля
        public void AddCar(Car car)
        {
            string sql = @"
                INSERT INTO Автомобиль 
                (IDМарки, IDКласса, IDКузова, IDТрансмиссии, IDТоплива, IDСтатуса, 
                 Модель, ГосНомер, ГодВыпуска, Пробег, СтоимостьВСутки, Фото)
                VALUES 
                (@BrandId, @ClassId, @BodyId, @TransId, @FuelId, @StatusId, 
                 @Model, @Plate, @Year, @Mileage, @Price, @Photo)";

            using var connection = GetConnection();
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            // Добавляем параметры (защита от SQL-инъекций)
            command.Parameters.AddWithValue("@BrandId", car.BrandId);
            command.Parameters.AddWithValue("@ClassId", car.ClassId);
            command.Parameters.AddWithValue("@BodyId", car.BodyTypeId);
            command.Parameters.AddWithValue("@TransId", car.TransmissionId);
            command.Parameters.AddWithValue("@FuelId", car.FuelId);
            command.Parameters.AddWithValue("@StatusId", car.StatusId);

            command.Parameters.AddWithValue("@Model", car.Model);
            command.Parameters.AddWithValue("@Plate", car.PlateNumber);
            command.Parameters.AddWithValue("@Year", car.Year);
            command.Parameters.AddWithValue("@Mileage", car.Mileage);
            command.Parameters.AddWithValue("@Price", car.PricePerDay);

            // Обработка NULL для фото
            if (string.IsNullOrEmpty(car.ImagePath))
                command.Parameters.AddWithValue("@Photo", DBNull.Value);
            else
                command.Parameters.AddWithValue("@Photo", car.ImagePath);

            command.ExecuteNonQuery();
        }

        // Метод обновления автомобиля
        public void UpdateCar(Car car)
        {
            string sql = @"
                UPDATE Автомобиль 
                SET IDМарки = @BrandId,
                    IDКласса = @ClassId,
                    IDКузова = @BodyId,
                    IDТрансмиссии = @TransId,
                    IDТоплива = @FuelId,
                    IDСтатуса = @StatusId,
                    Модель = @Model,
                    ГосНомер = @Plate,
                    ГодВыпуска = @Year,
                    Пробег = @Mileage,
                    СтоимостьВСутки = @Price,
                    Фото = @Photo
                WHERE ID = @Id";

            using var connection = GetConnection();
            connection.Open();
            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", car.Id);

            // Внешние ключи
            command.Parameters.AddWithValue("@BrandId", car.BrandId);
            command.Parameters.AddWithValue("@ClassId", car.ClassId);
            command.Parameters.AddWithValue("@BodyId", car.BodyTypeId);
            command.Parameters.AddWithValue("@TransId", car.TransmissionId);
            command.Parameters.AddWithValue("@FuelId", car.FuelId);
            command.Parameters.AddWithValue("@StatusId", car.StatusId);

            // Основные поля
            command.Parameters.AddWithValue("@Model", car.Model);
            command.Parameters.AddWithValue("@Plate", car.PlateNumber);
            command.Parameters.AddWithValue("@Year", car.Year);
            command.Parameters.AddWithValue("@Mileage", car.Mileage);
            command.Parameters.AddWithValue("@Price", car.PricePerDay);

            if (string.IsNullOrEmpty(car.ImagePath))
                command.Parameters.AddWithValue("@Photo", DBNull.Value);
            else
                command.Parameters.AddWithValue("@Photo", car.ImagePath);

            command.ExecuteNonQuery();
        }

        // Поиск свободных машин на заданный период
        // Исправленный метод поиска (SQL остался тем же, но проверим его)
        // Добавили параметр currentRentalId (по умолчанию null)
        // Поиск свободных машин на заданный период
        // Использует табличную функцию fn_GetAvailableCars и представление v_Cars_Catalog
        public List<Car> GetAvailableCars(DateTime start, DateTime end, int? currentRentalId = null)
        {
            var list = new List<Car>();

            // Мы выбираем полные данные из VIEW только для тех ID, которые вернула ФУНКЦИЯ
            string sql = @"
                SELECT * FROM Каталог_Машин 
                WHERE ID IN (SELECT ID FROM fn_GetAvailableCars(@Start, @End, @CurrentRentalId))
                ORDER BY МаркаНазвание, Модель";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);
            // Передаем ID текущей аренды (для редактирования) или NULL (для создания)
            cmd.Parameters.AddWithValue("@CurrentRentalId", currentRentalId ?? (object)DBNull.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapCar(reader));
            }
            return list;
        }

        // ИСПРАВЛЕННЫЙ MapCar (Ключи совпадают с SQL)
        private Car MapCar(SqlDataReader reader)
        {
            return new Car
            {
                Id = (int)reader["ID"],
                BrandId = (int)reader["IDМарки"],
                ClassId = (int)reader["IDКласса"],
                BodyTypeId = (int)reader["IDКузова"],
                TransmissionId = (int)reader["IDТрансмиссии"],
                FuelId = (int)reader["IDТоплива"],
                StatusId = (int)reader["IDСтатуса"],

                Model = reader["Модель"].ToString() ?? "",
                PlateNumber = reader["ГосНомер"].ToString() ?? "",
                Year = (int)reader["ГодВыпуска"],
                Mileage = (int)reader["Пробег"],
                PricePerDay = (decimal)reader["СтоимостьВСутки"],
                ImagePath = reader["Фото"] as string,

                // ВОТ ЗДЕСЬ БЫЛА ОШИБКА. Теперь ключи совпадают с AS в запросе:
                BrandName = reader["МаркаНазвание"].ToString() ?? "",
                ClassName = reader["КлассНазвание"].ToString() ?? "",
                StatusName = reader["СтатусНазвание"].ToString() ?? "",
                TransmissionName = reader["КППНазвание"].ToString() ?? "",
                FuelName = reader["ТопливоНазвание"].ToString() ?? "",
                BodyTypeName = reader["КузовНазвание"].ToString() ?? "",

                InsuranceExpiryDate = reader["ДатаСтраховки"] as DateTime?,
                NextMaintenanceDate = reader["ДатаТО"] as DateTime?,
                NextMaintenanceType = reader["ТипТО"] as string
            };
        }
    }
}
using Microsoft.Data.SqlClient;
using CarRental.Domain.Entities; // Подключаем наши сущности

namespace CarRental.DAL.Repositories
{
    public class CarRepository : BaseRepository
    {
        // Метод для получения ВСЕХ автомобилей
        public List<Car> GetAllCars()
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
                    
                    -- Подзапрос: Последняя дата окончания страховки
                    (SELECT MAX(ДатаОкончания) FROM Страховка WHERE IDАвтомобиля = a.ID) AS ДатаСтраховки,
                    
                    -- Подзапрос: Ближайшая будущая дата обслуживания
                    (SELECT MIN(ДатаНачала) FROM Обслуживание WHERE IDАвтомобиля = a.ID AND ДатаНачала >= CAST(GETDATE() AS DATE)) AS ДатаТО,
                    
                    (SELECT TOP 1 ТипОбслуживания FROM Обслуживание WHERE IDАвтомобиля = a.ID AND ДатаНачала >= CAST(GETDATE() AS DATE) ORDER BY ДатаНачала) AS ТипТО

                FROM Автомобиль a
                    INNER JOIN Марка m ON a.IDМарки = m.ID
                    INNER JOIN КлассАвтомобиля c ON a.IDКласса = c.ID
                    INNER JOIN СтатусАвто s ON a.IDСтатуса = s.ID
                    INNER JOIN ТипТрансмиссии t ON a.IDТрансмиссии = t.ID
                    INNER JOIN ТипТоплива f ON a.IDТоплива = f.ID
                    INNER JOIN ТипКузова b ON a.IDКузова = b.ID";

            using (var connection = GetConnection())
            {
                connection.Open();
                using var command = new SqlCommand(sql, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var car = new Car
                    {
                        Id = (int)reader["ID"],
                        Model = reader["Модель"].ToString() ?? string.Empty,
                        PlateNumber = reader["ГосНомер"].ToString() ?? string.Empty,
                        Year = (int)reader["ГодВыпуска"],
                        Mileage = (int)reader["Пробег"],
                        PricePerDay = (decimal)reader["СтоимостьВСутки"],
                        ImagePath = reader["Фото"] as string, // Может быть null

                        // Внешние ключи
                        BrandId = (int)reader["IDМарки"],
                        ClassId = (int)reader["IDКласса"],
                        StatusId = (int)reader["IDСтатуса"],
                        TransmissionId = (int)reader["IDТрансмиссии"],
                        FuelId = (int)reader["IDТоплива"],
                        BodyTypeId = (int)reader["IDКузова"],

                        // Данные из справочников
                        BrandName = reader["МаркаНазвание"].ToString() ?? string.Empty,
                        ClassName = reader["КлассНазвание"].ToString() ?? string.Empty,
                        StatusName = reader["СтатусНазвание"].ToString() ?? string.Empty,
                        TransmissionName = reader["КППНазвание"].ToString() ?? string.Empty,
                        FuelName = reader["ТопливоНазвание"].ToString() ?? string.Empty,
                        BodyTypeName = reader["КузовНазвание"].ToString() ?? string.Empty,
                        InsuranceExpiryDate = reader["ДатаСтраховки"] as DateTime?,
                        NextMaintenanceDate = reader["ДатаТО"] as DateTime?,
                        NextMaintenanceType = reader["ТипТО"] as string
                    };
                    cars.Add(car);
                }
            }

            return cars;
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
        public List<Car> GetAvailableCars(DateTime start, DateTime end)
        {
            var list = new List<Car>();

            string sql = @"
                SELECT 
                    a.ID, a.Модель, a.ГосНомер, a.ГодВыпуска, a.Пробег, a.СтоимостьВСутки, a.Фото,
                    a.IDМарки, m.Название AS МаркаНазвание,
                    a.IDКласса, c.Название AS КлассНазвание,
                    a.IDСтатуса, s.Название AS СтатусНазвание,
                    a.IDТрансмиссии, t.Название AS КППНазвание,
                    a.IDТоплива, f.Название AS ТопливоНазвание,
                    a.IDКузова, b.Название AS КузовНазвание,
                    
                    (SELECT MAX(ДатаОкончания) FROM Страховка WHERE IDАвтомобиля = a.ID) AS ДатаСтраховки,
                    (SELECT MIN(ДатаНачала) FROM Обслуживание WHERE IDАвтомобиля = a.ID AND ДатаНачала >= CAST(GETDATE() AS DATE)) AS ДатаТО,
                    (SELECT TOP 1 ТипОбслуживания FROM Обслуживание WHERE IDАвтомобиля = a.ID AND ДатаНачала >= CAST(GETDATE() AS DATE) ORDER BY ДатаНачала) AS ТипТО

                FROM Автомобиль a
                    INNER JOIN Марка m ON a.IDМарки = m.ID
                    INNER JOIN КлассАвтомобиля c ON a.IDКласса = c.ID
                    INNER JOIN СтатусАвто s ON a.IDСтатуса = s.ID
                    INNER JOIN ТипТрансмиссии t ON a.IDТрансмиссии = t.ID
                    INNER JOIN ТипТоплива f ON a.IDТоплива = f.ID
                    INNER JOIN ТипКузова b ON a.IDКузова = b.ID
                WHERE a.IDСтатуса NOT IN (5, 6) -- Исключаем ремонт и списанные
                  AND a.ID NOT IN (
                        SELECT DISTINCT IDАвтомобиля FROM Аренда 
                        WHERE (ДатаНачала <= @End) AND (ISNULL(ДатаОкончанияФактическая, ДатаОкончанияПлановая) >= @Start)
                  )
                  AND a.ID NOT IN (
                        SELECT DISTINCT IDАвтомобиля FROM Бронирование
                        WHERE (ДатаНачала <= @End) AND (ДатаОкончания >= @Start)
                  )
                  AND a.ID NOT IN (
                        SELECT DISTINCT IDАвтомобиля FROM Обслуживание
                        WHERE (ДатаНачала <= @End) AND (ISNULL(ДатаОкончания, '2100-01-01') >= @Start)
                  )";

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);

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
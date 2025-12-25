namespace CarRental.Domain.Entities
{
    // Марка автомобиля (BMW, Audi)
    public class CarBrand
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Класс (Эконом, Бизнес)
    public class CarClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // Статус (Свободен, В ремонте)
    public class CarStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Тип топлива
    public class FuelType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Тип КПП
    public class TransmissionType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Тип кузова
    public class BodyType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
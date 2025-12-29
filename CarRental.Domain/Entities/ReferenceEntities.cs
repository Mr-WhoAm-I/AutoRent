namespace CarRental.Domain.Entities
{
    // Марка автомобиля (BMW, Audi)
    public class CarBrand
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // ВОТ ЭТО ИСПРАВЛЯЕТ ОТОБРАЖЕНИЕ В СПИСКЕ
        public override string ToString() => Name;
    }

    // Класс (Эконом, Бизнес)
    public class CarClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public override string ToString() => Name;
    }

    // Статус (Свободен, В ремонте)
    public class CarStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    // Тип топлива
    public class FuelType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    // Тип КПП
    public class TransmissionType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    // Тип кузова
    public class BodyType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    public class ReferenceItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } // Будет null для справочников без описания

        // Служебное поле, чтобы знать, из какой таблицы эта запись (нужно для сохранения)
        public string TableName { get; set; } = string.Empty;

        // Флаг для UI: есть ли у этой таблицы поле описания?
        public bool HasDescription { get; set; }
    }
}
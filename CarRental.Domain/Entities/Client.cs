using System;

namespace CarRental.Domain.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }

        public int? DrivingExperience { get; set; }
        public bool IsArchived { get; set; }

        // === СВОЙСТВА ДЛЯ ОТОБРАЖЕНИЯ ===

        public string FullName
        {
            get
            {
                string result = $"{Surname} {Name}";
                if (!string.IsNullOrEmpty(Patronymic))
                    result += $" {Patronymic}";
                return result;
            }
        }

        // Форматирование Возраста (берем готовое число Age и добавляем "лет")
        public string AgeString => Age.HasValue ? $"{Age} лет" : "-";

        // Для отображения стажа текстом
        public string ExperienceString => DrivingExperience.HasValue ? $"{DrivingExperience} лет" : "-";

        // Хелпер склонения (1 год, 2 года, 5 лет)
        private string GetYearSuffix(int num)
        {
            int lastDigit = num % 10;
            int lastTwoDigits = num % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 19) return "лет";
            if (lastDigit == 1) return "год";
            if (lastDigit >= 2 && lastDigit <= 4) return "года";
            return "лет";
        }
    }
}
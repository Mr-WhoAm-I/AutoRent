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

        public int? Age { get; set; }
        public int? DrivingExperience { get; set; }

        public bool IsArchived { get; set; }

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
    }
}
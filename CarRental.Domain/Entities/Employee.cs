namespace CarRental.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public int RoleId { get; set; }

        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public string FullName => $"{Surname} {Name} ({RoleName})";
    }
}
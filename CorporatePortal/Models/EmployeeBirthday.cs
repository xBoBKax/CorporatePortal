namespace CorporatePortal.Api.Models
{
    public class EmployeeBirthday
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public DateOnly BirthDate { get; set; }

        public string? Department { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

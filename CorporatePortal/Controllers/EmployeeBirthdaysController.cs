using CorporatePortal.Api.Data;
using CorporatePortal.Api.DTOs;
using CorporatePortal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorporatePortal.Api.Controllers
{
    [ApiController]
    [Route("api/employee-birthdays")]
    public class EmployeeBirthdaysController : ControllerBase
    {
        private readonly PortalDbContext db;

        public EmployeeBirthdaysController(PortalDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<EmployeeBirthdayDto>> GetBirthdays()
        {
            var employees = await db.EmployeeBirthdays
                .OrderBy(employee => employee.BirthDate.Month)
                .ThenBy(employee => employee.BirthDate.Day)
                .ThenBy(employee => employee.FullName)
                .ToArrayAsync();

            return employees.Select(employee => employee.ToDto()).ToArray();
        }

        [HttpGet("today")]
        public async Task<IReadOnlyCollection<EmployeeBirthdayDto>> GetToday()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var employees = await db.EmployeeBirthdays
                .Where(employee => employee.IsActive && employee.BirthDate.Month == today.Month && employee.BirthDate.Day == today.Day)
                .OrderBy(employee => employee.FullName)
                .ToArrayAsync();

            return employees.Select(employee => employee.ToDto()).ToArray();
        }

        [HttpGet("upcoming")]
        public async Task<IReadOnlyCollection<EmployeeBirthdayDto>> GetUpcoming([FromQuery] int days = 30)
        {
            days = Math.Clamp(days, 1, 366);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var employees = await db.EmployeeBirthdays.Where(employee => employee.IsActive).ToArrayAsync();

            return employees
                .Select(employee => new { Employee = employee, Days = DaysUntilNextBirthday(today, employee.BirthDate) })
                .Where(item => item.Days > 0 && item.Days <= days)
                .OrderBy(item => item.Days)
                .ThenBy(item => item.Employee.FullName)
                .Select(item => item.Employee.ToDto())
                .ToArray();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmployeeBirthdayDto>> GetBirthday(int id)
        {
            var employee = await db.EmployeeBirthdays.FindAsync(id);
            return employee is null ? NotFound() : employee.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeBirthdayDto>> CreateBirthday(UpsertEmployeeBirthdayRequest request)
        {
            var employee = new EmployeeBirthday();
            Apply(request, employee);
            db.EmployeeBirthdays.Add(employee);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBirthday), new { id = employee.Id }, employee.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EmployeeBirthdayDto>> UpdateBirthday(int id, UpsertEmployeeBirthdayRequest request)
        {
            var employee = await db.EmployeeBirthdays.FindAsync(id);
            if (employee is null)
            {
                return NotFound();
            }

            Apply(request, employee);
            await db.SaveChangesAsync();
            return employee.ToDto();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBirthday(int id)
        {
            var employee = await db.EmployeeBirthdays.FindAsync(id);
            if (employee is null)
            {
                return NotFound();
            }

            db.EmployeeBirthdays.Remove(employee);
            await db.SaveChangesAsync();
            return NoContent();
        }

        private static int DaysUntilNextBirthday(DateOnly today, DateOnly birthday)
        {
            var next = new DateOnly(today.Year, birthday.Month, birthday.Day);
            if (next < today)
            {
                next = next.AddYears(1);
            }

            return next.DayNumber - today.DayNumber;
        }

        private static void Apply(UpsertEmployeeBirthdayRequest request, EmployeeBirthday employee)
        {
            employee.FullName = request.FullName.Trim();
            employee.BirthDate = request.BirthDate;
            employee.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim();
            employee.IsActive = request.IsActive;
        }
    }
}

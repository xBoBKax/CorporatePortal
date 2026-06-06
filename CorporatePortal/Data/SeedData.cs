using CorporatePortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CorporatePortal.Api.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
            await db.Database.EnsureCreatedAsync();

            if (await db.PortalPages.AnyAsync())
            {
                return;
            }

            var page = new PortalPage
            {
                Title = "Корпоративная информационная страница",
                WelcomeText = "Добро пожаловать на страницу интернет-ресурсов компании.",
                LogoUrl = "/images/logo-placeholder.svg",
                BackgroundType = "decorative",
                BackgroundValue = "yellow-blue",
                IsActive = true,
                LinkGroups = new List<LinkGroup>
                {
                    Group("О компании", 10, "🏢", new[]
                    {
                        Link("Видение компании", "#"),
                        Link("Миссия компании", "#"),
                        Link("Цели компании", "#"),
                        Link("Социальный пакет ГК ЭРИС", "#"),
                        Link("Политики компании", "#"),
                        Link("Корпоративная библиотека", "#")
                    }),
                    Group("Соц. сети", 20, "💬", new[]
                    {
                        Link("Telegram-канал", "https://telegram.org"),
                        Link("VK.com", "https://vk.com")
                    }),
                    Group("Отчетные системы", 30, "📊", new[] { Link("Производство", "#") }),
                    Group("Корпоративные сайты", 40, "🌐", new[]
                    {
                        Link("eriskip.com", "https://eriskip.com"),
                        Link("gas-control.ru", "https://gas-control.ru")
                    }),
                    Group("Нейронные сети", 50, "🧠", new[]
                    {
                        Link("Нейронные сети и промпты", "#"),
                        Link("Левша AI", "#")
                    }),
                    Group("Обучение", 60, "🎓", new[]
                    {
                        Link("Онлайн трансляции", "#"),
                        Link("Календарь обучений", "#"),
                        Link("SCRUM", "#")
                    }),
                    Group("Развлечения", 70, "🎭", new[] { Link("Календарь мероприятий", "#") }),
                    Group("Юридические справочники", 80, "⚖️", new[] { Link("Кодексы РФ", "#") }),
                    Group("Школьникам/студентам", 90, "📚", new[]
                    {
                        Link("Образование", "#"),
                        Link("Наставничество", "#"),
                        Link("Анкетирование", "#")
                    }),
                    Group("Общение и работа", 100, "💼", new[]
                    {
                        Link("Почта Eris", "#"),
                        Link("1С:Кабинет сотрудника", "#"),
                        Link("Инструкции Eris", "#"),
                        Link("Проведение совещаний", "#"),
                        Link("ELMA ERIS", "#"),
                        Link("1C-Web", "#"),
                        Link("Википедия ЭРИС", "#"),
                        Link("Музей ЭРИС", "#"),
                        Link("Телефонный справочник", "#"),
                        Link("Отчетная система", "#"),
                        Link("Календарь маркетинговых событий", "#"),
                        Link("Техэксперт Инструкция", "#"),
                        Link("Командировка Сервис", "#"),
                        Link("Онлайн конфигураторы", "#")
                    })
                },
                Modules = new List<PortalModule>
                {
                    new()
                    {
                        Type = "ImportantInfo",
                        Title = "Важная информация",
                        Content = "1. Удовлетворять потребности общества в обеспечении производственной и экологической безопасности; 2. Улучшать качество процессов и корпоративных сервисов.",
                        SortOrder = 10
                    },
                    new()
                    {
                        Type = "Birthdays",
                        Title = "Дни рождения",
                        Content = "Поздравляем коллег и желаем новых профессиональных побед!",
                        SortOrder = 20
                    },
                    new()
                    {
                        Type = "Weather",
                        Title = "Прогноз погоды",
                        Content = "Настройте город и источник погодного виджета в админке.",
                        SettingsJson = "{\"city\":\"Ваш город\",\"temperature\":\"+14°\",\"summary\":\"Облачно с прояснениями\"}",
                        SortOrder = 30
                    }
                }
            };

            db.PortalPages.Add(page);
            db.EmployeeBirthdays.AddRange(
                new EmployeeBirthday { FullName = "Егор Михайлович", BirthDate = new DateOnly(1990, 6, 6), Department = "Производство" },
                new EmployeeBirthday { FullName = "Чухланцев Виктор Олегович", BirthDate = new DateOnly(1988, 6, 10), Department = "ИТ" },
                new EmployeeBirthday { FullName = "Короткова Ольга Анатольевна", BirthDate = new DateOnly(1992, 6, 15), Department = "Маркетинг" });

            await db.SaveChangesAsync();
        }

        private static LinkGroup Group(string title, int sortOrder, string icon, IEnumerable<PortalLink> links)
        {
            var orderedLinks = links.Select((link, index) =>
            {
                link.SortOrder = (index + 1) * 10;
                return link;
            }).ToList();

            return new LinkGroup
            {
                Title = title,
                Icon = icon,
                SortOrder = sortOrder,
                IsVisible = true,
                Links = orderedLinks
            };
        }

        private static PortalLink Link(string title, string url)
        {
            return new PortalLink
            {
                Title = title,
                Url = url,
                IsActive = true,
                OpenInNewTab = url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}

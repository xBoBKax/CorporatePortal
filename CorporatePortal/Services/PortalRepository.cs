using CorporatePortal.Api.Models;

namespace CorporatePortal.Api.Services;

public class PortalRepository
{
    private readonly object _sync = new();
    private readonly List<PortalPage> _pages;
    private int _pageId = 2;
    private int _groupId = 3;
    private int _linkId = 6;
    

    public PortalRepository()
    {
        _pages =
        [
            new PortalPage
            {
                Id = 1,
                Title = "Общий портал",
                WelcomeText = "Добро пожаловать на корпоративный портал компании.",
                BackgroundType = "color",
                BackgroundValue = "#f2f4f8",
                ThemeCss = ":root{--accent:#1d4ed8;--card-bg:rgba(255,255,255,.9);}",
                Widgets = [new WidgetConfig{Id=1,Type="clock",Title="Часы",SortOrder=1},new WidgetConfig{Id=2,Type="date",Title="Дата",SortOrder=2},new WidgetConfig{Id=7,Type="embed",Title="Компания TV",SortOrder=3,EmbedHtml="<iframe src="https://example.com" width="100%" height="180"></iframe>"}],
                LinkGroups =
                [
                    new LinkGroup { Id = 1, PortalPageId = 1, Title = "Корпоративные сервисы", SortOrder = 1, Links = [new PortalLink { Id = 1, LinkGroupId = 1, Title = "Почта", Url = "https://mail.example.com", SortOrder = 1 }, new PortalLink { Id = 2, LinkGroupId = 1, Title = "База знаний", Url = "https://wiki.example.com", SortOrder = 2 }]},
                    new LinkGroup { Id = 2, PortalPageId = 1, Title = "Работа и коммуникации", SortOrder = 2, Links = [new PortalLink { Id = 3, LinkGroupId = 2, Title = "Задачи", Url = "https://tasks.example.com", SortOrder = 1 }, new PortalLink { Id = 4, LinkGroupId = 2, Title = "Телефонный справочник", Url = "https://phonebook.example.com", SortOrder = 2 }]}
                ]
            },
            new PortalPage
            {
                Id = 2,
                Title = "IT-страница",
                WelcomeText = "Ресурсы для IT-отдела.",
                BackgroundType = "gradient",
                BackgroundValue = "linear-gradient(120deg,#0f172a,#1d4ed8)",
                ThemeCss = ":root{--accent:#0ea5e9;--card-bg:rgba(15,23,42,.65);}",
                Widgets = [new WidgetConfig{Id=3,Type="weather",Title="Погода (заглушка)",SortOrder=1},new WidgetConfig{Id=4,Type="quote",Title="Цитата дня",SortOrder=2}],
                LinkGroups = [new LinkGroup { Id = 3, PortalPageId = 2, Title = "Инфраструктура", SortOrder = 1, Links = [new PortalLink { Id = 5, LinkGroupId = 3, Title = "Мониторинг", Url = "https://zabbix.example.com", SortOrder = 1 }, new PortalLink { Id = 6, LinkGroupId = 3, Title = "GitLab", Url = "https://gitlab.example.com", SortOrder = 2 }]}]
            }
        ];
    }
    public IReadOnlyList<PortalPage> GetPages(){lock(_sync){return _pages.Select(ClonePage).ToList();}}
    public PortalPage? GetPage(int id){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==id);return p is null?null:ClonePage(p);}}
    public PortalPage CreatePage(PortalPage request){lock(_sync){var p=new PortalPage{Id=++_pageId,Title=request.Title,WelcomeText=request.WelcomeText,LogoUrl=request.LogoUrl,BackgroundType=request.BackgroundType,BackgroundValue=request.BackgroundValue,IsActive=request.IsActive,ThemeCss=request.ThemeCss};_pages.Add(p);return ClonePage(p);}}
    public PortalPage? UpdatePage(int id, PortalPage request){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==id);if(p is null)return null;p.Title=request.Title;p.WelcomeText=request.WelcomeText;p.LogoUrl=request.LogoUrl;p.BackgroundType=request.BackgroundType;p.BackgroundValue=request.BackgroundValue;p.IsActive=request.IsActive;p.ThemeCss=request.ThemeCss;p.Widgets=request.Widgets.Select(CloneWidget).OrderBy(x=>x.SortOrder).ToList();return ClonePage(p);}}
    public bool DeletePage(int id){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==id);return p is not null&&_pages.Remove(p);}}
    public LinkGroup? AddGroup(int pageId, LinkGroup group){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==pageId);if(p is null)return null;var i=new LinkGroup{Id=++_groupId,PortalPageId=pageId,Title=group.Title,Description=group.Description,Icon=group.Icon,SortOrder=group.SortOrder,IsVisible=group.IsVisible};p.LinkGroups.Add(i);return CloneGroup(i);}}
    public PortalLink? AddLink(int pageId, int groupId, PortalLink link){lock(_sync){var g=_pages.FirstOrDefault(p=>p.Id==pageId)?.LinkGroups.FirstOrDefault(g=>g.Id==groupId);if(g is null)return null;var i=new PortalLink{Id=++_linkId,LinkGroupId=groupId,Title=link.Title,Url=link.Url,Description=link.Description,Icon=link.Icon,SortOrder=link.SortOrder,IsActive=link.IsActive,OpenInNewTab=link.OpenInNewTab};g.Links.Add(i);return CloneLink(i);}}
    public PortalPage? ReorderGroups(int pageId, List<int> groupIds){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==pageId);if(p is null)return null;for(int i=0;i<groupIds.Count;i++){var g=p.LinkGroups.FirstOrDefault(x=>x.Id==groupIds[i]);if(g is not null)g.SortOrder=i+1;}p.LinkGroups=p.LinkGroups.OrderBy(x=>x.SortOrder).ToList();return ClonePage(p);}}
    public PortalPage? ReorderLinks(int pageId,int groupId,List<int> linkIds){lock(_sync){var g=_pages.FirstOrDefault(p=>p.Id==pageId)?.LinkGroups.FirstOrDefault(x=>x.Id==groupId);if(g is null)return null;for(int i=0;i<linkIds.Count;i++){var l=g.Links.FirstOrDefault(x=>x.Id==linkIds[i]);if(l is not null)l.SortOrder=i+1;}g.Links=g.Links.OrderBy(x=>x.SortOrder).ToList();return ClonePage(_pages.First(p=>p.Id==pageId));}}
    private static PortalPage ClonePage(PortalPage s)=>new(){Id=s.Id,Title=s.Title,WelcomeText=s.WelcomeText,LogoUrl=s.LogoUrl,BackgroundType=s.BackgroundType,BackgroundValue=s.BackgroundValue,IsActive=s.IsActive,ThemeCss=s.ThemeCss,Widgets=s.Widgets.Select(CloneWidget).ToList(),LinkGroups=s.LinkGroups.Select(CloneGroup).ToList()};
    private static LinkGroup CloneGroup(LinkGroup s)=>new(){Id=s.Id,PortalPageId=s.PortalPageId,Title=s.Title,Description=s.Description,Icon=s.Icon,SortOrder=s.SortOrder,IsVisible=s.IsVisible,Links=s.Links.Select(CloneLink).ToList()};
    private static PortalLink CloneLink(PortalLink s)=>new(){Id=s.Id,LinkGroupId=s.LinkGroupId,Title=s.Title,Url=s.Url,Description=s.Description,Icon=s.Icon,SortOrder=s.SortOrder,IsActive=s.IsActive,OpenInNewTab=s.OpenInNewTab};
    private static WidgetConfig CloneWidget(WidgetConfig s)=>new(){Id=s.Id,Type=s.Type,Title=s.Title,SortOrder=s.SortOrder,IsVisible=s.IsVisible,EmbedHtml=s.EmbedHtml};
}

using Domain.Constants.User;
using Domain.Entities;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Persistence;

public class GarneauTemplateDbContextInitializer
{
    private const string MemberEmail = "member@gmail.com";
    private const string AdminEmail = "admin@gmail.com";
    private const string Password = "Qwerty123!";

    private readonly ILogger<GarneauTemplateDbContextInitializer> _logger;
    private readonly GarneauTemplateDbContext _context;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public GarneauTemplateDbContextInitializer(ILogger<GarneauTemplateDbContextInitializer> logger,
        GarneauTemplateDbContext context,
        RoleManager<Role> roleManager,
        UserManager<User> userManager)
    {
        _logger = logger;
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRoles();
            await SeedAdmins();
            await SeedMembers();
            await SeedPages();
            await SeedMenus();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRoles()
    {
        if (!_roleManager.RoleExistsAsync(Roles.ADMINISTRATOR).Result)
            await _roleManager.CreateAsync(new Role { Name = Roles.ADMINISTRATOR, NormalizedName = Roles.ADMINISTRATOR.Normalize() });

        if (!_roleManager.RoleExistsAsync(Roles.MEMBER).Result)
            await _roleManager.CreateAsync(new Role { Name = Roles.MEMBER, NormalizedName = Roles.MEMBER.Normalize() });
    }

    private async Task SeedAdmins()
    {
        var user = await _userManager.FindByEmailAsync(AdminEmail);
        if (user != null)
            return;

        user = BuildUser(AdminEmail);
        var result = await _userManager.CreateAsync(user, Password);

        if (result.Succeeded)
            await _userManager.AddToRoleAsync(user, Roles.ADMINISTRATOR);
        else
            throw new Exception($"Could not seed/create {Roles.ADMINISTRATOR} user.");


        var admin = new Administrator("Super", "Admin");
        admin.SetUser(user);
        _context.Administrators.Add(admin);
        await _context.SaveChangesAsync();
    }

    private async Task SeedMembers()
    {
        var user = await _userManager.FindByEmailAsync(MemberEmail);
        if (user != null)
            return;

        user = BuildUser(MemberEmail);
        var result = await _userManager.CreateAsync(user, Password);

        if (result.Succeeded)
            await _userManager.AddToRoleAsync(user, Roles.MEMBER);
        else
            throw new Exception($"Could not seed/create {Roles.MEMBER} user.");

        var existingMember = _context.Members.IgnoreQueryFilters().FirstOrDefault(x => x.User.Id == user.Id);
        if (existingMember is { Active: true })
            return;

        if (existingMember == null)
        {
            var member = new Member("John", "Doe", 1, "123, my street", "Quebec", "A1A 1A1");
            member.SetUser(user);
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
        }
        else if (!existingMember.Active)
        {
            existingMember.Activate();
            _context.Members.Update(existingMember);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedPages()
    {
        if (_context.Pages.Any())
            return;

        var pages = new List<Page>
        {
            CreatePage("Notre école", "notre-ecole", 1,
                "<h2>Bienvenue chez Expression Danse de Beauport</h2>" +
                "<p>Fondée avec la passion de la danse, Expression Danse de Beauport offre un environnement chaleureux " +
                "et stimulant pour les danseurs de tous les âges et de tous les niveaux.</p>" +
                "<p>Notre équipe de professeurs qualifiés et passionnés s'engage à transmettre " +
                "l'amour de la danse à travers un enseignement de qualité.</p>" +
                "<h3>Notre mission</h3>" +
                "<p>Rendre la danse accessible à tous et permettre à chacun de s'épanouir à travers le mouvement.</p>"),

            CreatePage("Récréatif", "recreatif", 2,
                "<h2>Cours récréatifs</h2>" +
                "<p>Nos cours récréatifs sont offerts pour tous les groupes d'âge, des tout-petits aux adultes. " +
                "Venez découvrir le plaisir de danser dans une ambiance décontractée et amusante.</p>" +
                "<h3>Styles offerts</h3>" +
                "<ul>" +
                "<li>Ballet</li>" +
                "<li>Jazz</li>" +
                "<li>Hip-hop</li>" +
                "<li>Contemporain</li>" +
                "<li>Danse créative (3-5 ans)</li>" +
                "</ul>"),

            CreatePage("Camp d'été", "camp-d-ete", 3,
                "<h2>Camp d'été de danse</h2>" +
                "<p>Chaque été, Expression Danse de Beauport propose un camp de jour axé sur la danse. " +
                "Une semaine remplie de plaisir, d'apprentissage et de spectacle!</p>" +
                "<h3>Ce qui est inclus</h3>" +
                "<ul>" +
                "<li>Cours de danse variés chaque jour</li>" +
                "<li>Activités thématiques et jeux</li>" +
                "<li>Spectacle de fin de semaine pour les parents</li>" +
                "</ul>"),

            CreatePage("Troupes compétitives", "troupes-competitives", 4,
                "<h2>Troupes compétitives</h2>" +
                "<p>Pour les danseurs qui souhaitent aller plus loin, nos troupes compétitives participent " +
                "à plusieurs compétitions régionales et provinciales tout au long de l'année.</p>" +
                "<h3>Auditions</h3>" +
                "<p>Les auditions ont lieu chaque année en juin. Contactez-nous pour plus de détails.</p>"),

            CreatePage("Nous joindre", "nous-joindre", 5,
                "<h2>Nous joindre</h2>" +
                "<p><strong>Adresse :</strong> 15, rue de la Promenade-des-Soeurs, Beauport, QC G1C 0G3</p>" +
                "<p><strong>Téléphone :</strong> <a href=\"tel:4186601086\">418-660-1086</a></p>" +
                "<p><strong>Courriel :</strong> <a href=\"mailto:info@expressiondansebeauport.com\">info@expressiondansebeauport.com</a></p>" +
                "<h3>Heures d'ouverture</h3>" +
                "<p>Lundi au vendredi : 16h00 - 21h00<br/>Samedi : 9h00 - 16h00<br/>Dimanche : Fermé</p>")
        };

        _context.Pages.AddRange(pages);
        await _context.SaveChangesAsync();
    }

    private static Page CreatePage(string title, string slug, int sortOrder, string content)
    {
        var page = new Page(title, slug);
        page.SetContent(content);
        page.SetSortOrder(sortOrder);
        page.Publish();
        return page;
    }

    private async Task SeedMenus()
    {
        if (_context.NavigationMenus.Any())
            return;

        var primaryMenu = new NavigationMenu("Menu principal", MenuLocation.Primary);
        _context.NavigationMenus.Add(primaryMenu);
        await _context.SaveChangesAsync();

        var pages = await _context.Pages.OrderBy(p => p.SortOrder).ToListAsync();
        var sortOrder = 0;
        foreach (var page in pages)
        {
            var item = new NavigationMenuItem(primaryMenu.Id, page.Title, sortOrder++);
            item.SetPageId(page.Id);
            item.SetUrl($"/{page.Slug}");
            _context.NavigationMenuItems.Add(item);
        }
        await _context.SaveChangesAsync();
    }

    private User BuildUser(string email)
    {
        return new User
        {
            Email = email,
            UserName = email,
            NormalizedEmail = email.Normalize(),
            NormalizedUserName = email,
            PhoneNumber = "555-555-5555",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false
        };
    }
}
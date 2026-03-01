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
                "<div class='camp-hero'>" +
                    "<div>" +
                        "<h2>Camp d'été 2026</h2>" +
                        "<p>Un été rempli de danse, d’énergie et de plaisir pour les 5 à 12 ans !</p>" +
                        "<a href='https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session' target='_blank' class='btn-camp'>S'inscrire maintenant</a>" +
                    "</div>" +
                "</div>" +

                "<section>" +
                    "<p>Votre enfant adore la danse et ne peut s’arrêter pendant les vacances? " +
                    "Offrez-lui un camp spécialisé, à l’écoute de sa passion, pour continuer à danser tout l’été!</p>" +
                "</section>" +

                "<section class='camp-highlight'>" +
                    "<h2>Dates & Horaire</h2>" +
                    "<ul>" +
                        "<li><strong>29 juin au 21 août 2026</strong></li>" +
                        "<li>*Pas de camp le 1 juillet</li>" +
                        "<li>9h à 16h</li>" +
                        "<li>Service de garde inclus : 7h30 à 9h / 16h à 17h</li>" +
                    "</ul>" +
                "</section>" +

                "<section class='camp-highlight'>" +
                    "<h2>Tarification</h2>" +
                    "<ul>" +
                        "<li>207$ / semaine</li>" +
                        "<li>10$ de rabais à partir de la 4e semaine achetée</li>" +
                    "</ul>" +
                "</section>" +

                "<section>" +
                    "<h2>Le camp d’été EDB, c’est quoi?</h2>" +
                    "<div class='camp-cards'>" +

                        "<div class='camp-card'>" +
                            "<h3>Danse tous les jours</h3>" +
                            "<p>Encadré par des professionnels de la danse.</p>" +
                        "</div>" +

                        "<div class='camp-card'>" +
                            "<h3>Activités extérieures</h3>" +
                            "<p>Baignade et jeux lors des belles journées.</p>" +
                        "</div>" +

                        "<div class='camp-card'>" +
                            "<h3>Spectacle chaque vendredi</h3>" +
                            "<p>Présentation ou vidéo personnalisée.</p>" +
                        "</div>" +

                        "<div class='camp-card'>" +
                            "<h3>Sécurité & encadrement</h3>" +
                            "<p>Présence constante d’adultes responsables.</p>" +
                        "</div>" +

                    "</div>" +
                "</section>" +

                "<section class='camp-highlight'>" +
                    "<h2>Inscriptions</h2>" +
                    "<ul>" +
                        "<li>Pré-inscriptions : 1 au 28 février 2026 (camp 2025 seulement)</li>" +
                        "<li>Inscriptions générales : dès le 1 mars 2026</li>" +
                    "</ul>" +
                "</section>" +

                "<section>" +
                    "<h2>Questions ?</h2>" +
                    "<p>Écrivez-nous à : <strong>info@expressiondansebeauport.com</strong></p>" +
                    "<p><small>*Tous les danseurs doivent être propres et aller seuls aux toilettes.</small></p>" +
                "</section>"
            ),

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
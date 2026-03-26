using Domain.Constants.User;
using Domain.Entities;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Persistence;

public class GarneauTemplateDbContextInitializer
{
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
            await SeedPages();
            await SeedMenus();
            await AssignAvatarColorsToExistingMembers();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRoles()
    {
        if (!await _roleManager.RoleExistsAsync(Roles.ADMINISTRATOR))
            await _roleManager.CreateAsync(new Role { Name = Roles.ADMINISTRATOR, NormalizedName = Roles.ADMINISTRATOR.Normalize() });

        if (!await _roleManager.RoleExistsAsync(Roles.PROFESSOR))
            await _roleManager.CreateAsync(new Role { Name = Roles.PROFESSOR, NormalizedName = Roles.PROFESSOR.Normalize() });

        if (!await _roleManager.RoleExistsAsync(Roles.MEMBER))
            await _roleManager.CreateAsync(new Role { Name = Roles.MEMBER, NormalizedName = Roles.MEMBER.Normalize() });
    }

    private async Task SeedAdmins()
    {
        var user = await _userManager.FindByEmailAsync(AdminEmail);
        if (user != null)
        {
            // Ensure admin has a Member record (may be missing if DB was seeded before social platform)
            if (!_context.Members.Any(m => m.UserId == user.Id))
            {
                var existingAdminMember = new Member("Super", "Admin");
                existingAdminMember.SetUser(user);
                _context.Members.Add(existingAdminMember);
                await _context.SaveChangesAsync();
            }
            return;
        }

        user = BuildUser(AdminEmail);
        var result = await _userManager.CreateAsync(user, Password);

        if (result.Succeeded)
            await _userManager.AddToRoleAsync(user, Roles.ADMINISTRATOR);
        else
            throw new Exception($"Could not seed/create {Roles.ADMINISTRATOR} user.");


        var admin = new Administrator("Super", "Admin");
        admin.SetUser(user);
        _context.Administrators.Add(admin);

        var adminMember = new Member("Super", "Admin");
        adminMember.SetUser(user);
        _context.Members.Add(adminMember);

        await _context.SaveChangesAsync();
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

            CreatePage("Camp d’été", "camp-d-ete", 3,
                "<div class=’camp-hero’>" +
                    "<div>" +
                        "<p>Un été rempli de danse, d’énergie et de plaisir pour les 5 à 12 ans !</p>" +
                        "<a href=’https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session’ target=’_blank’ class=’btn-camp’>S’inscrire maintenant</a>" +
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
                "</section>",
                ".public-page__container { max-width: 1100px; } " +
                ".public-page__title { text-align: center; font-size: 2.8rem; margin-bottom: 2.5rem; } " +
                ".public-page__content { display: flex; flex-direction: column; gap: 3rem; } " +
                ".public-page__content h2 { font-size: 1.6rem; margin-bottom: 1rem; color: #be1e2c; } " +
                ".camp-hero { background: #f4f6f8; border-radius: 16px; display: flex; align-items: center; justify-content: center; text-align: center; padding: 3rem 2rem; } " +
                ".camp-hero p { font-size: 1.2rem; color: #444; margin-bottom: 0.5rem; } " +
                ".btn-camp { display: inline-block; background: #be1e2c; color: white; padding: 14px 28px; border-radius: 10px; font-weight: bold; text-decoration: none; margin-top: 1rem; transition: 0.3s; } " +
                ".btn-camp:hover { background: #9e1824; } " +
                ".camp-cards { display: flex; gap: 1.5rem; justify-content: center; flex-wrap: wrap; margin-top: 2rem; } " +
                ".camp-card { background: #f4f6f8; padding: 2rem 1.5rem; flex: 1 1 220px; max-width: 260px; border-radius: 12px; text-align: center; transition: transform 0.2s ease; } " +
                ".camp-card:hover { transform: translateY(-4px); } " +
                ".camp-card h3 { color: #be1e2c; margin-bottom: 0.5rem; font-size: 1.1rem; } " +
                ".camp-card p { color: #555; font-size: 0.95rem; } " +
                ".camp-highlight { background: #f4f6f8; padding: 2rem; border-radius: 12px; } " +
                ".camp-highlight ul { list-style: none; padding: 0; } " +
                ".camp-highlight li { margin: 0.6rem 0; font-size: 1.05rem; } " +
                "strong { color: #be1e2c; } " +
                "section { padding-bottom: 0.5rem; }"
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

    private static Page CreatePage(string title, string slug, int sortOrder, string content, string? customCss = null)
    {
        var page = new Page(title, slug);
        page.SetContent(content);
        page.SetCustomCss(customCss);
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

    private async Task AssignAvatarColorsToExistingMembers()
    {
        // Specific color assignments
        var colorMap = new Dictionary<string, string>
        {
            { "Alexandre", "#38a169" },  // vert
            { "Adam", "#5a67d8" },       // indigo
            { "Super", "#d53f8c" },      // rose (admin)
        };

        foreach (var (firstName, color) in colorMap)
        {
            var member = _context.Members.FirstOrDefault(m => m.FirstName == firstName);
            if (member != null && member.AvatarColor != color)
            {
                member.SetAvatarColor(color);
            }
        }

        // Assign random colors to any remaining members with default color
        var membersWithoutColor = _context.Members
            .Where(m => m.AvatarColor == "#1a1a1a" || m.AvatarColor == null)
            .ToList();

        foreach (var member in membersWithoutColor)
        {
            member.AssignRandomAvatarColor();
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
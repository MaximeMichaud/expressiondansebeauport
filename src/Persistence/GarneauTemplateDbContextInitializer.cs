using System.Text.RegularExpressions;
using Domain.Constants.User;
using Domain.Entities;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Persistence;

public class GarneauTemplateDbContextInitializer
{
    private const string AdminEmail = "admin@gmail.com";
    private const string Password = "Qwerty123!";

    private readonly ILogger<GarneauTemplateDbContextInitializer> _logger;
    private readonly GarneauTemplateDbContext _context;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IHostEnvironment _environment;

    public GarneauTemplateDbContextInitializer(ILogger<GarneauTemplateDbContextInitializer> logger,
        GarneauTemplateDbContext context,
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IHostEnvironment environment)
    {
        _logger = logger;
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task InitialiseAsync()
    {
        const int maxRetries = 10;
        const int delayMs = 2000;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _context.Database.MigrateAsync();
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{MaxRetries}), retrying in {Delay}ms...",
                    attempt, maxRetries, delayMs);
                await Task.Delay(delayMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRoles();
            await SeedAdmins();
            await SeedPages();
            await SeedHelpArticles();
            await SeedMenus();
            await FixMenuHierarchy();
            await SeedSiteSettings();
            await AssignAvatarColorsToExistingMembers();
            if (_environment.IsDevelopment())
                await SeedDemoBlocksPage();
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
        if (!_context.Pages.Any())
        {
            var pages = new List<Page>
            {
                CreatePage("Notre école", "notre-ecole", 1, EcolePageContent(), EcolePageCss()),

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
                "<div class='camp-hero'>" +
                    "<div>" +
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
                "<p>Pour toutes questions ou informations supplémentaires, n'hésitez surtout pas à nous joindre.</p>" +
                "<p><strong>Téléphone :</strong> <a href=\"tel:4186666158\">418-666-6158</a></p>" +
                "<p><strong>Courriel :</strong> <a href=\"mailto:info@expressiondansebeauport.com\">info@expressiondansebeauport.com</a></p>" +
                "<h3>Nos locaux</h3>" +
                "<p>Centre de loisirs Ste-Gertrude<br/>788, avenue du Cénacle</p>" +
                "<h3>Adresse postale</h3>" +
                "<p>CP 29009 QUÉ CP RAYMOND PO<br/>G1B 3G0, Québec, QC</p>")
        };

            _context.Pages.AddRange(pages);
            await _context.SaveChangesAsync();
        }

        await SeedNousJoindreBlocks();

        if (_environment.IsDevelopment())
        {
            var notreEcole = _context.Pages.FirstOrDefault(p => p.Slug == "notre-ecole");
            if (notreEcole != null)
            {
                notreEcole.SetContent(EcolePageContent());
                notreEcole.SetCustomCss(EcolePageCss());
                await _context.SaveChangesAsync();
            }

            const string correctRegistrationUrl = "https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session";
            var campEte = _context.Pages.FirstOrDefault(p => p.Slug == "camp-d-ete");
            if (campEte != null)
            {
                campEte.SetCustomCss(CampPageCss());

                if (campEte.Content != null && !campEte.Content.Contains(correctRegistrationUrl))
                {
                    var fixedContent = System.Text.RegularExpressions.Regex.Replace(
                        campEte.Content,
                        @"<a[^>]+btn-camp[^>]*>[^<]*</a>",
                        $"<a href='{correctRegistrationUrl}' target='_blank' class='btn-camp'>S'inscrire maintenant</a>");
                    campEte.SetContent(fixedContent);
                }

                await _context.SaveChangesAsync();
            }
        }

        await SeedPageIfNotExists("Camp d'hiver", "camp-d-hiver", 6,
            "<div class='camp-hero'>" +
                "<div>" +
                    "<p>Un hiver rempli de danse, de créativité et de plaisir pour les 5 à 12 ans !</p>" +
                    "<a href='https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session' target='_blank' class='btn-camp'>S'inscrire maintenant</a>" +
                "</div>" +
            "</div>" +

            "<section>" +
                "<p>La relâche scolaire approche et votre enfant adore la danse ? " +
                "Offrez-lui une semaine inoubliable avec notre camp d'hiver spécialisé !</p>" +
            "</section>" +

            "<section class='camp-highlight'>" +
                "<h2>Dates & Horaire</h2>" +
                "<ul>" +
                    "<li><strong>Semaine de relâche scolaire 2027</strong></li>" +
                    "<li>9h à 16h</li>" +
                    "<li>Service de garde inclus : 7h30 à 9h / 16h à 17h</li>" +
                "</ul>" +
            "</section>" +

            "<section class='camp-highlight'>" +
                "<h2>Tarification</h2>" +
                "<ul>" +
                    "<li>207$ / semaine</li>" +
                    "<li>Inscription à la semaine complète seulement</li>" +
                "</ul>" +
            "</section>" +

            "<section>" +
                "<h2>Le camp d'hiver EDB, c'est quoi ?</h2>" +
                "<div class='camp-cards'>" +
                    "<div class='camp-card'>" +
                        "<h3>Danse tous les jours</h3>" +
                        "<p>Encadré par des professionnels passionnés de la danse.</p>" +
                    "</div>" +
                    "<div class='camp-card'>" +
                        "<h3>Activités créatives</h3>" +
                        "<p>Bricolage, jeux et ateliers thématiques d'hiver.</p>" +
                    "</div>" +
                    "<div class='camp-card'>" +
                        "<h3>Spectacle vendredi</h3>" +
                        "<p>Présentation ou vidéo souvenir de la semaine.</p>" +
                    "</div>" +
                    "<div class='camp-card'>" +
                        "<h3>Sécurité & encadrement</h3>" +
                        "<p>Présence constante d'adultes responsables.</p>" +
                    "</div>" +
                "</div>" +
            "</section>" +

            "<section class='camp-highlight'>" +
                "<h2>Inscriptions</h2>" +
                "<ul>" +
                    "<li>Inscriptions ouvertes dès le 1er décembre</li>" +
                    "<li>Places limitées — inscrivez-vous tôt !</li>" +
                "</ul>" +
            "</section>" +

            "<section>" +
                "<h2>Questions ?</h2>" +
                "<p>Écrivez-nous à : <strong>info@expressiondansebeauport.com</strong></p>" +
                "<p><small>*Tous les danseurs doivent être propres et aller seuls aux toilettes.</small></p>" +
            "</section>",
            CampPageCss());

        if (_environment.IsDevelopment())
        {
            var campLibre = _context.Pages.FirstOrDefault(p => p.Slug == "camp-libre");
            if (campLibre != null)
            {
                campLibre.SetTitle("Camp relâche");
                campLibre.SetSlug("camp-relache");
                await _context.SaveChangesAsync();
            }
        }

        await SeedPageIfNotExists("Camp relâche", "camp-relache", 7,
            "<div class='camp-hero'>" +
                "<div>" +
                    "<p>Un camp ouvert à tous les styles, pour danser librement et s'exprimer !</p>" +
                    "<a href='https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session' target='_blank' class='btn-camp'>S'inscrire maintenant</a>" +
                "</div>" +
            "</div>" +

            "<section>" +
                "<p>Le camp libre EDB, c'est la liberté de danser à sa façon. " +
                "Pas de style imposé, juste le plaisir de bouger, créer et s'exprimer en groupe !</p>" +
            "</section>" +

            "<section class='camp-highlight'>" +
                "<h2>Dates & Horaire</h2>" +
                "<ul>" +
                    "<li><strong>Disponible lors des congés scolaires</strong></li>" +
                    "<li>9h à 16h</li>" +
                    "<li>Service de garde inclus : 7h30 à 9h / 16h à 17h</li>" +
                "</ul>" +
            "</section>" +

            "<section class='camp-highlight'>" +
                "<h2>Tarification</h2>" +
                "<ul>" +
                    "<li>207$ / semaine</li>" +
                    "<li>Ouvert à tous les niveaux — aucune expérience requise</li>" +
                "</ul>" +
            "</section>" +

            "<section>" +
                "<h2>Le camp libre EDB, c'est quoi ?</h2>" +
                "<div class='camp-cards'>" +
                    "<div class='camp-card'>" +
                        "<h3>Tous les styles bienvenus</h3>" +
                        "<p>Ballet, jazz, hip-hop, contemporain — à toi de choisir !</p>" +
                    "</div>" +
                    "<div class='camp-card'>" +
                        "<h3>Chorégraphies libres</h3>" +
                        "<p>Crée ta propre danse avec l'aide de nos animateurs.</p>" +
                    "</div>" +
                    "<div class='camp-card'>" +
                        "<h3>Spectacle vendredi</h3>" +
                        "<p>Présentation de tes créations en fin de semaine.</p>" +
                    "</div>" +
                    "<div class='camp-card'>" +
                        "<h3>Ambiance inclusive</h3>" +
                        "<p>Un environnement bienveillant pour tous les danseurs.</p>" +
                    "</div>" +
                "</div>" +
            "</section>" +

            "<section class='camp-highlight'>" +
                "<h2>Inscriptions</h2>" +
                "<ul>" +
                    "<li>Inscriptions ouvertes toute l'année</li>" +
                    "<li>Places limitées — réservez votre place dès maintenant !</li>" +
                "</ul>" +
            "</section>" +

            "<section>" +
                "<h2>Questions ?</h2>" +
                "<p>Écrivez-nous à : <strong>info@expressiondansebeauport.com</strong></p>" +
                "<p><small>*Tous les danseurs doivent être propres et aller seuls aux toilettes.</small></p>" +
            "</section>",
            CampPageCss());

        await SeedPolicyPageWithBlocks();
        await SeedActualitesPage();
    }

    private async Task SeedDemoBlocksPage()
    {
        const string slug = "demo-blocs-visuels";
        var existing = _context.Pages.FirstOrDefault(p => p.Slug == slug);
        if (existing != null)
        {
            if (!string.IsNullOrWhiteSpace(existing.Blocks))
            {
                var updatedBlocks = RewriteSeedMediaUrls(
                    existing.Blocks,
                    ["image-devant-studio.jpg", "vue-de-rue-education.jpg", "directions-sur-map.jpg"]);
                if (updatedBlocks != existing.Blocks)
                {
                    existing.SetBlocks(updatedBlocks);
                    await _context.SaveChangesAsync();
                }
            }
            return;
        }

        var page = new Page("Démo - Blocs visuels", slug);
        page.SetContentMode("blocks");
        page.SetBlocks(DemoBlocksPageBlocks());
        page.SetSortOrder(100);
        page.Publish();
        _context.Pages.Add(page);
        await _context.SaveChangesAsync();
    }

    private static string DemoBlocksPageBlocks()
    {
        var blocks = new List<string>
        {
            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"hero\",\"data\":{" +
                "\"title\":\"Bienvenue à Expression Danse de Beauport\"," +
                "\"subtitle\":\"Découvrez comment nos pages sont construites avec des blocs visuels flexibles\"," +
                "\"overlayOpacity\":0.55" +
            "}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
                "<h2>Une page, des dizaines de possibilités</h2>" +
                "<p>Chaque page du site est construite avec des <strong>blocs visuels</strong> que l'administrateur assemble librement. " +
                "Chaque bloc peut occuper toute la largeur de la page, ou être placé <strong>côte à côte</strong> avec un autre. " +
                "Sur mobile, les blocs s'empilent automatiquement.</p>" +
            "\"}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"width\":\"half\",\"data\":{\"html\":\"" +
                "<h2>Cours récréatifs</h2>" +
                "<p>Nos cours récréatifs sont ouverts à tous les groupes d'âge, des tout-petits aux adultes. " +
                "Venez découvrir le plaisir de danser dans une ambiance décontractée.</p>" +
                "<ul>" +
                "<li>Ballet</li>" +
                "<li>Jazz</li>" +
                "<li>Hip-Hop</li>" +
                "</ul>" +
            "\"}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"width\":\"half\",\"data\":{\"html\":\"" +
                "<h2>Troupes compétitives</h2>" +
                "<p>Pour les danseurs qui souhaitent aller plus loin, nos troupes compétitives participent " +
                "à des compétitions régionales et provinciales tout au long de l'année.</p>" +
                "<ul>" +
                "<li>Contemporain</li>" +
                "<li>Acro Danse</li>" +
                "<li>Danse créative (3-5 ans)</li>" +
                "</ul>" +
            "\"}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"image-gallery\",\"width\":\"half\",\"data\":{" +
                "\"images\":[" +
                    "{\"url\":\"/uploads/image-devant-studio.jpg\",\"alt\":\"Devant le studio\"}," +
                    "{\"url\":\"/uploads/vue-de-rue-education.jpg\",\"alt\":\"Vue de la rue\"}" +
                "]," +
                "\"columns\":2" +
            "}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"width\":\"half\",\"data\":{\"html\":\"" +
                "<h2>Nos installations</h2>" +
                "<p>Le studio est situé au Centre de loisirs Ste-Gertrude, au 788, avenue du Cénacle à Beauport. " +
                "Nos salles sont équipées de planchers de bois franc, de miroirs et d'une sono professionnelle.</p>" +
                "<p>L'accès au stationnement se fait par l'Avenue de l'Éducation.</p>" +
            "\"}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"faq\",\"width\":\"half\",\"data\":{\"items\":[" +
                "{\"question\":\"À quel âge peut-on commencer?\",\"answer\":\"Dès 3 ans avec nos cours de danse créative pour les tout-petits.\"}," +
                "{\"question\":\"Les cours sont-ils pour tous les niveaux?\",\"answer\":\"Oui, nous accueillons les débutants comme les danseurs expérimentés.\"}," +
                "{\"question\":\"Faut-il acheter un costume?\",\"answer\":\"Un costume de spectacle est requis en fin d'année. Les détails sont communiqués en janvier.\"}" +
            "]}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"cta-button\",\"width\":\"half\",\"data\":{" +
                "\"label\":\"S'inscrire maintenant\"," +
                "\"url\":\"https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session\"," +
                "\"style\":\"primary\",\"alignment\":\"center\",\"openInNewTab\":true" +
            "}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"google-map\",\"width\":\"half\",\"data\":{" +
                "\"embedUrl\":\"https://www.google.com/maps?q=788+avenue+du+C%C3%A9nacle,+Qu%C3%A9bec,+QC+G1E+5J4&z=15&output=embed\"," +
                "\"address\":\"788 avenue du Cénacle, Québec, QC G1E 5J4\"," +
                "\"height\":320" +
            "}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"width\":\"half\",\"data\":{\"html\":\"" +
                "<h2>Comment s'y rendre</h2>" +
                "<p>L'accès au stationnement et au local se fait par <strong>l'Avenue de l'Éducation</strong>. " +
                "Google Maps peut parfois manquer de précision sur ce point.</p>" +
                "<p><strong>Studio EDB</strong><br/>" +
                "788, avenue du Cénacle<br/>" +
                "Centre de loisirs Ste-Gertrude<br/>" +
                "Québec, QC G1E 5J4</p>" +
                "<p><strong>Téléphone :</strong> 418-666-6158</p>" +
            "\"}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"hero\",\"width\":\"half\",\"data\":{" +
                "\"title\":\"Camp d'été\"," +
                "\"subtitle\":\"Du 29 juin au 21 août 2026\"," +
                "\"ctaLabel\":\"S'inscrire\"," +
                "\"ctaUrl\":\"https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session\"," +
                "\"overlayOpacity\":0.6" +
            "}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"hero\",\"width\":\"half\",\"data\":{" +
                "\"title\":\"Camp d'hiver\"," +
                "\"subtitle\":\"Semaine de relâche 2027\"," +
                "\"ctaLabel\":\"S'inscrire\"," +
                "\"ctaUrl\":\"https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session\"," +
                "\"overlayOpacity\":0.6" +
            "}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"image-gallery\",\"data\":{" +
                "\"images\":[" +
                    "{\"url\":\"/uploads/image-devant-studio.jpg\",\"alt\":\"Devant le studio\"}," +
                    "{\"url\":\"/uploads/vue-de-rue-education.jpg\",\"alt\":\"Vue de rue\"}," +
                    "{\"url\":\"/uploads/directions-sur-map.jpg\",\"alt\":\"Plan d'accès\"}" +
                "]," +
                "\"columns\":3" +
            "}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"faq\",\"data\":{\"items\":[" +
                "{\"question\":\"Quand ont lieu les spectacles de fin d'année?\",\"answer\":\"Le spectacle annuel a lieu généralement en mai. La date exacte est confirmée en janvier.\"}," +
                "{\"question\":\"Quels styles de danse sont offerts?\",\"answer\":\"Ballet, Jazz, Hip-Hop, Contemporain, Acro Danse, Danse créative (3-5 ans) et cours Parents-Enfants.\"}," +
                "{\"question\":\"Comment s'inscrire aux cours?\",\"answer\":\"Les inscriptions se font en ligne via notre partenaire Qidigo. Vous pouvez aussi nous contacter au 418-666-6158.\"}," +
                "{\"question\":\"Y a-t-il des aides financières?\",\"answer\":\"Oui, des aides sont disponibles pour les familles dans le besoin. Contactez-nous pour en savoir plus.\"}" +
            "]}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"google-map\",\"data\":{" +
                "\"embedUrl\":\"https://www.google.com/maps?q=788+avenue+du+C%C3%A9nacle,+Qu%C3%A9bec,+QC+G1E+5J4&z=15&output=embed\"," +
                "\"address\":\"788 avenue du Cénacle, Québec, QC G1E 5J4\"," +
                "\"height\":450" +
            "}}",

            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"cta-button\",\"data\":{" +
                "\"label\":\"Voir tous nos cours et s'inscrire\"," +
                "\"url\":\"https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session\"," +
                "\"style\":\"primary\",\"alignment\":\"center\",\"openInNewTab\":true" +
            "}}"
        };

        return "[" + string.Join(",", blocks) + "]";
    }

    private async Task SeedPolicyPageWithBlocks()
    {
        var slug = Page.GenerateSlug("politique-confidentialite");
        var existing = _context.Pages.FirstOrDefault(p => p.Slug == slug);
        var blocks = PolicyPageBlocks();

        if (existing == null)
        {
            var page = new Page("Politique de confidentialité", "politique-confidentialite");
            page.SetContentMode("blocks");
            page.SetBlocks(blocks);
            page.SetSortOrder(99);
            page.Publish();
            _context.Pages.Add(page);
            await _context.SaveChangesAsync();
        }
        else if (existing.ContentMode != "blocks")
        {
            existing.SetContentMode("blocks");
            existing.SetBlocks(blocks);
            existing.SetCustomCss(null);
            await _context.SaveChangesAsync();
        }
    }

    private static string PolicyPageBlocks() =>
        "[" +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<p>Conform\\u00e9ment \\u00e0 la Loi modernisant des dispositions l\\u00e9gislatives en mati\\u00e8re de protection des renseignements personnels (Loi 25), " +
            "Expression Danse de Beauport s&#39;engage \\u00e0 prot\\u00e9ger les renseignements personnels de ses membres, \\u00e9l\\u00e8ves et visiteurs.</p>" +
        "\"}}," +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<h2>1. Responsable de la protection des renseignements personnels</h2>" +
            "<p>La directrice d&#39;Expression Danse de Beauport est responsable de la protection des renseignements personnels au sein de l&#39;organisation.</p>" +
            "<p><strong>Courriel :</strong> <a href=\\\"mailto:info@expressiondansebeauport.com\\\">info@expressiondansebeauport.com</a></p>" +
            "<p><strong>T\\u00e9l\\u00e9phone :</strong> <a href=\\\"tel:4186666158\\\">418-666-6158</a></p>" +
        "\"}}," +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<h2>2. Renseignements collect\\u00e9s</h2>" +
            "<p>Nous collectons uniquement les renseignements n\\u00e9cessaires \\u00e0 la prestation de nos services :</p>" +
            "<ul><li>Nom et pr\\u00e9nom</li><li>Adresse courriel</li><li>Num\\u00e9ro de t\\u00e9l\\u00e9phone</li>" +
            "<li>Date de naissance (pour les inscriptions de mineurs)</li><li>Informations de paiement (trait\\u00e9es par notre prestataire s\\u00e9curis\\u00e9)</li></ul>" +
        "\"}}," +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<h2>3. Finalit\\u00e9s de la collecte</h2>" +
            "<p>Vos renseignements personnels sont utilis\\u00e9s aux fins suivantes :</p>" +
            "<ul><li>Gestion des inscriptions aux cours et aux camps</li><li>Communication concernant les activit\\u00e9s de l&#39;\\u00e9cole</li>" +
            "<li>Facturation et traitement des paiements</li><li>Respect de nos obligations l\\u00e9gales</li></ul>" +
        "\"}}," +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<h2>4. Partage des renseignements</h2>" +
            "<p>Nous ne vendons, n&#39;\\u00e9changeons ni ne transmettons vos renseignements personnels \\u00e0 des tiers, sauf dans les cas suivants :</p>" +
            "<ul><li>Prestataires de services tiers n\\u00e9cessaires \\u00e0 nos activit\\u00e9s (ex. : traitement des paiements)</li>" +
            "<li>Obligations l\\u00e9gales impos\\u00e9es par la loi</li></ul>" +
            "<p>Tout prestataire tiers ayant acc\\u00e8s \\u00e0 vos donn\\u00e9es est tenu de respecter la confidentialit\\u00e9 de celles-ci.</p>" +
        "\"}}," +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<h2>5. Conservation des renseignements</h2>" +
            "<p>Vos renseignements personnels sont conserv\\u00e9s uniquement pour la dur\\u00e9e n\\u00e9cessaire aux finalit\\u00e9s pour lesquelles ils ont \\u00e9t\\u00e9 collect\\u00e9s, " +
            "ou pour satisfaire aux exigences l\\u00e9gales applicables. Une fois cette p\\u00e9riode \\u00e9coul\\u00e9e, ils sont d\\u00e9truits de fa\\u00e7on s\\u00e9curitaire.</p>" +
        "\"}}," +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<h2>6. Vos droits</h2>" +
            "<p>Conform\\u00e9ment \\u00e0 la Loi 25, vous disposez des droits suivants :</p>" +
            "<ul><li><strong>Droit d&#39;acc\\u00e8s</strong> : Vous pouvez demander \\u00e0 consulter les renseignements personnels que nous d\\u00e9tenons \\u00e0 votre sujet.</li>" +
            "<li><strong>Droit de rectification</strong> : Vous pouvez demander la correction de renseignements inexacts ou incomplets.</li>" +
            "<li><strong>Droit \\u00e0 la suppression</strong> : Vous pouvez demander la suppression de vos renseignements, sous r\\u00e9serve des obligations l\\u00e9gales.</li>" +
            "<li><strong>Droit \\u00e0 la portabilit\\u00e9</strong> : Vous pouvez demander que vos donn\\u00e9es vous soient transmises dans un format structur\\u00e9.</li></ul>" +
            "<p>Pour exercer ces droits, contactez-nous \\u00e0 : <a href=\\\"mailto:info@expressiondansebeauport.com\\\">info@expressiondansebeauport.com</a></p>" +
        "\"}}," +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<h2>7. S\\u00e9curit\\u00e9</h2>" +
            "<p>Nous mettons en \\u0153uvre des mesures de s\\u00e9curit\\u00e9 techniques et organisationnelles appropri\\u00e9es pour prot\\u00e9ger vos renseignements personnels " +
            "contre tout acc\\u00e8s non autoris\\u00e9, divulgation, alt\\u00e9ration ou destruction.</p>" +
        "\"}}," +
        "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
            "<h2>8. Mise \\u00e0 jour de la politique</h2>" +
            "<p>Cette politique de confidentialit\\u00e9 peut \\u00eatre mise \\u00e0 jour \\u00e0 tout moment. La version la plus r\\u00e9cente sera toujours disponible sur cette page. " +
            "Derni\\u00e8re mise \\u00e0 jour : janvier 2026.</p>" +
        "\"}}" +
        "]";

    private static string CampPageCss() =>
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
        "section { padding-bottom: 0.5rem; }";

    private static string EcolePageCss() =>
        ".public-page__container { max-width: 1100px; } " +
        ".public-page__title { text-align: center; font-size: 2.8rem; margin-bottom: 2.5rem; } " +
        ".public-page__content { display: flex; flex-direction: column; gap: 3rem; } " +
        ".public-page__content h2 { font-size: 1.6rem; margin-bottom: 1.2rem; color: #be1e2c; } " +
        ".ecole-hero { background: #f4f6f8; border-radius: 16px; padding: 3rem 2rem; text-align: center; } " +
        ".ecole-hero p { font-size: 1.15rem; color: #444; max-width: 720px; margin: 0 auto; line-height: 1.7; } " +
        ".ecole-cards { display: flex; gap: 1.5rem; flex-wrap: wrap; justify-content: center; margin-top: 1.5rem; } " +
        ".ecole-card { background: #f4f6f8; padding: 2rem 1.5rem; flex: 1 1 260px; max-width: 320px; border-radius: 12px; text-align: center; transition: transform 0.2s ease; } " +
        ".ecole-card:hover { transform: translateY(-4px); } " +
        ".ecole-card__icon { font-size: 2rem; margin-bottom: 0.75rem; } " +
        ".ecole-card h3 { color: #be1e2c; font-size: 1.05rem; margin-bottom: 0.6rem; text-transform: uppercase; letter-spacing: 0.05em; } " +
        ".ecole-card p { color: #555; font-size: 0.95rem; line-height: 1.6; } " +
        ".ecole-intro { color: #555; font-size: 1.05rem; line-height: 1.7; max-width: 800px; margin-bottom: 2rem; } " +
        ".ecole-team { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.5rem; margin-top: 1rem; } " +
        ".ecole-member { background: #f4f6f8; border-radius: 12px; padding: 1.75rem; transition: transform 0.2s ease; } " +
        ".ecole-member:hover { transform: translateY(-4px); } " +
        ".ecole-member__avatar { width: 56px; height: 56px; border-radius: 50%; background: #be1e2c; color: white; font-size: 1rem; font-weight: 700; display: flex; align-items: center; justify-content: center; margin-bottom: 1rem; } " +
        ".ecole-member h3 { font-size: 1.05rem; font-weight: 700; color: #1a1a1a; margin-bottom: 0.25rem; } " +
        ".ecole-member__role { font-size: 0.85rem; color: #be1e2c; font-weight: 600; margin-bottom: 0.75rem; } " +
        ".ecole-member p { font-size: 0.9rem; color: #555; line-height: 1.6; } " +
        ".ecole-ca { background: #f4f6f8; padding: 2rem; border-radius: 12px; } " +
        ".ecole-ca p { color: #555; font-size: 1rem; line-height: 1.7; margin-bottom: 1.5rem; } " +
        ".ecole-ca ul { list-style: none; padding: 0; display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 0.5rem; } " +
        ".ecole-ca li { font-size: 0.95rem; color: #333; padding: 0.4rem 0; border-bottom: 1px solid #e5e7eb; } " +
        "strong { color: #be1e2c; } " +
        "section { padding-bottom: 0.5rem; }";

    private static string EcolePageContent() =>
        "<div class='ecole-hero'>" +
            "<p>Établie à Beauport depuis plus de 30 ans, Expression Danse de Beauport offre un enseignement de qualité " +
            "dans une ambiance chaleureuse et stimulante pour les danseurs de tous les âges et de tous les niveaux.</p>" +
        "</div>" +

        "<section>" +
            "<h2>Notre mission</h2>" +
            "<div class='ecole-cards'>" +
                "<div class='ecole-card'>" +
                    "<div class='ecole-card__icon'>🎯</div>" +
                    "<h3>Accessibilité</h3>" +
                    "<p>Favoriser l'accessibilité de la danse et un apprentissage de qualité axé sur le plaisir et le respect. " +
                    "Enfant, adolescent ou adulte, nous avons un cours pour vous !</p>" +
                "</div>" +
                "<div class='ecole-card'>" +
                    "<div class='ecole-card__icon'>🌟</div>" +
                    "<h3>Diversité</h3>" +
                    "<p>Pour permettre à chacun de trouver le plaisir de danser, EDB s'engage à offrir une grande variété " +
                    "de cours dans une ambiance conviviale par des professeurs passionnés et expérimentés.</p>" +
                "</div>" +
                "<div class='ecole-card'>" +
                    "<div class='ecole-card__icon'>💪</div>" +
                    "<h3>Engagement</h3>" +
                    "<p>Accompagner les danseurs dans le développement de leurs talents et laisser une empreinte positive " +
                    "dans le cœur de tous ceux qui viennent vivre leur passion à EDB.</p>" +
                "</div>" +
            "</div>" +
        "</section>" +

        "<section>" +
            "<h2>Notre équipe</h2>" +
            "<p class='ecole-intro'>Une équipe de professeurs compétents, passionnés et dévoués qui souhaitent offrir des cours " +
            "de qualité afin d'amener chaque élève à atteindre ses propres objectifs — que ce soit le dépassement de soi, " +
            "la performance, le divertissement ou le bien-être.</p>" +
            "<div class='ecole-team'>" +
                "<div class='ecole-member'>" +
                    "<div class='ecole-member__avatar'>AS</div>" +
                    "<h3>Alexandra St-Pierre</h3>" +
                    "<p class='ecole-member__role'>Directrice &amp; Professeure — Troupes compétitives</p>" +
                    "<p>Directrice, enseignante et interprète passionnée. Formée au programme professionnel de l'École de danse " +
                    "de Québec, elle partage ses connaissances à EDB depuis 2019. Spécialiste en Jazz, Ballet, Lyrique et Contemporain.</p>" +
                "</div>" +
                "<div class='ecole-member'>" +
                    "<div class='ecole-member__avatar'>AD</div>" +
                    "<h3>Audrey Dupont</h3>" +
                    "<p class='ecole-member__role'>Professeure — Contemporain &amp; Acro Danse</p>" +
                    "<p>Finissante au programme DEC danse interprétation de l'École de danse de Québec. Elle danse depuis l'âge " +
                    "de 3 ans et enseigne depuis 7 ans le contemporain, le jazz et le ballet.</p>" +
                "</div>" +
                "<div class='ecole-member'>" +
                    "<div class='ecole-member__avatar'>ST</div>" +
                    "<h3>Sandrine Touzin</h3>" +
                    "<p class='ecole-member__role'>Professeure — Hip-Hop &amp; Troupes compétitives</p>" +
                    "<p>Passionnée, dynamique et engagée. Native de Saint-Casimir, elle a commencé la danse à 6 ans et est " +
                    "tombée en amour avec le Hip-Hop. Elle enseigne à EDB depuis 2022.</p>" +
                "</div>" +
                "<div class='ecole-member'>" +
                    "<div class='ecole-member__avatar'>LS</div>" +
                    "<h3>Lauralie Simard</h3>" +
                    "<p class='ecole-member__role'>Professeure — Ballet</p>" +
                    "<p>Danseuse depuis 2013, elle a développé une passion pour le ballet. En troisième année d'enseignement, " +
                    "son côté artistique saura vous étonner par ses choix musicaux et chorégraphiques.</p>" +
                "</div>" +
                "<div class='ecole-member'>" +
                    "<div class='ecole-member__avatar'>LB</div>" +
                    "<h3>Léa-Rose Blais</h3>" +
                    "<p class='ecole-member__role'>Professeure — Danse enfantine &amp; Parents-Enfants</p>" +
                    "<p>Formée dans plusieurs styles : jazz, claquette, ballet, celtique et gigue. Calme mais impliquée, " +
                    "douce mais investie — une vraie force tranquille.</p>" +
                "</div>" +
            "</div>" +
        "</section>" +

        "<section class='ecole-ca'>" +
            "<h2>Conseil d'administration</h2>" +
            "<p>Organisme à but non lucratif, Expression Danse de Beauport est doté d'un conseil d'administration composé " +
            "de parents bénévoles élus chaque année lors de l'assemblée générale en septembre.</p>" +
            "<ul>" +
                "<li><strong>Janie Ruest</strong> — Présidente</li>" +
                "<li><strong>Vicky Babin</strong> — Vice-Présidente</li>" +
                "<li><strong>Karine Chantal</strong> — Trésorière</li>" +
                "<li><strong>Claudia Thibault</strong> — Secrétaire</li>" +
                "<li><strong>Jonathan Laflamme</strong> — Administrateur</li>" +
                "<li><strong>Audrey Bégin</strong> — Administratrice</li>" +
                "<li><strong>Amélie Therrien</strong> — Administratrice</li>" +
            "</ul>" +
        "</section>";

    private async Task SeedNousJoindreBlocks()
    {
        var page = _context.Pages.FirstOrDefault(p => p.Slug == "nous-joindre");
        if (page == null) return;

        var seedImages = new[]
        {
            ("image-devant-studio.jpg", "image-devant-studio.jpg", 107520L, "Vue du centre des loisirs où se situe le studio de danse"),
            ("vue-de-rue-education.jpg", "vue-de-rue-education.jpg", 141312L, "Repère visuel montrant l'église, l'école et l'entrée par l'Avenue de l'Éducation"),
            ("directions-sur-map.jpg", "directions-sur-map.jpg", 59392L, "Vue aérienne annotée montrant le chemin à suivre vers le studio")
        };

        foreach (var (fileName, originalName, size, alt) in seedImages)
        {
            var correctUrl = SeedMediaUrl(fileName);
            var existing = _context.Set<MediaFile>().FirstOrDefault(m => m.FileName == fileName);
            if (existing != null)
            {
                if (existing.BlobUrl != correctUrl)
                    existing.SetBlobUrl(correctUrl);
                continue;
            }

            // Chercher aussi les anciens MediaFile avec le préfixe seed-
            var legacy = _context.Set<MediaFile>().FirstOrDefault(m =>
                m.FileName == $"seed-{fileName}" || m.BlobUrl.Contains($"seed-{fileName}"));
            if (legacy != null)
            {
                legacy.SetBlobUrl(correctUrl);
                continue;
            }

            var media = new MediaFile(fileName, originalName, "image/jpeg", size, correctUrl);
            media.SetAltText(alt);
            _context.Set<MediaFile>().Add(media);
            await _context.SaveChangesAsync();
        }

        var blocks = new object[]
        {
            new
            {
                id = Guid.NewGuid(),
                type = "rich-text",
                data = new
                {
                    html =
                        "<h2>Nous joindre</h2>" +
                        "<p>Pour toutes questions ou informations supplémentaires, n'hésitez surtout pas à nous joindre.</p>" +
                        "<p><strong>Téléphone :</strong> <a href=\"tel:4186666158\">418-666-6158</a></p>" +
                        "<p><strong>Courriel :</strong> <a href=\"mailto:info@expressiondansebeauport.com\">info@expressiondansebeauport.com</a></p>" +
                        "<h3>Nos locaux</h3>" +
                        "<p>Centre de loisirs Ste-Gertrude<br/>788, avenue du Cénacle</p>" +
                        "<h3>Adresse postale</h3>" +
                        "<p>CP 29009 QUÉ CP RAYMOND PO<br/>G1B 3G0, Québec, QC</p>"
                }
            },
            new
            {
                id = Guid.NewGuid(),
                type = "google-map",
                data = new
                {
                    embedUrl = "https://www.google.com/maps?q=788+avenue+du+C%C3%A9nacle,+Qu%C3%A9bec,+QC+G1E+5J4&z=15&output=embed",
                    address = "788 avenue du Cénacle, Québec, QC G1E 5J4",
                    height = 400
                }
            },
            new
            {
                id = Guid.NewGuid(),
                type = "rich-text",
                data = new
                {
                    html =
                        "<h2>Comment s'y rendre?</h2>" +
                        "<p>L'accès au stationnement et au local se fait par l'Avenue de l'Éducation. " +
                        "Google Maps peut parfois manquer de précision sur ce point, donc voici les repères visuels à suivre.</p>"
                }
            },
            new
            {
                id = Guid.NewGuid(),
                type = "image-gallery",
                data = new
                {
                    images = seedImages.Select(img => new { url = SeedMediaUrl(img.Item1), alt = img.Item4 }).ToArray(),
                    columns = 3
                }
            },
            new
            {
                id = Guid.NewGuid(),
                type = "contact-form",
                data = new
                {
                    title = "Contactez-nous",
                    introText = "Vous avez une question? Envoyez-nous un message et nous vous répondrons dès que possible.",
                    submitLabel = "Envoyer",
                    successMessage = "Votre message a été envoyé.",
                    recipientEmail = "",
                    enabled = true
                }
            },
            new
            {
                id = Guid.NewGuid(),
                type = "cta-button",
                data = new
                {
                    label = "S'inscrire maintenant",
                    url = "https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session",
                    style = "primary",
                    alignment = "center",
                    openInNewTab = true
                }
            }
        };

        var blocksJson = JsonSerializer.Serialize(blocks);

        // Si la page est déjà en mode blocks, migrer les URLs de médias seedés vers /uploads.
        if (page.ContentMode == "blocks")
        {
            if (!string.IsNullOrWhiteSpace(page.Blocks))
            {
                var updatedBlocks = RewriteSeedMediaUrls(page.Blocks!, seedImages.Select(img => img.Item1));
                if (updatedBlocks != page.Blocks)
                    page.SetBlocks(updatedBlocks);
            }
            await _context.SaveChangesAsync();
            return;
        }

        page.SetContentMode("blocks");
        page.SetBlocks(blocksJson);
        await _context.SaveChangesAsync();
    }

    private async Task SeedPageIfNotExists(string title, string slug, int sortOrder, string content, string? customCss = null)
    {
        var generatedSlug = Page.GenerateSlug(slug);
        if (!_context.Pages.IgnoreQueryFilters().Any(p => p.Slug == generatedSlug))
        {
            var page = CreatePage(title, slug, sortOrder, content, customCss);
            _context.Pages.Add(page);
            await _context.SaveChangesAsync();
        }
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

    private async Task SeedHelpArticles()
    {
        if (_context.HelpArticles.Any())
            return;

        var articles = new List<HelpArticle>
        {
            CreateHelpArticle(
                "Premiers pas après WordPress",
                "premiers-pas",
                HelpCategory.PremiersPas,
                sortOrder: 1,
                routeHint: null,
                content:
                    "<h2>Bienvenue dans votre nouveau panneau d'administration</h2>" +
                    "<p>L'école Expression Danse de Beauport remplace son ancien site WordPress par une plateforme sur mesure, " +
                    "pensée pour la réalité d'un organisme à but non lucratif. L'objectif : vous faire gagner du temps, réduire " +
                    "le nombre d'outils à connaître et garder votre contenu en sécurité sans dépendre d'extensions externes.</p>" +
                    "<p>Si vous avez l'habitude de WordPress, vous retrouverez la même logique : un menu d'administration sur le côté, " +
                    "des pages à éditer, une médiathèque pour vos images et des menus à organiser. Quelques détails changent toutefois, " +
                    "et cette courte introduction vous aide à prendre vos repères.</p>" +
                    "<h3>Ce qui change par rapport à votre ancien site</h3>" +
                    "<ul>" +
                        "<li><strong>Le tableau de bord se trouve à gauche.</strong> Toutes les sections (Pages, Menus, Médias, Membres, " +
                        "Sauvegardes, etc.) sont regroupées dans le menu latéral. Plus besoin de chercher dans plusieurs onglets.</li>" +
                        "<li><strong>Aucune extension à installer.</strong> Les outils dont vous avez besoin sont déjà inclus : éditeur " +
                        "de pages, gestion des membres, sauvegardes, formulaires de contact.</li>" +
                        "<li><strong>Sauvegardes.</strong> Vous pouvez déclencher une sauvegarde manuelle à tout moment. " +
                        "Selon l'environnement, des sauvegardes automatiques peuvent aussi être activées par le développeur.</li>" +
                        "<li><strong>Mises à jour invisibles.</strong> Le site est entretenu en arrière-plan ; vous ne verrez plus de bannière " +
                        "vous demandant de tout mettre à jour avant chaque modification.</li>" +
                        "<li><strong>Le contenu et la présentation sont bien séparés.</strong> Vous écrivez le texte ; le style graphique reste " +
                        "cohérent avec l'identité de l'école.</li>" +
                    "</ul>" +
                    "<h3>Vos premières actions recommandées</h3>" +
                    "<ul>" +
                        "<li><strong>Vérifier les pages existantes.</strong> Ouvrez la section <em>Pages</em> dans le menu de gauche et " +
                        "parcourez chaque page (Notre école, Récréatif, Nous joindre, etc.) pour confirmer que les informations sont à jour.</li>" +
                        "<li><strong>Mettre à jour le menu principal si nécessaire.</strong> Si vous avez créé une nouvelle page, n'oubliez pas " +
                        "de l'ajouter au menu pour qu'elle soit visible des visiteurs.</li>" +
                        "<li><strong>Déposer une nouvelle annonce ou photo.</strong> Profitez-en pour téléverser une image récente de l'école " +
                        "dans la médiathèque et la placer sur la page d'accueil ou une page d'activité.</li>" +
                        "<li><strong>Tester une sauvegarde manuelle.</strong> Cela ne prend que quelques secondes et vous rassure sur le fait " +
                        "que vos contenus sont protégés.</li>" +
                    "</ul>" +
                    "<p><strong>En cas de pépin, ouvrez ce centre d'aide en tout temps avec le bouton ? dans le coin supérieur droit.</strong> " +
                    "Vous y trouverez des articles détaillés sur chaque section du panneau, avec des explications pas à pas.</p>"
            ),
            CreateHelpArticle(
                "Modifier une page existante",
                "modifier-une-page",
                HelpCategory.Pages,
                sortOrder: 1,
                routeHint: "admin.children.pages.index",
                content:
                    "<h2>Modifier le contenu d'une page</h2>" +
                    "<p>Les pages sont les sections de votre site visibles par les visiteurs : Notre école, Récréatif, Camps, Nous joindre, etc. " +
                    "Vous pouvez les modifier à tout moment sans toucher au design du site.</p>" +
                    "<h3>Étapes à suivre</h3>" +
                    "<ul>" +
                        "<li><strong>Étape 1 :</strong> Dans le menu de gauche du panneau d'administration, cliquez sur <em>Pages</em>. " +
                        "La liste de toutes les pages du site s'affiche.</li>" +
                        "<li><strong>Étape 2 :</strong> Repérez la page à modifier, puis cliquez sur son titre (ou sur le bouton Modifier). " +
                        "L'éditeur s'ouvre.</li>" +
                        "<li><strong>Étape 3 :</strong> Apportez vos changements dans le contenu. Vous pouvez ajouter du texte, changer un " +
                        "paragraphe, insérer une image depuis la médiathèque ou ajouter un lien. Deux modes d'édition sont disponibles : " +
                        "le mode <em>blocs visuels</em> (recommandé) ou le mode <em>HTML</em> pour les utilisateurs plus à l'aise.</li>" +
                        "<li><strong>Étape 4 :</strong> Choisissez l'état de publication. Sélectionnez <strong>Publié</strong> pour rendre " +
                        "la page visible aux visiteurs, ou <strong>Brouillon</strong> pour la garder cachée pendant que vous travaillez dessus.</li>" +
                        "<li><strong>Étape 5 :</strong> Cliquez sur le bouton <strong>Enregistrer</strong> en haut ou en bas de la page. " +
                        "Vos modifications sont appliquées immédiatement.</li>" +
                    "</ul>" +
                    "<h3>Brouillon ou publié, quelle différence ?</h3>" +
                    "<p>Une page en <strong>Brouillon</strong> est invisible pour les visiteurs : elle n'apparaît ni dans le menu ni dans une " +
                    "recherche. Utilisez ce mode pour préparer un contenu sans le diffuser tout de suite. Une page <strong>Publiée</strong>, " +
                    "au contraire, est accessible immédiatement à toute personne qui visite le site.</p>" +
                    "<p>Vous pouvez basculer entre les deux états à tout moment. C'est utile par exemple pour mettre une page hors ligne " +
                    "temporairement, sans la supprimer.</p>" +
                    "<h3>Attention au slug de la page</h3>" +
                    "<p>Le <strong>slug</strong> est la partie de l'adresse qui identifie la page (par exemple <em>/camp-d-ete</em>). " +
                    "Une fois qu'une page est en ligne et partagée, son slug fait partie de son adresse web. Si vous le modifiez sans précaution, " +
                    "les anciens liens (par exemple ceux partagés dans une infolettre ou sur Facebook) ne fonctionneront plus.</p>" +
                    "<p><strong>Notre conseil :</strong> ne changez le slug d'une page déjà en ligne que si c'est vraiment intentionnel. " +
                    "Pour une nouvelle page, vous pouvez choisir librement le slug avant la publication.</p>"
            ),
            CreateHelpArticle(
                "Créer une nouvelle page",
                "creer-une-page",
                HelpCategory.Pages,
                sortOrder: 2,
                routeHint: "admin.children.pages.add",
                content:
                    "<h2>Créer une page dans le CMS</h2>" +
                    "<p>Une nouvelle page sert à ajouter une section complète au site public, par exemple une activité, une annonce durable " +
                    "ou une page d'information qui doit rester accessible dans le menu.</p>" +
                    "<h3>Avant de commencer</h3>" +
                    "<ul>" +
                        "<li><strong>Choisissez un titre clair.</strong> Il sera visible dans l'administration et peut aussi servir à générer l'adresse de la page.</li>" +
                        "<li><strong>Préparez les images.</strong> Téléversez-les d'abord dans la médiathèque si elles doivent être réutilisées ailleurs.</li>" +
                        "<li><strong>Décidez si la page doit être publiée immédiatement.</strong> Utilisez le mode brouillon si le contenu doit être relu.</li>" +
                    "</ul>" +
                    "<h3>Créer puis rendre la page visible</h3>" +
                    "<p>Après l'enregistrement, vérifiez l'aperçu de la page. Si elle doit apparaître dans la navigation publique, ouvrez ensuite " +
                    "la section <em>Menus</em> et ajoutez une entrée vers cette page dans le menu principal ou le pied de page.</p>"
            ),
            CreateHelpArticle(
                "Travailler dans l'éditeur de page",
                "editeur-de-page",
                HelpCategory.Pages,
                sortOrder: 3,
                routeHint: "admin.children.pages.edit",
                content:
                    "<h2>Utiliser l'éditeur de page</h2>" +
                    "<p>L'éditeur permet de modifier le contenu sans toucher au code du site. Le mode blocs visuels est recommandé pour la majorité " +
                    "des changements : il garde une mise en page cohérente et limite les erreurs de HTML.</p>" +
                    "<h3>Bonnes pratiques</h3>" +
                    "<ul>" +
                        "<li><strong>Modifier un bloc à la fois.</strong> Relisez le résultat après chaque changement important.</li>" +
                        "<li><strong>Utiliser des titres courts.</strong> Les titres trop longs sont difficiles à lire sur mobile.</li>" +
                        "<li><strong>Éviter de coller du contenu formaté depuis Word.</strong> Collez le texte puis appliquez les styles avec l'éditeur.</li>" +
                        "<li><strong>Enregistrer avant de quitter.</strong> Les changements ne sont visibles qu'après l'enregistrement.</li>" +
                    "</ul>" +
                    "<p>Si une modification importante doit être préparée tranquillement, basculez la page en brouillon ou travaillez sur une copie " +
                    "du contenu avant de publier.</p>"
            ),
            CreateHelpArticle(
                "Ajouter un élément au menu principal",
                "gerer-le-menu",
                HelpCategory.Menus,
                sortOrder: 1,
                routeHint: "admin.children.menus",
                content:
                    "<h2>Organiser les menus du site</h2>" +
                    "<p>Les menus sont les listes de liens qui guident les visiteurs sur le site. Le <strong>menu principal</strong> est " +
                    "celui qui apparaît en haut de chaque page ; le <strong>menu du pied de page</strong> (footer) regroupe les liens utiles " +
                    "comme la politique de confidentialité ou les coordonnées de l'école.</p>" +
                    "<h3>Étapes à suivre</h3>" +
                    "<ul>" +
                        "<li><strong>Étape 1 :</strong> Dans le menu de gauche, cliquez sur <em>Menus</em>. Vous verrez la liste des menus " +
                        "disponibles (Principal et Pied de page).</li>" +
                        "<li><strong>Étape 2 :</strong> Choisissez le menu à modifier (généralement le <em>Menu principal</em>). " +
                        "Sa structure actuelle s'affiche sous forme de liste.</li>" +
                        "<li><strong>Étape 3 :</strong> Cliquez sur <strong>Ajouter une entrée</strong>. Vous pouvez choisir entre deux types " +
                        "de liens : un <em>lien interne</em> (vers une page de votre site, comme Récréatif ou Nous joindre) ou un " +
                        "<em>lien externe</em> (vers un site partenaire, par exemple le portail d'inscription Qidigo).</li>" +
                        "<li><strong>Étape 4 :</strong> Donnez un titre clair à votre entrée (c'est ce que verront les visiteurs) et associez-la " +
                        "à la page ou à l'adresse souhaitée.</li>" +
                        "<li><strong>Étape 5 :</strong> Réorganisez vos entrées par <strong>glisser-déposer</strong>. Saisissez une entrée " +
                        "avec la souris et déplacez-la vers le haut ou vers le bas pour changer son ordre d'apparition.</li>" +
                        "<li><strong>Étape 6 :</strong> Cliquez sur <strong>Enregistrer</strong> pour appliquer vos changements. Le menu " +
                        "se met à jour immédiatement sur le site public.</li>" +
                    "</ul>" +
                    "<h3>Créer un sous-menu</h3>" +
                    "<p>Vous pouvez regrouper plusieurs entrées sous un même titre parent. C'est particulièrement utile pour les pages liées entre " +
                    "elles, par exemple les différents camps (Camp d'été, Camp d'hiver, Camp relâche) sous une seule entrée <em>Camps</em>.</p>" +
                    "<p>Pour créer un sous-menu, glissez une entrée vers la droite, sous l'entrée parente. Vous verrez l'élément se décaler pour " +
                    "indiquer qu'il devient un sous-élément. Sur le site, le parent affichera un petit indicateur (souvent une flèche) et le " +
                    "sous-menu apparaîtra au survol ou au clic.</p>" +
                    "<p>N'oubliez pas d'<strong>enregistrer</strong> après chaque réorganisation pour conserver vos changements.</p>"
            ),
            CreateHelpArticle(
                "Téléverser une image ou un document",
                "mediatheque",
                HelpCategory.Medias,
                sortOrder: 1,
                routeHint: "admin.children.media",
                content:
                    "<h2>Utiliser la médiathèque</h2>" +
                    "<p>La <strong>médiathèque</strong> centralise toutes les images, documents et fichiers utilisés sur votre site. " +
                    "Une fois qu'un fichier y est déposé, vous pouvez le réutiliser autant de fois que vous voulez dans n'importe quelle " +
                    "page sans avoir à le téléverser de nouveau.</p>" +
                    "<h3>Étapes à suivre pour téléverser un fichier</h3>" +
                    "<ul>" +
                        "<li><strong>Étape 1 :</strong> Dans le menu de gauche, cliquez sur <em>Médias</em>. La galerie de fichiers s'affiche.</li>" +
                        "<li><strong>Étape 2 :</strong> Glissez votre fichier directement dans la zone d'upload (la grande zone en pointillé) " +
                        "ou cliquez sur <strong>Téléverser</strong> pour le sélectionner depuis votre ordinateur.</li>" +
                        "<li><strong>Étape 3 :</strong> Attendez la fin du transfert (une barre de progression vous indique où en est " +
                        "le téléversement). Une fois terminé, le fichier apparaît dans la galerie.</li>" +
                        "<li><strong>Étape 4 :</strong> Cliquez sur la miniature pour ajouter une description ou un texte alternatif " +
                        "(important pour l'accessibilité aux personnes utilisant un lecteur d'écran).</li>" +
                    "</ul>" +
                    "<h3>Formats acceptés</h3>" +
                    "<ul>" +
                        "<li><strong>Images :</strong> jpg, jpeg, png, webp, gif</li>" +
                        "<li><strong>Documents :</strong> pdf</li>" +
                        "<li><strong>Autres :</strong> selon la configuration du site, vous pouvez aussi téléverser des fichiers Word ou Excel</li>" +
                    "</ul>" +
                    "<h3>Pourquoi compresser vos images avant de les téléverser</h3>" +
                    "<p>Une image prise avec un téléphone récent peut peser plusieurs mégaoctets. Si vous mettez en ligne plusieurs photos " +
                    "non compressées, votre site devient plus lent à charger pour les visiteurs, surtout sur mobile. Un site lent décourage " +
                    "les visites et nuit au référencement sur Google.</p>" +
                    "<p>Avant de téléverser, pensez à <strong>réduire le poids</strong> de vos images avec un outil gratuit comme " +
                    "<em>tinypng.com</em> ou <em>squoosh.app</em>. Ces sites compressent l'image sans perte visible de qualité. Visez moins " +
                    "de 500 Ko par photo dans la mesure du possible.</p>" +
                    "<h3>Réutiliser une image dans une page</h3>" +
                    "<p>Lorsque vous éditez une page, cliquez sur l'icône <em>Image</em> dans la barre d'édition. La médiathèque s'ouvre et " +
                    "vous pouvez choisir n'importe quel fichier déjà téléversé. Pas besoin de le mettre en ligne deux fois : un même fichier " +
                    "peut servir sur plusieurs pages.</p>"
            ),
            CreateHelpArticle(
                "Personnaliser l'identité du site",
                "personnalisation-du-site",
                HelpCategory.Parametres,
                sortOrder: 1,
                routeHint: "admin.children.customizer",
                content:
                    "<h2>Personnaliser l'apparence et les informations publiques</h2>" +
                    "<p>La section Personnalisation regroupe les réglages qui influencent l'identité du site : couleurs, logo, favicon, " +
                    "coordonnées, réseaux sociaux, bandeau d'information et paramètres visibles par les visiteurs.</p>" +
                    "<h3>Ce qu'il faut vérifier avant d'enregistrer</h3>" +
                    "<ul>" +
                        "<li><strong>Logo et favicon :</strong> utilisez des fichiers propres, déjà téléversés dans la médiathèque.</li>" +
                        "<li><strong>Coordonnées :</strong> relisez le courriel, le téléphone et l'adresse, car ils peuvent apparaître à plusieurs endroits.</li>" +
                        "<li><strong>Bandeau public :</strong> gardez le message court et retirez-le quand il n'est plus pertinent.</li>" +
                        "<li><strong>Couleurs :</strong> privilégiez les couleurs officielles de l'école pour garder une image cohérente.</li>" +
                    "</ul>" +
                    "<p>Après une modification visuelle, ouvrez le site public dans un nouvel onglet et vérifiez la page d'accueil sur ordinateur " +
                    "et sur téléphone.</p>"
            ),
            CreateHelpArticle(
                "Vérifier la santé du site",
                "sante-du-site",
                HelpCategory.Parametres,
                sortOrder: 2,
                routeHint: "admin.children.siteHealth",
                content:
                    "<h2>Comprendre la santé du site</h2>" +
                    "<p>La page Santé du site donne un portrait rapide de l'état technique de l'application : connexion à la base de données, " +
                    "version .NET, mémoire disponible et indicateurs de contenu.</p>" +
                    "<h3>Quand consulter cette page</h3>" +
                    "<ul>" +
                        "<li><strong>Avant de signaler un problème technique.</strong> Elle aide à voir si le site répond normalement.</li>" +
                        "<li><strong>Après un déploiement.</strong> Vérifiez que la base de données et l'application sont bien accessibles.</li>" +
                        "<li><strong>Si l'administration semble lente.</strong> Les indicateurs de mémoire peuvent aider le développeur à diagnostiquer.</li>" +
                    "</ul>" +
                    "<p>Si un statut critique apparaît, évitez les changements importants et contactez la personne responsable du support technique " +
                    "avec une capture d'écran de la page.</p>"
            ),
            CreateHelpArticle(
                "Importer ou exporter les données CMS",
                "import-export-cms",
                HelpCategory.Parametres,
                sortOrder: 3,
                routeHint: "admin.children.importExport",
                content:
                    "<h2>Utiliser l'import et l'export</h2>" +
                    "<p>L'export télécharge une copie JSON des données CMS. L'import sert à réinjecter un fichier compatible dans le site. " +
                    "C'est un outil puissant, à utiliser seulement quand l'objectif est clair.</p>" +
                    "<h3>Exporter</h3>" +
                    "<p>Utilisez l'export avant une série de modifications importantes ou pour transmettre l'état du CMS au développeur. " +
                    "Conservez le fichier dans un endroit privé, car il peut contenir du contenu administratif.</p>" +
                    "<h3>Importer</h3>" +
                    "<ul>" +
                        "<li><strong>Vérifiez la provenance du fichier.</strong> N'importez jamais un fichier reçu d'une source inconnue.</li>" +
                        "<li><strong>Faites une sauvegarde avant.</strong> L'import peut modifier des données existantes.</li>" +
                        "<li><strong>Relisez les pages importantes après l'import.</strong> Confirmez que le menu, les pages et les médias attendus sont présents.</li>" +
                    "</ul>"
            ),
            CreateHelpArticle(
                "Lire les logs d'erreurs",
                "logs-erreurs",
                HelpCategory.Parametres,
                sortOrder: 4,
                routeHint: "admin.children.errorLogs",
                content:
                    "<h2>Comprendre les logs d'erreurs</h2>" +
                    "<p>Les logs d'erreurs listent les problèmes techniques récents détectés par l'application. Cette page est surtout utile " +
                    "pour transmettre une information précise au développeur.</p>" +
                    "<h3>Comment s'en servir</h3>" +
                    "<ul>" +
                        "<li><strong>Filtrez par date ou par niveau</strong> pour isoler ce qui s'est passé au moment du problème.</li>" +
                        "<li><strong>Copiez le message principal</strong> ou prenez une capture d'écran avant de demander de l'aide.</li>" +
                        "<li><strong>Ne supprimez pas le contexte.</strong> L'heure, la source et le niveau de l'erreur sont souvent importants.</li>" +
                    "</ul>" +
                    "<p>Un log d'erreur ne veut pas toujours dire que le site est inutilisable. Il indique surtout qu'un événement doit être vérifié.</p>"
            ),
            CreateHelpArticle(
                "Consulter le journal d'audit",
                "journal-audit",
                HelpCategory.Parametres,
                sortOrder: 5,
                routeHint: "admin.children.auditLogs",
                content:
                    "<h2>Suivre les actions administratives</h2>" +
                    "<p>Le journal d'audit garde une trace des actions importantes réalisées dans l'administration : créations, modifications, " +
                    "suppressions, connexions et déconnexions.</p>" +
                    "<h3>Quand l'utiliser</h3>" +
                    "<ul>" +
                        "<li><strong>Retrouver qui a modifié une page.</strong> Filtrez par utilisateur ou par type d'action.</li>" +
                        "<li><strong>Comprendre une suppression.</strong> Cherchez l'entité concernée et la date de l'action.</li>" +
                        "<li><strong>Préparer une demande de support.</strong> Mentionnez l'heure et l'action observées.</li>" +
                    "</ul>" +
                    "<p>Le journal d'audit sert à comprendre ce qui s'est passé. Il ne remplace pas une sauvegarde pour restaurer du contenu.</p>"
            ),
            CreateHelpArticle(
                "Vérifier la version installée",
                "version-installee",
                HelpCategory.Parametres,
                sortOrder: 6,
                routeHint: "admin.children.version",
                content:
                    "<h2>Vérifier la version du site</h2>" +
                    "<p>La page Version indique la version actuellement installée, le commit de build quand il est disponible, et la dernière " +
                    "version publiée sur GitHub.</p>" +
                    "<h3>Pourquoi c'est utile</h3>" +
                    "<ul>" +
                        "<li><strong>Après un déploiement :</strong> confirmer que la nouvelle version est bien en ligne.</li>" +
                        "<li><strong>Avant de rapporter un bug :</strong> transmettre le numéro de version aide à reproduire le problème.</li>" +
                        "<li><strong>Pour suivre les mises à jour :</strong> comparer la version installée avec la dernière version publiée.</li>" +
                    "</ul>" +
                    "<p>Si la vérification GitHub échoue, la version installée peut quand même être correcte. Réessayez plus tard ou contactez " +
                    "le développeur si l'information reste indisponible.</p>"
            ),
            CreateHelpArticle(
                "Comprendre les membres du portail social",
                "membres-portail-social",
                HelpCategory.Membres,
                sortOrder: 1,
                routeHint: null,
                content:
                    "<h2>Membres et professeurs dans EDB Social</h2>" +
                    "<p>Les membres utilisent le portail social pour consulter les annonces, rejoindre leurs groupes, écrire des messages et " +
                    "mettre leur profil à jour. Certains membres peuvent aussi avoir le rôle de professeur.</p>" +
                    "<h3>Points à surveiller</h3>" +
                    "<ul>" +
                        "<li><strong>Adresse courriel :</strong> elle sert à la connexion et aux communications importantes.</li>" +
                        "<li><strong>Rôle professeur :</strong> donne plus de responsabilités dans les groupes et les annonces.</li>" +
                        "<li><strong>Profil :</strong> les noms et photos doivent rester appropriés pour un contexte d'école de danse.</li>" +
                    "</ul>" +
                    "<p>Si une personne ne reçoit pas ses courriels, vérifiez d'abord l'adresse saisie et demandez-lui de consulter ses pourriels.</p>"
            ),
            CreateHelpArticle(
                "Comprendre les groupes de danse",
                "groupes-de-danse",
                HelpCategory.Groupes,
                sortOrder: 1,
                routeHint: null,
                content:
                    "<h2>Groupes dans EDB Social</h2>" +
                    "<p>Les groupes rassemblent les membres autour d'un cours, d'une troupe ou d'une saison. Ils servent à diffuser des annonces, " +
                    "partager des fichiers et centraliser les discussions liées à un groupe précis.</p>" +
                    "<h3>Bonnes pratiques</h3>" +
                    "<ul>" +
                        "<li><strong>Nommer le groupe clairement.</strong> Incluez le niveau, la saison ou le nom du cours si nécessaire.</li>" +
                        "<li><strong>Garder les codes d'invitation privés.</strong> Partagez-les seulement avec les personnes qui doivent rejoindre le groupe.</li>" +
                        "<li><strong>Archiver les groupes terminés.</strong> Cela évite de mélanger les anciennes saisons avec les groupes actifs.</li>" +
                    "</ul>" +
                    "<p>Avant de supprimer un groupe, assurez-vous que les informations importantes ont été sauvegardées ou transférées ailleurs.</p>"
            ),
            CreateHelpArticle(
                "Lancer une sauvegarde manuelle",
                "sauvegarde-manuelle",
                HelpCategory.Sauvegardes,
                sortOrder: 1,
                routeHint: "admin.children.backup",
                content:
                    "<h2>Sauvegardes du site</h2>" +
                    "<p>Une sauvegarde est une copie complète de votre site (toutes les pages, les images, les membres, les paramètres) " +
                    "à un instant donné. Elle vous permet de revenir en arrière en cas de fausse manœuvre, de bug ou de problème technique. " +
                    "C'est votre filet de sécurité.</p>" +
                    "<h3>Où trouver les sauvegardes</h3>" +
                    "<p>Dans le menu de gauche du panneau d'administration, cliquez sur <em>Sauvegardes</em>. Vous verrez la liste des " +
                    "sauvegardes existantes, avec leur date de création et leur taille. Plus la sauvegarde est récente, plus la copie reflète " +
                    "l'état actuel du site.</p>" +
                    "<h3>Déclencher une sauvegarde manuelle</h3>" +
                    "<ul>" +
                        "<li><strong>Étape 1 :</strong> Ouvrez la section <em>Sauvegardes</em>.</li>" +
                        "<li><strong>Étape 2 :</strong> Cliquez sur le bouton <strong>Lancer une sauvegarde maintenant</strong> " +
                        "(habituellement en haut à droite de la page).</li>" +
                        "<li><strong>Étape 3 :</strong> Patientez quelques secondes à quelques minutes selon la quantité de contenus. " +
                        "Une fois la sauvegarde terminée, elle apparaît en haut de la liste avec la date et l'heure exactes.</li>" +
                    "</ul>" +
                    "<h3>Combien de temps ça prend</h3>" +
                    "<p>Pour un site de la taille de celui d'Expression Danse, comptez généralement entre <strong>quelques secondes et deux " +
                    "ou trois minutes</strong>. Le temps dépend surtout du nombre d'images et de documents dans la médiathèque. Vous pouvez " +
                    "continuer à naviguer dans le panneau pendant que la sauvegarde se fait.</p>" +
                    "<h3>Où est stocké le fichier de sauvegarde</h3>" +
                    "<p>Les fichiers de sauvegarde sont conservés <strong>sur le serveur</strong> qui héberge le site. Ils sont protégés et " +
                    "ne sont accessibles qu'aux administrateurs. <strong>Attention à l'espace disponible :</strong> chaque sauvegarde prend " +
                    "de la place. Si vous accumulez beaucoup de sauvegardes manuelles, pensez à supprimer les plus anciennes pour ne pas " +
                    "remplir l'espace de stockage.</p>" +
                    "<p>Pour une sécurité supplémentaire, vous pouvez aussi télécharger un fichier de sauvegarde sur votre ordinateur " +
                    "personnel ou sur un disque externe.</p>" +
                    "<h3>Et les sauvegardes automatiques ?</h3>" +
                    "<p>Les sauvegardes automatiques peuvent être activées par le développeur selon l'environnement. Quand elles sont " +
                    "activées, elles se lancent à intervalle régulier sans intervention de votre part. La sauvegarde manuelle reste utile " +
                    "<em>avant</em> une modification importante (refonte d'une page, suppression de membres, changement de paramètres) " +
                    "pour avoir un point de retour précis si jamais quelque chose se passe mal.</p>"
            )
        };

        _context.HelpArticles.AddRange(articles);
        await _context.SaveChangesAsync();
    }

    private static HelpArticle CreateHelpArticle(string title, string slug, HelpCategory category, int sortOrder, string? routeHint, string content)
    {
        var article = new HelpArticle(title, slug, category);
        article.SetContentMode("html");
        article.SetContent(content);
        article.SetSortOrder(sortOrder);
        article.SetRouteHint(routeHint);
        article.Publish();
        return article;
    }

    private static readonly string[] CampSlugs = ["camp-d-ete", "camp-d-hiver", "camp-relache"];

    private static string SeedMediaUrl(string fileName) => $"/uploads/{fileName}";

    private static string SeedFooterPartnerUrl(string fileName) => $"/uploads/seed-partners/{fileName}";

    private static string RewriteSeedMediaUrls(string content, IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            var correctUrl = SeedMediaUrl(fileName);
            content = content
                .Replace($"/uploads/uploads/{fileName}", correctUrl)
                .Replace($"/uploads/seed-{fileName}", correctUrl)
                .Replace($"/images/seed/{fileName}", correctUrl)
                .Replace($"/images/{fileName}", correctUrl);
            content = Regex.Replace(content, $@"(?<![A-Za-z0-9_./-])/{Regex.Escape(fileName)}", correctUrl);
        }

        return content;
    }

    private async Task SeedMenus()
    {
        await SeedPrimaryMenu();
        await SeedFooterMenu();
    }

    private async Task SeedPrimaryMenu()
    {
        if (_context.NavigationMenus.Any(m => m.Location == MenuLocation.Primary))
            return;

        var primaryMenu = new NavigationMenu("Menu principal", MenuLocation.Primary);
        _context.NavigationMenus.Add(primaryMenu);
        await _context.SaveChangesAsync();

        var pages = await _context.Pages.OrderBy(p => p.SortOrder).ToListAsync();
        var sortOrder = 0;

        var mesCampsItem = new NavigationMenuItem(primaryMenu.Id, "Mes camps", 3);
        _context.NavigationMenuItems.Add(mesCampsItem);
        await _context.SaveChangesAsync();

        foreach (var page in pages)
        {
            if (page.Slug == "politique-confidentialite")
                continue;

            if (CampSlugs.Contains(page.Slug))
            {
                var child = new NavigationMenuItem(primaryMenu.Id, page.Title, sortOrder++);
                child.SetPageId(page.Id);
                child.SetUrl($"/{page.Slug}");
                child.SetParentId(mesCampsItem.Id);
                _context.NavigationMenuItems.Add(child);
            }
            else
            {
                var item = new NavigationMenuItem(primaryMenu.Id, page.Title, sortOrder++);
                item.SetPageId(page.Id);
                item.SetUrl($"/{page.Slug}");
                _context.NavigationMenuItems.Add(item);
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedFooterMenu()
    {
        if (_context.NavigationMenus.Any(m => m.Location == MenuLocation.Footer))
            return;

        var footerMenu = new NavigationMenu("Menu du pied de page", MenuLocation.Footer);
        _context.NavigationMenus.Add(footerMenu);
        await _context.SaveChangesAsync();

        var pagesBySlug = await _context.Pages.ToDictionaryAsync(p => p.Slug);

        var footerLinks = new[]
        {
            (Label: "Accueil", Slug: (string?)null, Url: "/"),
            (Label: "Notre école", Slug: (string?)"notre-ecole", Url: "/notre-ecole"),
            (Label: "Récréatif", Slug: (string?)"recreatif", Url: "/recreatif"),
            (Label: "Camp d'été", Slug: (string?)"camp-d-ete", Url: "/camp-d-ete"),
            (Label: "Compétitif", Slug: (string?)"troupes-competitives", Url: "/troupes-competitives"),
            (Label: "Nous joindre", Slug: (string?)"nous-joindre", Url: "/nous-joindre"),
            (Label: "Politique de confidentialité", Slug: (string?)"politique-confidentialite", Url: "/politique-confidentialite"),
        };

        var sortOrder = 0;
        foreach (var link in footerLinks)
        {
            var item = new NavigationMenuItem(footerMenu.Id, link.Label, sortOrder++);
            item.SetUrl(link.Url);
            if (link.Slug is not null && pagesBySlug.TryGetValue(link.Slug, out var page))
                item.SetPageId(page.Id);
            _context.NavigationMenuItems.Add(item);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedActualitesPage()
    {
        const string slug = "actualites";
        var generatedSlug = Page.GenerateSlug(slug);
        if (_context.Pages.Any(p => p.Slug == generatedSlug))
            return;

        var blocks = new List<string>
        {
            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{" +
                "\"html\":\"<h2>Actualités du moment</h2>" +
                "<p>Bienvenue sur la page des actualités d'Expression Danse de Beauport. " +
                "Retrouvez ici toutes les nouvelles importantes concernant nos activités, " +
                "nos inscriptions et nos événements spéciaux.</p>\"" +
            "}}",
            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{" +
                "\"html\":\"<h3>🎉 Inscriptions ouvertes</h3>" +
                "<p>Les inscriptions pour la prochaine saison sont maintenant ouvertes ! " +
                "Ne tardez pas à vous inscrire, les places sont limitées.</p>" +
                "<p><strong>Date limite :</strong> à confirmer par l'équipe administrative.</p>\"" +
            "}}",
            "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"cta-button\",\"data\":{" +
                "\"label\":\"S'inscrire maintenant\"," +
                "\"url\":\"https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session\"," +
                "\"style\":\"primary\"," +
                "\"alignment\":\"center\"," +
                "\"openInNewTab\":true" +
            "}}"
        };

        var page = new Page("Actualités", slug);
        page.SetContentMode("blocks");
        page.SetBlocks("[" + string.Join(",", blocks) + "]");
        page.SetSortOrder(98);
        page.Publish();
        _context.Pages.Add(page);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSiteSettings()
    {
        var settings = await _context.SiteSettings
            .Include(s => s.SocialLinks)
            .FirstOrDefaultAsync();

        var created = false;
        if (settings is null)
        {
            settings = new SiteSettings();
            _context.SiteSettings.Add(settings);
            created = true;
        }

        if (string.IsNullOrWhiteSpace(settings.FooterAddress))
            settings.SetFooterAddress("CP 29009 QUÉ CP RAYMOND PO");
        if (string.IsNullOrWhiteSpace(settings.FooterCity))
            settings.SetFooterCity("Québec, Qc G1B 3G0");
        if (string.IsNullOrWhiteSpace(settings.FooterPhone))
            settings.SetFooterPhone("418-666-6158");
        if (string.IsNullOrWhiteSpace(settings.FooterEmail))
            settings.SetFooterEmail("info@expressiondansebeauport.com");
        if (string.IsNullOrWhiteSpace(settings.FacebookUrl))
            settings.SetFacebookUrl("https://www.facebook.com/expressiondansebeauport");
        if (string.IsNullOrWhiteSpace(settings.InstagramUrl))
            settings.SetInstagramUrl("https://www.instagram.com/expressiondansebeauport/");
        if (string.IsNullOrWhiteSpace(settings.CopyrightText))
            settings.SetCopyrightText("EDB");

        if (!settings.IsBannerEnabled)
        {
            settings.SetBannerEnabled(true);
            settings.SetBannerText("📣 Inscriptions ouvertes ! Consultez nos actualités du moment");
            settings.SetBannerUrl("/actualites");
        }

        if (string.IsNullOrWhiteSpace(settings.ReviewsSectionEyebrow))
            settings.SetReviewsSectionEyebrow("Avis de notre communauté");
        if (string.IsNullOrWhiteSpace(settings.ReviewsSectionTitle))
            settings.SetReviewsSectionTitle("Ce que les familles disent de nous");
        if (string.IsNullOrWhiteSpace(settings.ReviewsSectionSubtitle))
            settings.SetReviewsSectionSubtitle("Quelques témoignages gérés par l’équipe du site pour refléter l’expérience vécue à l’école.");

        if (created)
            await _context.SaveChangesAsync();

        if (!settings.SocialLinks.Any(sl => sl.Platform == "facebook"))
            _context.SocialLinks.Add(new SocialLink(settings.Id, "facebook", "https://www.facebook.com/expressiondansebeauport", 0));
        if (!settings.SocialLinks.Any(sl => sl.Platform == "instagram"))
            _context.SocialLinks.Add(new SocialLink(settings.Id, "instagram", "https://www.instagram.com/expressiondansebeauport/", 1));

        await _context.SaveChangesAsync();

        await SeedReviews(settings);
        await SeedFooterPartners(settings);
    }

    private async Task SeedReviews(SiteSettings settings)
    {
        var existingReviews = await _context.Reviews
            .Where(r => r.SiteSettingsId == settings.Id)
            .OrderBy(r => r.SortOrder)
            .ToListAsync();

        if (existingReviews.Count > 0)
            return;

        var seededReviews = new[]
        {
            new Review(settings.Id, "Une ambiance qui met en confiance", "Ma fille a trouvé sa place dès le premier cours. L’équipe est chaleureuse, structurée et très attentive aux enfants.", "Sophie, parent", 5, 0),
            new Review(settings.Id, "Des cours dynamiques et bien encadrés", "On sent une vraie passion dans l’enseignement. Les explications sont claires et les jeunes progressent rapidement.", "Karine, parent", 5, 1),
            new Review(settings.Id, "Un milieu positif pour évoluer", "Les professeurs encouragent vraiment chaque danseur. C’est motivant, bienveillant et toujours professionnel.", "Mélanie, élève adulte", 5, 2),
            new Review(settings.Id, "Organisation rassurante", "L’accueil, les communications et le suivi sont constants. Comme parent, c’est exactement le genre d’encadrement qu’on recherche.", "David, parent", 4, 3),
            new Review(settings.Id, "Des spectacles mémorables", "Chaque fin de session est préparée avec soin. On voit le travail derrière chaque chorégraphie et les enfants sont très fiers.", "Julie, parent", 5, 4),
        };

        _context.Reviews.AddRange(seededReviews);
        await _context.SaveChangesAsync();
    }

    private static readonly (string FileName, string ContentType, string AltText)[] SeedFooterPartnerAssets =
    [
        ("vdq-beauport.png", "image/png", "Ville de Québec - Arrondissement de Beauport"),
        ("cafe-de-julie.png", "image/png", "Café de Julie"),
        ("culture-beauport.png", "image/png", "Culture Beauport"),
    ];

    private async Task SeedFooterPartners(SiteSettings settings)
    {
        var existingPartners = await _context.FooterPartners
            .Where(fp => fp.SiteSettingsId == settings.Id)
            .Include(fp => fp.MediaFile)
            .ToListAsync();

        var sortOrder = 0;
        foreach (var asset in SeedFooterPartnerAssets)
        {
            var blobUrl = SeedFooterPartnerUrl(asset.FileName);
            var mediaFile = await _context.MediaFiles.FirstOrDefaultAsync(m => m.FileName == asset.FileName);
            if (mediaFile == null)
            {
                mediaFile = new MediaFile(asset.FileName, asset.FileName, asset.ContentType, 0, blobUrl);
                _context.MediaFiles.Add(mediaFile);
                await _context.SaveChangesAsync();
            }

            if (mediaFile.BlobUrl != blobUrl)
                mediaFile.SetBlobUrl(blobUrl);
            mediaFile.SetAltText(asset.AltText);

            var existingPartner = existingPartners.FirstOrDefault(fp =>
                fp.MediaFileId == mediaFile.Id ||
                fp.MediaFile.FileName == asset.FileName ||
                fp.MediaFile.BlobUrl.EndsWith($"/{asset.FileName}", StringComparison.OrdinalIgnoreCase));

            if (existingPartner != null)
            {
                existingPartner.SetMediaFileId(mediaFile.Id);
                existingPartner.SetAltText(asset.AltText);
                existingPartner.SetSortOrder(sortOrder++);
                continue;
            }

            _context.FooterPartners.Add(new FooterPartner(settings.Id, mediaFile.Id, asset.AltText, null, sortOrder++));
        }

        await _context.SaveChangesAsync();
    }

    private async Task FixMenuHierarchy()
    {
        var primaryMenu = _context.NavigationMenus
            .FirstOrDefault(m => m.Location == MenuLocation.Primary);

        if (primaryMenu == null) return;

        var items = _context.NavigationMenuItems
            .Where(i => i.MenuId == primaryMenu.Id)
            .Include(i => i.Page)
            .ToList();

        var privacyItem = items.FirstOrDefault(i => i.Page != null && i.Page.Slug == "politique-confidentialite");
        if (privacyItem != null)
            _context.NavigationMenuItems.Remove(privacyItem);

        var mesCampsItem = items.FirstOrDefault(i => i.Label == "Mes camps" && i.ParentId == null);
        if (mesCampsItem == null)
        {
            mesCampsItem = new NavigationMenuItem(primaryMenu.Id, "Mes camps", 3);
            _context.NavigationMenuItems.Add(mesCampsItem);
            await _context.SaveChangesAsync();
        }

        var campItems = items.Where(i => i.Page != null && CampSlugs.Contains(i.Page.Slug)).ToList();
        foreach (var campItem in campItems)
        {
            if (campItem.ParentId != mesCampsItem.Id)
                campItem.SetParentId(mesCampsItem.Id);
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

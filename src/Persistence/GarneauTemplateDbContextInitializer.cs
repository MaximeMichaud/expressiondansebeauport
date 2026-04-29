using Domain.Constants.User;
using Domain.Entities;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
    }

    private async Task SeedDemoBlocksPage()
    {
        const string slug = "demo-blocs-visuels";
        var existing = _context.Pages.FirstOrDefault(p => p.Slug == slug);
        if (existing != null) return;

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
                    "{\"url\":\"/image-devant-studio.jpg\",\"alt\":\"Devant le studio\"}," +
                    "{\"url\":\"/vue-de-rue-education.jpg\",\"alt\":\"Vue de la rue\"}" +
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
                    "{\"url\":\"/image-devant-studio.jpg\",\"alt\":\"Devant le studio\"}," +
                    "{\"url\":\"/vue-de-rue-education.jpg\",\"alt\":\"Vue de rue\"}," +
                    "{\"url\":\"/directions-sur-map.jpg\",\"alt\":\"Plan d'accès\"}" +
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

        // Patterns d'URLs obsolètes provenant d'anciennes versions du seed
        var obsoleteUrlPatterns = new[] { "/uploads/seed-", "/images/seed/", "/images/" };

        var mediaIds = new List<Guid>();
        foreach (var (fileName, originalName, size, alt) in seedImages)
        {
            var correctUrl = $"/{fileName}";
            var existing = _context.Set<MediaFile>().FirstOrDefault(m => m.FileName == fileName);
            if (existing != null)
            {
                if (obsoleteUrlPatterns.Any(p => existing.BlobUrl.Contains(p)) || existing.BlobUrl != correctUrl)
                    existing.SetBlobUrl(correctUrl);
                mediaIds.Add(existing.Id);
                continue;
            }

            // Chercher aussi les anciens MediaFile avec le préfixe seed-
            var legacy = _context.Set<MediaFile>().FirstOrDefault(m =>
                m.FileName == $"seed-{fileName}" || m.BlobUrl.Contains($"seed-{fileName}"));
            if (legacy != null)
            {
                legacy.SetBlobUrl(correctUrl);
                mediaIds.Add(legacy.Id);
                continue;
            }

            var media = new MediaFile(fileName, originalName, "image/jpeg", size, correctUrl);
            media.SetAltText(alt);
            _context.Set<MediaFile>().Add(media);
            await _context.SaveChangesAsync();
            mediaIds.Add(media.Id);
        }

        var imagesJson = string.Join(",", seedImages.Select((img, i) =>
            $"{{\"url\":\"/{img.Item1}\",\"alt\":\"{img.Item4.Replace("\"", "\\\"")}\"}}"
        ));

        var blocksJson =
            "[" +
                "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
                    "<h2>Nous joindre</h2>" +
                    "<p>Pour toutes questions ou informations supplémentaires, n'hésitez surtout pas à nous joindre.</p>" +
                    "<p><strong>Téléphone :</strong> <a href=\\\"tel:4186666158\\\">418-666-6158</a></p>" +
                    "<p><strong>Courriel :</strong> <a href=\\\"mailto:info@expressiondansebeauport.com\\\">info@expressiondansebeauport.com</a></p>" +
                    "<h3>Nos locaux</h3>" +
                    "<p>Centre de loisirs Ste-Gertrude<br/>788, avenue du Cénacle</p>" +
                    "<h3>Adresse postale</h3>" +
                    "<p>CP 29009 QUÉ CP RAYMOND PO<br/>G1B 3G0, Québec, QC</p>" +
                "\"}}," +
                "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"google-map\",\"data\":{" +
                    "\"embedUrl\":\"https://www.google.com/maps?q=788+avenue+du+C%C3%A9nacle,+Qu%C3%A9bec,+QC+G1E+5J4&z=15&output=embed\"," +
                    "\"address\":\"788 avenue du Cénacle, Québec, QC G1E 5J4\"," +
                    "\"height\":400" +
                "}}," +
                "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"rich-text\",\"data\":{\"html\":\"" +
                    "<h2>Comment s'y rendre?</h2>" +
                    "<p>L'accès au stationnement et au local se fait par l'Avenue de l'Éducation. " +
                    "Google Maps peut parfois manquer de précision sur ce point, donc voici les repères visuels à suivre.</p>" +
                "\"}}," +
                "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"image-gallery\",\"data\":{\"images\":[" + imagesJson + "],\"columns\":3}}," +
                "{\"id\":\"" + Guid.NewGuid() + "\",\"type\":\"cta-button\",\"data\":{" +
                    "\"label\":\"S'inscrire maintenant\"," +
                    "\"url\":\"https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session\"," +
                    "\"style\":\"primary\",\"alignment\":\"center\",\"openInNewTab\":true" +
                "}}" +
            "]";

        // Si la page est déjà en mode blocks, vérifier si les URLs sont obsolètes
        if (page.ContentMode == "blocks")
        {
            var needsUpdate = page.Blocks != null &&
                obsoleteUrlPatterns.Any(p => page.Blocks.Contains(p));
            if (needsUpdate)
            {
                var updatedBlocks = page.Blocks!;
                foreach (var pattern in obsoleteUrlPatterns)
                {
                    // Remplacer /uploads/seed-X.jpg et /images/seed/X.jpg par /images/X.jpg
                    foreach (var (fileName, _, _, _) in seedImages)
                    {
                        updatedBlocks = updatedBlocks
                            .Replace($"/uploads/seed-{fileName}", $"/{fileName}")
                            .Replace($"/images/seed/{fileName}", $"/{fileName}")
                            .Replace($"/images/{fileName}", $"/{fileName}");
                    }
                }
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

    private static readonly string[] CampSlugs = ["camp-d-ete", "camp-d-hiver", "camp-relache"];

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

        if (created)
            await _context.SaveChangesAsync();

        if (!settings.SocialLinks.Any(sl => sl.Platform == "facebook"))
            _context.SocialLinks.Add(new SocialLink(settings.Id, "facebook", "https://www.facebook.com/expressiondansebeauport", 0));
        if (!settings.SocialLinks.Any(sl => sl.Platform == "instagram"))
            _context.SocialLinks.Add(new SocialLink(settings.Id, "instagram", "https://www.instagram.com/expressiondansebeauport/", 1));

        await _context.SaveChangesAsync();

        await SeedFooterPartners(settings);
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
            var blobUrl = $"/seed-partners/{asset.FileName}";
            if (existingPartners.Any(fp => fp.MediaFile.BlobUrl == blobUrl))
            {
                sortOrder++;
                continue;
            }

            var mediaFile = new MediaFile(asset.FileName, asset.FileName, asset.ContentType, 0, blobUrl);
            mediaFile.SetAltText(asset.AltText);
            _context.MediaFiles.Add(mediaFile);
            await _context.SaveChangesAsync();

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
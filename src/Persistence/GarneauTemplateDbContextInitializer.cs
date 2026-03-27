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
                "<p><strong>Adresse :</strong> 15, rue de la Promenade-des-Soeurs, Beauport, QC G1C 0G3</p>" +
                "<p><strong>Téléphone :</strong> <a href=\"tel:4186601086\">418-660-1086</a></p>" +
                "<p><strong>Courriel :</strong> <a href=\"mailto:info@expressiondansebeauport.com\">info@expressiondansebeauport.com</a></p>" +
                "<h3>Heures d'ouverture</h3>" +
                "<p>Lundi au vendredi : 16h00 - 21h00<br/>Samedi : 9h00 - 16h00<br/>Dimanche : Fermé</p>")
        };

            _context.Pages.AddRange(pages);
            await _context.SaveChangesAsync();
        }

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
    }

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

    private async Task SeedPageIfNotExists(string title, string slug, int sortOrder, string content, string? customCss = null)
    {
        var generatedSlug = Page.GenerateSlug(slug);
        if (!_context.Pages.Any(p => p.Slug == generatedSlug))
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
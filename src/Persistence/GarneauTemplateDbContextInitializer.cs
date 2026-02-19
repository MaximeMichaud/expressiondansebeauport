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

        // Accueil
        var accueil = new Page("Accueil - Expression Danse", "accueil", true);
        accueil.AddSection(new PageSection("Bienvenue", "<h2>Quand le talent et la passion riment avec plaisir!</h2><p>École de danse à Beauport, organisme de la ville de Québec. Cours de danse pour tous, enfant de 3 ans à adulte. De tous les styles: jazz, hip-hop, ballet, mise en forme, acrodanse, danse enfantine. Troupes compétitives en hip-hop, jazz et lyrique, à partir de 6 ans, 3 à 4 compétitions de danse dans l'année.</p>", null, 0));

        // Notre école
        var ecole = new Page("Notre école", "notre-ecole");
        ecole.AddSection(new PageSection("Mission", "<h3>Accessibilité</h3><p>Établie à Beauport depuis plus de 30 ans, l'école de danse Expression danse de Beauport a pour mission de favoriser l'accessibilité de la danse ainsi qu'un apprentissage de qualité et le dépassement de soi, axé sur le plaisir et le respect.</p><p>Enfant, adolescent ou adulte, nous avons un cours qui s'adresse à vous!</p><h3>Diversité</h3><p>Pour permettre à chacun de trouver le plaisir de danser, de se divertir ou de se mettre en forme, EDB s'engage à offrir une grande variété de cours.</p><p>Tous les cours sont offerts dans une ambiance conviviale par des professeurs passionnés et expérimentés autant pour notre volet récréatif que compétitif.</p><h3>Engagement</h3><p>Nous prenons l'engagement d'accompagner les danseurs dans le développement de leurs talents et de laisser une empreinte positive dans le cœur de tous ceux qui viennent vivre leur passion à EDB.</p>", null, 0));
        ecole.AddSection(new PageSection("Notre équipe", "<p>Une équipe de professeurs compétents, passionnés et dévoués qui souhaitent offrir des cours de qualité afin d'amener chaque élève à atteindre leurs propres objectifs que ce soit le dépassement de soi, la performance, le divertissement ou le bien-être.</p><p>Enseigner et partager son savoir faire, son art, son expérience dans la bonne humeur, dans le respect de chacun et dans le plaisir est une recette gagnante pour Expression Danse de Beauport.</p><p>Permettre à ses professeurs de se perfectionner et de suivre des formations continues de façon à ce que leur enseignement soit diversifié et réponde aux différents besoins de la clientèle.</p>", null, 1));
        ecole.AddSection(new PageSection("Conseil d'administration", "<p>Fier partenaire de la Ville de Québec depuis plus de 30 ans, Expression danse de Beauport est un organisme à but non lucratif doté d'un conseil d'administration composé de parents bénévoles.</p><p>Sept membres sont élus à chaque année lors de l'assemblée générale qui se tient au mois de septembre. En plus de l'élection des membres, le conseil d'administration sortant présente, lors de l'assemblée générale, le bilan de l'année précédente et les projets à venir.</p><p>C'est une occasion en or pour découvrir l'école et pour vous impliquer dans les multiples activités de l'école.</p><p>Les sept membres élus ont la responsabilité de voir à la bonne gestion de l'école, de la soutenir de façon active et positive en plus de seconder la directrice dans l'évolution et l'innovation de notre magnifique école.</p>", null, 2));

        // Cours récréatifs
        var recreatifs = new Page("Cours récréatifs", "cours-recreatifs");
        recreatifs.AddSection(new PageSection("Cours récréatifs", "<p>De tout pour tous! Faire bouger nos tout-petits, aider au développement et au divertissement de nos adolescents et naturellement permettre à nos adultes de bénéficier des bienfaits extraordinaires que la danse peut apporter sont tous des objectifs dont Expression Danse de Beauport est fier de pouvoir offrir à sa communauté.</p><p>Au-delà de la performance, nous croyons que les cours offerts à Expression danse de Beauport doivent permettre aux danseurs d'expérimenter tout le plaisir de bien danser, dans une atmosphère d'amitié, d'entraide et de respect.</p>", null, 0));

        // Camp d'été
        var camp = new Page("Camp d'été", "camp-dete");
        camp.AddSection(new PageSection("Camp d'été", "<p>Votre enfant adore la danse et il/elle ne peut s'arrêter pendant les vacances? Vous cherchez un camp qui plaira à votre enfant pour l'été? Inscrivez votre enfant à notre camp de danse pour l'été qui arrive! Un camp spécialisé qui est à l'écoute de sa clientèle. Pour permettre à votre enfant de continuer sa passion durant les vacances d'été!</p><p><strong>Du 29 Juin au 21 août 2026!</strong></p><p>Camp de danse: 9h à 16h<br>Service de garde inclus pour tous: 7h30 à 9h / 16h à 17h</p><p><strong>207$ / Semaine</strong><br>10$ de rabais à partir de la 4e semaine achetée.</p><p>Offert aux danseurs de 5 à 12 ans seulement!</p>", null, 0));
        camp.AddSection(new PageSection("Le camp d'été de danse EDB c'est quoi?", "<ul><li>De la danse à tous les jours!</li><li>Des moniteurs qui sont aussi professionnels de la danse.</li><li>Des activités extérieurs et de la baignade lors des belles journées.</li><li>Une foule de plaisir à l'intérieur lors de journée plus grises!</li><li>Une présentation de danse ou une vidéo personnalisée à tous les vendredis!</li><li>Des personnes significatives qui sont toujours présentes pour s'assurer de la sécurité et du bon déroulement du camp d'été!</li><li>Des collations spéciales à tous les jours!</li></ul><p>Vous avez des questions? N'hésitez pas à nous écrire!<br><strong>info@expressiondansebeauport.com</strong></p>", null, 1));

        // Troupes compétitives
        var troupes = new Page("Troupes compétitives", "troupes-competitives");
        troupes.AddSection(new PageSection("Puis-je faire partie des troupes compétitives?", "<p>La réponse est définitivement oui!</p><p>Ton meilleur atout pour faire partie des troupes compétitives, c'est ta détermination et ta passion! Si tu t'es rendu jusqu'ici pour prendre des informations sur nous, c'est que tu es prêt.e! On croit souvent que pour être dans une troupe compétitive on doit être flexible et avoir une bonne technique. Chez Expression Danse, on préfère des danseurs passionnés et déterminés qui voudront apprendre la bonne technique et travailler leur flexibilité! On ne s'attend pas à ce que nos danseurs soient déjà des étoiles. On veut leur permettre de devenir des étoiles!</p><p>Pour la prochaine saison, il est possible d'auditionner pour les troupes suivantes:</p><ul><li>5 à 14 ans | Hip-Hop</li><li>5 à 14 ans | Artistique (Jazz, Lyrique, Contemporain)</li><li>15 à 19 ans | Contemporain</li><li>19 ans et plus | Multi Style (Hip-Hop et Artistique)</li></ul>", null, 0));
        troupes.AddSection(new PageSection("Pourquoi choisir Expression Danse de Beauport?", "<ul><li>Parce que nous croyons que chaque danseur doit avoir sa chance.</li><li>Parce que nous offrons des cours de niveau compétitif pour les débutants, intermédiaires et avancés.</li><li>Parce que nos troupes accueillent des jeunes de 6 ans et plus.</li><li>Parce que la danse est avant tout une passion et non un travail!</li><li>On doit pouvoir évoluer dans le plaisir!</li><li>Parce que nous croyons que l'argent ne doit pas être un frein aux talents des danseurs.</li><li>Parce que nous sommes une grande famille et que nous sommes toujours là les uns pour les autres!</li></ul>", null, 1));

        // Nous joindre
        var contact = new Page("Nous joindre", "nous-joindre");
        contact.AddSection(new PageSection("Nous joindre", "<p>Pour toutes questions ou informations supplémentaires, n'hésitez surtout pas à nous joindre.</p>", null, 0));
        contact.AddSection(new PageSection("Nos locaux", "<p><strong>Centre de loisirs Ste-Gertrude</strong><br>788, avenue du Cénacle</p><p><strong>Adresse postale</strong><br>CP 29009 QUÉ CP RAYMOND PO<br>G1B 3G0, QUÉBEC, QC</p><p><strong>Téléphone:</strong> 418-666-6158<br><strong>Courriel:</strong> info@expressiondansebeauport.com</p>", null, 1));

        // Politiques de confidentialité
        var politiques = new Page("Politiques de confidentialité", "politiques-de-confidentialite");
        politiques.AddSection(new PageSection("Politiques de confidentialité", "<p>Nous nous engageons à préserver l'exactitude, la confidentialité et la sécurité de vos renseignements personnels. Nous suivons notre politique de confidentialité concernant la collecte, l'utilisation et la divulgation de renseignements personnels. Notre politique est basée sur les valeurs définies par le code modèle de l'Association canadienne de normalisation et la Loi sur la protection des renseignements personnels et les documents électroniques du Canada.</p><p><strong>Date d'entrée en vigueur: 22 septembre 2023</strong></p>", null, 0));
        politiques.AddSection(new PageSection("Le but de cette politique de confidentialité", "<p>Cette politique informe les utilisateurs des données personnelles que nous collectons et comprend:</p><ol><li>Les données personnelles que nous collectons</li><li>L'utilisation des données collectées</li><li>Qui accède aux données collectées</li><li>Les droits des utilisateurs du site</li><li>La politique de cookies du site</li></ol><p>Cette politique fonctionne conjointement avec nos conditions d'utilisation du site.</p>", null, 1));
        politiques.AddSection(new PageSection("Consentement", "<p>Les utilisateurs acceptent qu'en utilisant notre site, ils consentent à:</p><p>a. Les termes énoncés dans cette politique de confidentialité<br>b. La collecte, l'utilisation et la conservation des données énumérées dans cette politique</p>", null, 2));
        politiques.AddSection(new PageSection("Données personnelles que nous collectons", "<p><strong>Données collectées automatiquement</strong></p><p>Lors de la visite et de l'utilisation de notre site, nous pouvons automatiquement collecter et conserver:</p><ul><li>Adresse IP</li><li>Liens sur lesquels l'utilisateur clique en utilisant le site</li><li>Contenu que l'utilisateur consulte sur le site</li><li>Adresse postale</li><li>Adresse courriel</li><li>Prénom et nom de famille</li><li>Informations de carte de crédit collectées par notre processeur de paiement</li></ul><p><strong>Données collectées non automatiquement</strong></p><p>Nous pouvons collecter les données suivantes lorsque vous effectuez certaines fonctions du site:</p><ul><li>Prénom et nom de famille</li><li>Courriel</li><li>Numéro de téléphone</li><li>Informations de paiement (via nos partenaires de paiement uniquement)</li><li>Adresse postale</li><li>Genre</li><li>Date de naissance et âge</li><li>Numéro d'assurance sociale (pour la création de feuillets T4 – Camp d'été uniquement)</li><li>Autorisations photos</li></ul>", null, 3));
        politiques.AddSection(new PageSection("Vos droits en tant qu'utilisateur", "<p>En tant qu'utilisateur, vous avez le droit d'accéder à toutes les données personnelles que nous avons collectées. De plus, vous avez le droit de mettre à jour ou de corriger toute donnée personnelle en notre possession, à condition que cela soit acceptable en vertu de la loi.</p><p>Vous pouvez choisir de supprimer ou de modifier votre consentement à la collecte et à l'utilisation des données à tout moment, à condition que cela soit légalement acceptable et que vous nous en informiez dans un délai raisonnable.</p><p>Pour demander la suppression ou la modification de vos informations, contactez notre agent de confidentialité:</p><p><strong>Alexandra St-Pierre</strong><br>Direction@expressiondansebeauport.com</p>", null, 4));
        politiques.AddSection(new PageSection("Contact", "<p>Si vous avez des questions, n'hésitez pas à nous contacter en utilisant les informations suivantes:</p><p>À l'attention du responsable de la protection des renseignements personnels:</p><p><strong>Alexandra St-Pierre</strong><br>Direction@expressiondansebeauport.com<br>418-666-6158</p><p><em>Mise à jour le 11 novembre 2024</em></p>", null, 5));

        _context.Pages.AddRange(new[] { accueil, ecole, recreatifs, camp, troupes, contact, politiques });
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
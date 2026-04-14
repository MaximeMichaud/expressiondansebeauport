# Group Join Requests — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow members to request to join a group via the messaging system, where professors can accept or reject directly from the conversation.

**Architecture:** New `JoinRequest` entity as centralized state, referenced by typed `Message` records. Backend service orchestrates creation (multi-prof fan-out) and resolution (accept/reject with auto-reply). Frontend renders a special `JoinRequestCard` inside conversation bubbles and adds a two-option modal to the group portal.

**Tech Stack:** .NET 10 / FastEndpoints / EF Core (PostgreSQL + NodaTime) / SignalR / Vue 3 + TypeScript + Tailwind

---

### Task 1: Domain layer — Enums, entity, repository interface

**Files:**
- Create: `src/Domain/Enums/JoinRequestStatus.cs`
- Create: `src/Domain/Enums/MessageType.cs`
- Create: `src/Domain/Entities/JoinRequest.cs`
- Modify: `src/Domain/Entities/Message.cs`
- Create: `src/Domain/Repositories/IJoinRequestRepository.cs`
- Modify: `src/Domain/Repositories/IGroupMemberRepository.cs`

- [ ] **Step 1: Create JoinRequestStatus enum**

```csharp
// src/Domain/Enums/JoinRequestStatus.cs
namespace Domain.Enums;

public enum JoinRequestStatus
{
    Pending,
    Accepted,
    Rejected
}
```

- [ ] **Step 2: Create MessageType enum**

```csharp
// src/Domain/Enums/MessageType.cs
namespace Domain.Enums;

public enum MessageType
{
    Text,
    JoinRequest
}
```

- [ ] **Step 3: Create JoinRequest entity**

```csharp
// src/Domain/Entities/JoinRequest.cs
using Domain.Common;
using Domain.Enums;
using NodaTime;

namespace Domain.Entities;

public class JoinRequest : Entity
{
    public Guid GroupId { get; private set; }
    public Group Group { get; private set; } = null!;
    public Guid RequesterMemberId { get; private set; }
    public Member RequesterMember { get; private set; } = null!;
    public JoinRequestStatus Status { get; private set; }
    public Guid? ResolvedByMemberId { get; private set; }
    public Member? ResolvedByMember { get; private set; }
    public Instant? ResolvedAt { get; private set; }
    public Instant CreatedAt { get; private set; }

    public void SetGroup(Group group) { Group = group; GroupId = group.Id; }
    public void SetGroupId(Guid groupId) => GroupId = groupId;
    public void SetRequesterMember(Member member) { RequesterMember = member; RequesterMemberId = member.Id; }
    public void SetRequesterMemberId(Guid memberId) => RequesterMemberId = memberId;
    public void SetStatus(JoinRequestStatus status) => Status = status;
    public void SetResolvedByMember(Member member) { ResolvedByMember = member; ResolvedByMemberId = member.Id; }
    public void SetResolvedAt(Instant resolvedAt) => ResolvedAt = resolvedAt;
    public void SetCreatedAt(Instant createdAt) => CreatedAt = createdAt;
}
```

- [ ] **Step 4: Modify Message entity — add MessageType and JoinRequestId**

Add these properties and setters to `src/Domain/Entities/Message.cs`, after the existing `Media` collection:

```csharp
public MessageType MessageType { get; private set; }
public Guid? JoinRequestId { get; private set; }
public JoinRequest? JoinRequest { get; private set; }

public void SetMessageType(MessageType type) => MessageType = type;
public void SetJoinRequest(JoinRequest joinRequest) { JoinRequest = joinRequest; JoinRequestId = joinRequest.Id; }
public void SetJoinRequestId(Guid? joinRequestId) => JoinRequestId = joinRequestId;
```

Add `using Domain.Enums;` to the imports.

- [ ] **Step 5: Create IJoinRequestRepository**

```csharp
// src/Domain/Repositories/IJoinRequestRepository.cs
using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IJoinRequestRepository
{
    Task Add(JoinRequest joinRequest);
    Task<JoinRequest?> FindById(Guid id, bool asNoTracking = true);
    Task<JoinRequest?> FindPendingByGroupAndMember(Guid groupId, Guid memberId);
    Task Update(JoinRequest joinRequest);
}
```

- [ ] **Step 6: Add GetProfessorsOfGroup to IGroupMemberRepository**

Add this method to `src/Domain/Repositories/IGroupMemberRepository.cs`:

```csharp
Task<List<GroupMember>> GetProfessorsOfGroup(Guid groupId);
```

- [ ] **Step 7: Commit**

```bash
git add src/Domain/Enums/JoinRequestStatus.cs src/Domain/Enums/MessageType.cs src/Domain/Entities/JoinRequest.cs src/Domain/Entities/Message.cs src/Domain/Repositories/IJoinRequestRepository.cs src/Domain/Repositories/IGroupMemberRepository.cs
git commit -m "feat(social): add JoinRequest domain layer — entity, enums, repository interface"
```

---

### Task 2: Persistence layer — EF configuration + DbContext

**Files:**
- Create: `src/Persistence/Configurations/JoinRequestConfiguration.cs`
- Modify: `src/Persistence/Configurations/MessageConfiguration.cs`
- Modify: `src/Persistence/GarneauTemplateDbContext.cs`

- [ ] **Step 1: Create JoinRequestConfiguration**

```csharp
// src/Persistence/Configurations/JoinRequestConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class JoinRequestConfiguration : IEntityTypeConfiguration<JoinRequest>
{
    public void Configure(EntityTypeBuilder<JoinRequest> builder)
    {
        builder.HasKey(jr => jr.Id);

        builder.Property(jr => jr.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(jr => jr.Group)
            .WithMany()
            .HasForeignKey(jr => jr.GroupId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(jr => jr.RequesterMember)
            .WithMany()
            .HasForeignKey(jr => jr.RequesterMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(jr => jr.ResolvedByMember)
            .WithMany()
            .HasForeignKey(jr => jr.ResolvedByMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(jr => new { jr.GroupId, jr.RequesterMemberId, jr.Status });
    }
}
```

- [ ] **Step 2: Update MessageConfiguration — add MessageType and JoinRequest FK**

Add to the `Configure` method in `src/Persistence/Configurations/MessageConfiguration.cs`, after existing config:

```csharp
builder.Property(m => m.MessageType)
    .HasConversion<string>()
    .HasMaxLength(20)
    .HasDefaultValue(Domain.Enums.MessageType.Text);

builder.HasOne(m => m.JoinRequest)
    .WithMany()
    .HasForeignKey(m => m.JoinRequestId)
    .OnDelete(DeleteBehavior.SetNull);
```

Add `using Domain.Enums;` to imports if not already present.

- [ ] **Step 3: Add DbSet to GarneauTemplateDbContext**

Add to `src/Persistence/GarneauTemplateDbContext.cs` in the social platform entities section:

```csharp
public DbSet<JoinRequest> JoinRequests { get; set; } = null!;
```

- [ ] **Step 4: Generate EF Core migration**

Run from the repo root:

```bash
cd /Users/alexandreroy/repos/expressiondansebeauport
dotnet ef migrations add AddJoinRequests --project src/Persistence --startup-project src/Web
```

Expected: a new migration file in `src/Persistence/Migrations/` with `AddJoinRequests` suffix creating the `JoinRequests` table and adding `MessageType` + `JoinRequestId` columns to `Messages`.

- [ ] **Step 5: Verify the build compiles**

```bash
dotnet build src/Web
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/Persistence/Configurations/JoinRequestConfiguration.cs src/Persistence/Configurations/MessageConfiguration.cs src/Persistence/GarneauTemplateDbContext.cs "src/Persistence/Migrations/*AddJoinRequests*"
git commit -m "feat(social): add JoinRequest EF configuration and migration"
```

---

### Task 3: Infrastructure layer — Repository implementations + DI

**Files:**
- Create: `src/Infrastructure/Repositories/JoinRequests/JoinRequestRepository.cs`
- Modify: `src/Infrastructure/Repositories/Groups/GroupMemberRepository.cs`
- Modify: `src/Infrastructure/ConfigureServices.cs`

- [ ] **Step 1: Create JoinRequestRepository**

```csharp
// src/Infrastructure/Repositories/JoinRequests/JoinRequestRepository.cs
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.JoinRequests;

public class JoinRequestRepository : IJoinRequestRepository
{
    private readonly GarneauTemplateDbContext _context;

    public JoinRequestRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(JoinRequest joinRequest)
    {
        _context.JoinRequests.Add(joinRequest);
        await _context.SaveChangesAsync();
    }

    public async Task<JoinRequest?> FindById(Guid id, bool asNoTracking = true)
    {
        var query = _context.JoinRequests
            .Include(jr => jr.Group)
            .Include(jr => jr.RequesterMember)
            .Include(jr => jr.ResolvedByMember)
            .AsQueryable();
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(jr => jr.Id == id);
    }

    public async Task<JoinRequest?> FindPendingByGroupAndMember(Guid groupId, Guid memberId)
    {
        return await _context.JoinRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(jr =>
                jr.GroupId == groupId &&
                jr.RequesterMemberId == memberId &&
                jr.Status == JoinRequestStatus.Pending);
    }

    public async Task Update(JoinRequest joinRequest)
    {
        if (_context.Entry(joinRequest).State == EntityState.Detached)
            _context.JoinRequests.Update(joinRequest);
        await _context.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Add GetProfessorsOfGroup to GroupMemberRepository**

Add this method to `src/Infrastructure/Repositories/Groups/GroupMemberRepository.cs`:

```csharp
public async Task<List<GroupMember>> GetProfessorsOfGroup(Guid groupId)
{
    return await _context.GroupMembers
        .AsNoTracking()
        .Where(gm => gm.GroupId == groupId && gm.Role == GroupMemberRole.Professor)
        .Include(gm => gm.Member).ThenInclude(m => m.User)
        .ToListAsync();
}
```

- [ ] **Step 3: Register JoinRequestRepository in DI**

Add to `src/Infrastructure/ConfigureServices.cs` after the `IMessageRepository` line:

```csharp
services.AddScoped<IJoinRequestRepository, JoinRequestRepository>();
```

Add to imports at the top:

```csharp
using Infrastructure.Repositories.JoinRequests;
```

- [ ] **Step 4: Verify build**

```bash
dotnet build src/Web
```

Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add src/Infrastructure/Repositories/JoinRequests/JoinRequestRepository.cs src/Infrastructure/Repositories/Groups/GroupMemberRepository.cs src/Infrastructure/ConfigureServices.cs
git commit -m "feat(social): add JoinRequest repository + GetProfessorsOfGroup"
```

---

### Task 4: Application layer — Update ConversationService + JoinRequestService

**Files:**
- Modify: `src/Application/Services/Messaging/IConversationService.cs`
- Modify: `src/Application/Services/Messaging/ConversationService.cs`
- Create: `src/Application/Services/JoinRequests/IJoinRequestService.cs`
- Create: `src/Application/Services/JoinRequests/JoinRequestService.cs`
- Modify: `src/Application/ConfigureServices.cs`

- [ ] **Step 1: Add MessageType + JoinRequestId params to IConversationService.SendMessage**

In `src/Application/Services/Messaging/IConversationService.cs`, add `using Domain.Enums;` to imports, then replace the `SendMessage` signature:

```csharp
Task<Message> SendMessage(
    Guid conversationId,
    Guid senderMemberId,
    string? content,
    IReadOnlyList<MessageMediaItem> media,
    MessageType messageType = MessageType.Text,
    Guid? joinRequestId = null);
```

- [ ] **Step 2: Update ConversationService.SendMessage implementation**

In `src/Application/Services/Messaging/ConversationService.cs`, add `using Domain.Enums;` to imports, update the `SendMessage` signature to match the interface, then add these two lines after `message.SetContent(content ?? string.Empty);`:

```csharp
message.SetMessageType(messageType);
message.SetJoinRequestId(joinRequestId);
```

- [ ] **Step 3: Create IJoinRequestService**

```csharp
// src/Application/Services/JoinRequests/IJoinRequestService.cs
using Domain.Entities;

namespace Application.Services.JoinRequests;

public interface IJoinRequestService
{
    Task<JoinRequest> CreateRequest(Guid groupId, Guid requesterMemberId);
    Task AcceptRequest(Guid joinRequestId, Guid professorMemberId);
    Task RejectRequest(Guid joinRequestId, Guid professorMemberId);
    Task<JoinRequest?> GetPendingRequest(Guid groupId, Guid memberId);
    Task<JoinRequest?> GetJoinRequestById(Guid id);
    Task<List<GroupMember>> GetProfessorsForGroup(Guid groupId);
}
```

- [ ] **Step 4: Create JoinRequestService**

```csharp
// src/Application/Services/JoinRequests/JoinRequestService.cs
using Application.Services.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Domain.Repositories;

namespace Application.Services.JoinRequests;

public class JoinRequestService : IJoinRequestService
{
    private readonly IJoinRequestRepository _joinRequestRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IConversationService _conversationService;

    public JoinRequestService(
        IJoinRequestRepository joinRequestRepository,
        IGroupMemberRepository groupMemberRepository,
        IGroupRepository groupRepository,
        IMemberRepository memberRepository,
        IConversationService conversationService)
    {
        _joinRequestRepository = joinRequestRepository;
        _groupMemberRepository = groupMemberRepository;
        _groupRepository = groupRepository;
        _memberRepository = memberRepository;
        _conversationService = conversationService;
    }

    public async Task<JoinRequest> CreateRequest(Guid groupId, Guid requesterMemberId)
    {
        var group = await _groupRepository.FindById(groupId);
        if (group == null)
            throw new InvalidOperationException("Group not found.");

        var isMember = await _groupMemberRepository.IsMember(groupId, requesterMemberId);
        if (isMember)
            throw new InvalidOperationException("Already a member of this group.");

        var existing = await _joinRequestRepository.FindPendingByGroupAndMember(groupId, requesterMemberId);
        if (existing != null)
            throw new InvalidOperationException("DUPLICATE");

        var requester = _memberRepository.FindById(requesterMemberId);
        if (requester == null)
            throw new InvalidOperationException("Member not found.");

        var professors = await _groupMemberRepository.GetProfessorsOfGroup(groupId);
        if (professors.Count == 0)
            throw new InvalidOperationException("No professors in this group.");

        var joinRequest = new JoinRequest();
        joinRequest.SetId(Guid.NewGuid());
        joinRequest.SetGroupId(groupId);
        joinRequest.SetRequesterMemberId(requesterMemberId);
        joinRequest.SetStatus(JoinRequestStatus.Pending);
        joinRequest.SetCreatedAt(InstantHelper.GetLocalNow());

        await _joinRequestRepository.Add(joinRequest);

        var content = $"{requester.FullName} souhaite rejoindre le groupe {group.Name}";

        foreach (var prof in professors)
        {
            var conversation = await _conversationService.GetOrCreateConversation(requesterMemberId, prof.MemberId);
            if (conversation == null) continue;

            await _conversationService.SendMessage(
                conversation.Id,
                requesterMemberId,
                content,
                new List<MessageMediaItem>(),
                MessageType.JoinRequest,
                joinRequest.Id);
        }

        return joinRequest;
    }

    public async Task AcceptRequest(Guid joinRequestId, Guid professorMemberId)
    {
        var joinRequest = await _joinRequestRepository.FindById(joinRequestId, asNoTracking: false);
        if (joinRequest == null)
            throw new InvalidOperationException("Join request not found.");

        if (joinRequest.Status != JoinRequestStatus.Pending)
            throw new InvalidOperationException("Join request already resolved.");

        var profGm = await _groupMemberRepository.FindProfessorInGroup(joinRequest.GroupId, professorMemberId);
        if (profGm == null)
            throw new InvalidOperationException("Not a professor in this group.");

        var professor = _memberRepository.FindById(professorMemberId, asNoTracking: false);
        if (professor == null)
            throw new InvalidOperationException("Professor not found.");

        joinRequest.SetStatus(JoinRequestStatus.Accepted);
        joinRequest.SetResolvedByMember(professor);
        joinRequest.SetResolvedAt(InstantHelper.GetLocalNow());
        await _joinRequestRepository.Update(joinRequest);

        var gm = new GroupMember();
        gm.SetId(Guid.NewGuid());
        gm.SetGroupId(joinRequest.GroupId);
        gm.SetMemberId(joinRequest.RequesterMemberId);
        gm.SetRole(GroupMemberRole.Member);
        gm.SetJoinedAt(InstantHelper.GetLocalNow());
        await _groupMemberRepository.Add(gm);

        var group = await _groupRepository.FindById(joinRequest.GroupId);
        var conversation = await _conversationService.GetOrCreateConversation(joinRequest.RequesterMemberId, professorMemberId);
        if (conversation != null)
        {
            await _conversationService.SendMessage(
                conversation.Id,
                professorMemberId,
                $"✅� {professor.FullName} a accepté votre demande pour {group?.Name ?? "le groupe"}",
                new List<MessageMediaItem>());
        }
    }

    public async Task RejectRequest(Guid joinRequestId, Guid professorMemberId)
    {
        var joinRequest = await _joinRequestRepository.FindById(joinRequestId, asNoTracking: false);
        if (joinRequest == null)
            throw new InvalidOperationException("Join request not found.");

        if (joinRequest.Status != JoinRequestStatus.Pending)
            throw new InvalidOperationException("Join request already resolved.");

        var profGm = await _groupMemberRepository.FindProfessorInGroup(joinRequest.GroupId, professorMemberId);
        if (profGm == null)
            throw new InvalidOperationException("Not a professor in this group.");

        var professor = _memberRepository.FindById(professorMemberId, asNoTracking: false);
        if (professor == null)
            throw new InvalidOperationException("Professor not found.");

        joinRequest.SetStatus(JoinRequestStatus.Rejected);
        joinRequest.SetResolvedByMember(professor);
        joinRequest.SetResolvedAt(InstantHelper.GetLocalNow());
        await _joinRequestRepository.Update(joinRequest);

        var group = await _groupRepository.FindById(joinRequest.GroupId);
        var conversation = await _conversationService.GetOrCreateConversation(joinRequest.RequesterMemberId, professorMemberId);
        if (conversation != null)
        {
            await _conversationService.SendMessage(
                conversation.Id,
                professorMemberId,
                $"❌ {professor.FullName} a refusé votre demande pour {group?.Name ?? "le groupe"}",
                new List<MessageMediaItem>());
        }
    }

    public async Task<JoinRequest?> GetPendingRequest(Guid groupId, Guid memberId)
    {
        return await _joinRequestRepository.FindPendingByGroupAndMember(groupId, memberId);
    }

    public async Task<JoinRequest?> GetJoinRequestById(Guid id)
    {
        return await _joinRequestRepository.FindById(id);
    }

    public async Task<List<GroupMember>> GetProfessorsForGroup(Guid groupId)
    {
        return await _groupMemberRepository.GetProfessorsOfGroup(groupId);
    }
}
```

- [ ] **Step 4: Register JoinRequestService in DI**

Add to `src/Application/ConfigureServices.cs` after the `IConversationService` line:

```csharp
services.AddScoped<IJoinRequestService, JoinRequestService>();
```

Add imports:

```csharp
using Application.Services.JoinRequests;
```

- [ ] **Step 5: Verify build**

```bash
dotnet build src/Web
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/Application/Services/JoinRequests/ src/Application/ConfigureServices.cs
git commit -m "feat(social): add JoinRequestService with create, accept, reject, getPending"
```

---

### Task 5: Backend endpoints — Create, Accept, Reject, GetMine

**Files:**
- Create: `src/Web/Features/Social/Groups/JoinRequests/Create/CreateJoinRequestEndpoint.cs`
- Create: `src/Web/Features/Social/Groups/JoinRequests/Create/CreateJoinRequestRequest.cs`
- Create: `src/Web/Features/Social/Groups/JoinRequests/Accept/AcceptJoinRequestEndpoint.cs`
- Create: `src/Web/Features/Social/Groups/JoinRequests/Accept/AcceptJoinRequestRequest.cs`
- Create: `src/Web/Features/Social/Groups/JoinRequests/Reject/RejectJoinRequestEndpoint.cs`
- Create: `src/Web/Features/Social/Groups/JoinRequests/Reject/RejectJoinRequestRequest.cs`
- Create: `src/Web/Features/Social/Groups/JoinRequests/Mine/GetMyJoinRequestEndpoint.cs`
- Create: `src/Web/Features/Social/Groups/JoinRequests/Mine/GetMyJoinRequestRequest.cs`

- [ ] **Step 1: Create CreateJoinRequestRequest**

```csharp
// src/Web/Features/Social/Groups/JoinRequests/Create/CreateJoinRequestRequest.cs
namespace Web.Features.Social.Groups.JoinRequests.Create;

public class CreateJoinRequestRequest
{
    public Guid GroupId { get; set; }
}
```

- [ ] **Step 2: Create CreateJoinRequestEndpoint**

The service's `CreateRequest` handles both JoinRequest creation AND sending typed messages to professors via the updated `ConversationService.SendMessage`. The endpoint just orchestrates and handles SignalR notifications.

```csharp
// src/Web/Features/Social/Groups/JoinRequests/Create/CreateJoinRequestEndpoint.cs
using Application.Interfaces.Services.Users;
using Application.Services.JoinRequests;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Features.Social.Groups.JoinRequests.Create;

public class CreateJoinRequestEndpoint : Endpoint<CreateJoinRequestRequest>
{
    private readonly IJoinRequestService _joinRequestService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public CreateJoinRequestEndpoint(
        IJoinRequestService joinRequestService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IGroupRepository groupRepository,
        IHubContext<ChatHub> hubContext)
    {
        _joinRequestService = joinRequestService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _groupRepository = groupRepository;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/groups/{GroupId}/join-requests");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateJoinRequestRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        Domain.Entities.JoinRequest joinRequest;
        try
        {
            joinRequest = await _joinRequestService.CreateRequest(req.GroupId, member.Id);
        }
        catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE")
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("Duplicate", "Une demande est déjà en attente.")), ct);
            return;
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("Error", ex.Message)), ct);
            return;
        }

        var group = await _groupRepository.FindById(req.GroupId);
        var professors = await _joinRequestService.GetProfessorsForGroup(req.GroupId);

        foreach (var prof in professors)
        {
            var profMember = _memberRepository.FindById(prof.MemberId);
            if (profMember == null) continue;
            var connectionId = ChatHub.GetConnectionId(profMember.UserId.ToString());
            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                {
                    Id = Guid.NewGuid(),
                    Content = $"{member.FullName} souhaite rejoindre le groupe {group?.Name ?? ""}",
                    SenderName = member.FullName,
                    ConversationId = Guid.Empty,
                    MessageType = "JoinRequest",
                    JoinRequestId = joinRequest.Id,
                    JoinRequestStatus = "Pending",
                    GroupName = group?.Name,
                    RequesterName = member.FullName,
                    Media = Array.Empty<object>()
                }, ct);
            }
        }

        await Send.OkAsync(new { succeeded = true, joinRequestId = joinRequest.Id }, ct);
    }
}
```

- [ ] **Step 3: Create AcceptJoinRequestRequest**

```csharp
// src/Web/Features/Social/Groups/JoinRequests/Accept/AcceptJoinRequestRequest.cs
namespace Web.Features.Social.Groups.JoinRequests.Accept;

public class AcceptJoinRequestRequest
{
    public Guid Id { get; set; }
}
```

- [ ] **Step 4: Create AcceptJoinRequestEndpoint**

```csharp
// src/Web/Features/Social/Groups/JoinRequests/Accept/AcceptJoinRequestEndpoint.cs
using Application.Interfaces.Services.Users;
using Application.Services.JoinRequests;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Features.Social.Groups.JoinRequests.Accept;

public class AcceptJoinRequestEndpoint : Endpoint<AcceptJoinRequestRequest>
{
    private readonly IJoinRequestService _joinRequestService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public AcceptJoinRequestEndpoint(
        IJoinRequestService joinRequestService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IHubContext<ChatHub> hubContext)
    {
        _joinRequestService = joinRequestService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("social/join-requests/{Id}/accept");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AcceptJoinRequestRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        try
        {
            await _joinRequestService.AcceptRequest(req.Id, member.Id);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("Error", ex.Message)), ct);
            return;
        }

        var joinRequest = await _joinRequestService.GetJoinRequestById(req.Id);
        if (joinRequest != null)
        {
            var requesterMember = _memberRepository.FindById(joinRequest.RequesterMemberId);
            if (requesterMember != null)
            {
                var connectionId = ChatHub.GetConnectionId(requesterMember.UserId.ToString());
                if (connectionId != null)
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("JoinRequestResolved", new
                    {
                        JoinRequestId = joinRequest.Id,
                        Status = "Accepted",
                        ResolvedByName = member.FullName,
                        joinRequest.GroupId,
                        GroupName = joinRequest.Group?.Name
                    }, ct);
                }
            }

            var professors = await _joinRequestService.GetProfessorsForGroup(joinRequest.GroupId);
            foreach (var prof in professors)
            {
                if (prof.MemberId == member.Id) continue;
                var profMember = _memberRepository.FindById(prof.MemberId);
                if (profMember == null) continue;
                var connId = ChatHub.GetConnectionId(profMember.UserId.ToString());
                if (connId != null)
                {
                    await _hubContext.Clients.Client(connId).SendAsync("JoinRequestResolved", new
                    {
                        JoinRequestId = joinRequest.Id,
                        Status = "Accepted",
                        ResolvedByName = member.FullName,
                        joinRequest.GroupId,
                        GroupName = joinRequest.Group?.Name
                    }, ct);
                }
            }
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
```

- [ ] **Step 5: Create RejectJoinRequestRequest**

```csharp
// src/Web/Features/Social/Groups/JoinRequests/Reject/RejectJoinRequestRequest.cs
namespace Web.Features.Social.Groups.JoinRequests.Reject;

public class RejectJoinRequestRequest
{
    public Guid Id { get; set; }
}
```

- [ ] **Step 7: Create RejectJoinRequestEndpoint**

```csharp
// src/Web/Features/Social/Groups/JoinRequests/Reject/RejectJoinRequestEndpoint.cs
using Application.Interfaces.Services.Users;
using Application.Services.JoinRequests;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Features.Social.Groups.JoinRequests.Reject;

public class RejectJoinRequestEndpoint : Endpoint<RejectJoinRequestRequest>
{
    private readonly IJoinRequestService _joinRequestService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public RejectJoinRequestEndpoint(
        IJoinRequestService joinRequestService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IHubContext<ChatHub> hubContext)
    {
        _joinRequestService = joinRequestService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("social/join-requests/{Id}/reject");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(RejectJoinRequestRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        try
        {
            await _joinRequestService.RejectRequest(req.Id, member.Id);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("Error", ex.Message)), ct);
            return;
        }

        var joinRequest = await _joinRequestService.GetJoinRequestById(req.Id);
        if (joinRequest != null)
        {
            var requesterMember = _memberRepository.FindById(joinRequest.RequesterMemberId);
            if (requesterMember != null)
            {
                var connectionId = ChatHub.GetConnectionId(requesterMember.UserId.ToString());
                if (connectionId != null)
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("JoinRequestResolved", new
                    {
                        JoinRequestId = joinRequest.Id,
                        Status = "Rejected",
                        ResolvedByName = member.FullName,
                        joinRequest.GroupId,
                        GroupName = joinRequest.Group?.Name
                    }, ct);
                }
            }

            var professors = await _joinRequestService.GetProfessorsForGroup(joinRequest.GroupId);
            foreach (var prof in professors)
            {
                if (prof.MemberId == member.Id) continue;
                var profMember = _memberRepository.FindById(prof.MemberId);
                if (profMember == null) continue;
                var connId = ChatHub.GetConnectionId(profMember.UserId.ToString());
                if (connId != null)
                {
                    await _hubContext.Clients.Client(connId).SendAsync("JoinRequestResolved", new
                    {
                        JoinRequestId = joinRequest.Id,
                        Status = "Rejected",
                        ResolvedByName = member.FullName,
                        joinRequest.GroupId,
                        GroupName = joinRequest.Group?.Name
                    }, ct);
                }
            }
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
```

- [ ] **Step 8: Create GetMyJoinRequestRequest**

```csharp
// src/Web/Features/Social/Groups/JoinRequests/Mine/GetMyJoinRequestRequest.cs
namespace Web.Features.Social.Groups.JoinRequests.Mine;

public class GetMyJoinRequestRequest
{
    public Guid GroupId { get; set; }
}
```

- [ ] **Step 9: Create GetMyJoinRequestEndpoint**

```csharp
// src/Web/Features/Social/Groups/JoinRequests/Mine/GetMyJoinRequestEndpoint.cs
using Application.Interfaces.Services.Users;
using Application.Services.JoinRequests;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.JoinRequests.Mine;

public class GetMyJoinRequestEndpoint : Endpoint<GetMyJoinRequestRequest>
{
    private readonly IJoinRequestService _joinRequestService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public GetMyJoinRequestEndpoint(
        IJoinRequestService joinRequestService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _joinRequestService = joinRequestService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/groups/{GroupId}/join-requests/mine");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetMyJoinRequestRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        var pending = await _joinRequestService.GetPendingRequest(req.GroupId, member.Id);
        if (pending == null)
        {
            await Send.OkAsync(new { found = false }, ct);
            return;
        }

        await Send.OkAsync(new
        {
            found = true,
            joinRequestId = pending.Id,
            status = pending.Status.ToString()
        }, ct);
    }
}
```

- [ ] **Step 10: Verify build**

```bash
dotnet build src/Web
```

Expected: Build succeeded.

- [ ] **Step 11: Commit**

```bash
git add src/Web/Features/Social/Groups/JoinRequests/ src/Application/Services/JoinRequests/
git commit -m "feat(social): add join request endpoints — create, accept, reject, get-mine"
```

---

### Task 6: Update GetMessagesEndpoint to include join request data

**Files:**
- Modify: `src/Web/Features/Social/Messages/GetMessages/GetMessagesEndpoint.cs`
- Modify: `src/Infrastructure/Repositories/Messaging/MessageRepository.cs`

- [ ] **Step 1: Include JoinRequest in message query**

In `src/Infrastructure/Repositories/Messaging/MessageRepository.cs`, update the `GetByConversation` method to include the `JoinRequest` navigation:

Add `.Include(m => m.JoinRequest).ThenInclude(jr => jr!.Group)` and `.Include(m => m.JoinRequest).ThenInclude(jr => jr!.RequesterMember)` and `.Include(m => m.JoinRequest).ThenInclude(jr => jr!.ResolvedByMember)` to the query chain, after the existing `.Include(m => m.Media...)` line.

The full method becomes:

```csharp
public async Task<List<Message>> GetByConversation(Guid conversationId, int skip, int take)
{
    return await _context.Messages
        .AsNoTracking()
        .IgnoreQueryFilters()
        .Where(m => m.ConversationId == conversationId)
        .Include(m => m.SenderMember).ThenInclude(s => s.User)
        .Include(m => m.Media.OrderBy(x => x.SortOrder))
        .Include(m => m.JoinRequest).ThenInclude(jr => jr!.Group)
        .Include(m => m.JoinRequest).ThenInclude(jr => jr!.RequesterMember)
        .Include(m => m.JoinRequest).ThenInclude(jr => jr!.ResolvedByMember)
        .OrderByDescending(m => m.Created)
        .Skip(skip).Take(take)
        .ToListAsync();
}
```

- [ ] **Step 2: Update GetMessagesEndpoint response shape**

In `src/Web/Features/Social/Messages/GetMessages/GetMessagesEndpoint.cs`, update the `results` selector to include join request data. Replace the `return new { ... }` block inside the `.Select(m => ...)` lambda. After the `IsRead` line, add:

```csharp
MessageType = m.MessageType.ToString(),
JoinRequest = m.JoinRequest != null ? new
{
    m.JoinRequest.Id,
    m.JoinRequest.GroupId,
    GroupName = m.JoinRequest.Group?.Name,
    RequesterMemberId = m.JoinRequest.RequesterMemberId,
    RequesterName = m.JoinRequest.RequesterMember?.FullName ?? "Inconnu",
    Status = m.JoinRequest.Status.ToString(),
    ResolvedByName = m.JoinRequest.ResolvedByMember?.FullName
} : null
```

Add `using Domain.Enums;` if not present.

- [ ] **Step 3: Verify build**

```bash
dotnet build src/Web
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/Web/Features/Social/Messages/GetMessages/GetMessagesEndpoint.cs src/Infrastructure/Repositories/Messaging/MessageRepository.cs
git commit -m "feat(social): include join request data in message responses"
```

---

### Task 7: Frontend — TypeScript types + service methods

**Files:**
- Modify: `src/Web/vue-app/src/types/entities/social.ts`
- Modify: `src/Web/vue-app/src/services/socialService.ts`

- [ ] **Step 1: Add JoinRequest type and update Message type**

In `src/Web/vue-app/src/types/entities/social.ts`, add after the `Message` interface:

```typescript
export interface JoinRequestInfo {
  id: string
  groupId: string
  groupName: string
  requesterMemberId: string
  requesterName: string
  status: 'Pending' | 'Accepted' | 'Rejected'
  resolvedByName?: string
}
```

Update the `Message` interface to add:

```typescript
messageType?: string
joinRequest?: JoinRequestInfo
```

- [ ] **Step 2: Add service methods to socialService.ts**

Add these methods to `src/Web/vue-app/src/services/socialService.ts` in the SocialService class, in a new `// === Join Requests ===` section:

```typescript
// === Join Requests ===
async requestJoinGroup(groupId: string): Promise<any> {
  const response = await this._httpClient.post(`${API}/social/groups/${groupId}/join-requests`, {}, this.headersWithJsonContentType())
  return toCamel(response.data)
}

async acceptJoinRequest(joinRequestId: string): Promise<SucceededOrNotResponse> {
  const response = await this._httpClient.put<SucceededOrNotResponse>(`${API}/social/join-requests/${joinRequestId}/accept`, {}, this.headersWithJsonContentType())
  return response.data
}

async rejectJoinRequest(joinRequestId: string): Promise<SucceededOrNotResponse> {
  const response = await this._httpClient.put<SucceededOrNotResponse>(`${API}/social/join-requests/${joinRequestId}/reject`, {}, this.headersWithJsonContentType())
  return response.data
}

async getMyJoinRequest(groupId: string): Promise<any> {
  const response = await this._httpClient.get(`${API}/social/groups/${groupId}/join-requests/mine`)
  return toCamel(response.data)
}
```

- [ ] **Step 3: Verify frontend build**

```bash
cd /Users/alexandreroy/repos/expressiondansebeauport/src/Web/vue-app && npx vue-tsc --noEmit
```

Expected: no errors (or only pre-existing ones).

- [ ] **Step 4: Commit**

```bash
git add src/Web/vue-app/src/types/entities/social.ts src/Web/vue-app/src/services/socialService.ts
git commit -m "feat(social): add join request TypeScript types and service methods"
```

---

### Task 8: Frontend — JoinRequestCard component

**Files:**
- Create: `src/Web/vue-app/src/components/social/JoinRequestCard.vue`

- [ ] **Step 1: Create JoinRequestCard.vue**

```vue
<template>
  <div class="jr-card" :class="statusClass">
    <template v-if="status === 'Pending' && !isMine">
      <p class="jr-card__text">
        <strong>{{ requesterName }}</strong> souhaite rejoindre <strong>{{ groupName }}</strong>
      </p>
      <div class="jr-card__actions">
        <button
          class="jr-card__btn jr-card__btn--accept"
          :disabled="loading"
          @click="handleAccept"
        >
          {{ loading ? '...' : 'Accepter' }}
        </button>
        <button
          class="jr-card__btn jr-card__btn--reject"
          :disabled="loading"
          @click="handleReject"
        >
          Refuser
        </button>
      </div>
    </template>
    <template v-else-if="status === 'Pending' && isMine">
      <p class="jr-card__text">
        Vous avez demandé à rejoindre <strong>{{ groupName }}</strong>
      </p>
      <p class="jr-card__status">En attente...</p>
    </template>
    <template v-else-if="status === 'Accepted'">
      <p class="jr-card__text jr-card__text--resolved">
        ✅ Accepté par {{ resolvedByName }}
      </p>
    </template>
    <template v-else-if="status === 'Rejected'">
      <p class="jr-card__text jr-card__text--resolved">
        ❌ Refusé par {{ resolvedByName }}
      </p>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useSocialService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'

const props = defineProps<{
  joinRequestId: string
  groupName: string
  requesterName: string
  status: 'Pending' | 'Accepted' | 'Rejected'
  resolvedByName?: string
  isMine: boolean
}>()

const emit = defineEmits<{
  resolved: []
}>()

const socialService = useSocialService()
const toast = useSocialToast()
const loading = ref(false)

const statusClass = {
  'jr-card--pending': props.status === 'Pending',
  'jr-card--accepted': props.status === 'Accepted',
  'jr-card--rejected': props.status === 'Rejected',
}

async function handleAccept() {
  loading.value = true
  try {
    const result = await socialService.acceptJoinRequest(props.joinRequestId)
    if (result.succeeded) {
      toast.success('Membre ajouté au groupe!')
      emit('resolved')
    } else {
      toast.error(result.errors?.[0]?.errorMessage || 'Erreur')
    }
  } catch {
    toast.error('Erreur de connexion')
  }
  loading.value = false
}

async function handleReject() {
  loading.value = true
  try {
    const result = await socialService.rejectJoinRequest(props.joinRequestId)
    if (result.succeeded) {
      toast.success('Demande refusée.')
      emit('resolved')
    } else {
      toast.error(result.errors?.[0]?.errorMessage || 'Erreur')
    }
  } catch {
    toast.error('Erreur de connexion')
  }
  loading.value = false
}
</script>

<style lang="scss">
.jr-card {
  padding: 12px 16px;
  border-radius: 12px;
  background: var(--soc-card-bg, #f8f7f5);
  border: 1px solid var(--soc-border, #e5e3df);
  max-width: 300px;

  &__text {
    font-size: 0.85rem;
    line-height: 1.4;
    color: var(--soc-text, #1a1a1a);
    margin: 0 0 8px;

    strong {
      font-weight: 600;
    }

    &--resolved {
      margin-bottom: 0;
    }
  }

  &__status {
    font-size: 0.78rem;
    color: var(--soc-text-muted, #78716c);
    margin: 0;
  }

  &__actions {
    display: flex;
    gap: 8px;
  }

  &__btn {
    flex: 1;
    padding: 6px 12px;
    border-radius: 8px;
    border: none;
    font-size: 0.82rem;
    font-weight: 600;
    cursor: pointer;
    transition: opacity 0.15s;

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    &--accept {
      background: #22c55e;
      color: white;

      &:hover:not(:disabled) {
        opacity: 0.9;
      }
    }

    &--reject {
      background: var(--soc-input-bg, #f0eeeb);
      color: var(--soc-text, #1a1a1a);

      &:hover:not(:disabled) {
        background: var(--soc-bar-hover, #e5e3df);
      }
    }
  }
}
</style>
```

- [ ] **Step 2: Commit**

```bash
git add src/Web/vue-app/src/components/social/JoinRequestCard.vue
git commit -m "feat(social): add JoinRequestCard component for conversation bubbles"
```

---

### Task 9: Frontend — Update SocialConversation.vue

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialConversation.vue`

- [ ] **Step 1: Import JoinRequestCard**

Add to the imports in the `<script setup>` section, after the `ImageLightbox` import:

```typescript
import JoinRequestCard from '@/components/social/JoinRequestCard.vue'
```

- [ ] **Step 2: Add joinRequest fields to ChatMessage interface**

Add to the `ChatMessage` interface:

```typescript
messageType?: string
joinRequest?: {
  id: string
  groupId: string
  groupName: string
  requesterMemberId: string
  requesterName: string
  status: 'Pending' | 'Accepted' | 'Rejected'
  resolvedByName?: string
}
```

- [ ] **Step 3: Map joinRequest data in loadMessages and pollMessages**

In the `loadMessages` function, in the `raw.map(...)` callback, add after the `media` line:

```typescript
messageType: m.messageType,
joinRequest: m.joinRequest,
```

Do the same in `pollMessages` function's `raw.map(...)` callback.

- [ ] **Step 4: Update the template to render JoinRequestCard**

In the template, find the message rendering section. The current code has a `<!-- Deleted message -->` block followed by a `<!-- Normal message -->` block inside the `v-for="msg in allMessages"` loop. Add a new block between the deleted message block and the normal message block:

```html
<!-- Join request message -->
<JoinRequestCard
  v-else-if="msg.messageType === 'JoinRequest' && msg.joinRequest"
  :join-request-id="msg.joinRequest.id"
  :group-name="msg.joinRequest.groupName"
  :requester-name="msg.joinRequest.requesterName"
  :status="msg.joinRequest.status"
  :resolved-by-name="msg.joinRequest.resolvedByName"
  :is-mine="msg.isMine"
  @resolved="pollMessages"
/>
```

- [ ] **Step 5: Handle JoinRequestResolved SignalR event**

The current codebase doesn't use SignalR for this view (it uses polling). Since conversations already poll every 1000ms, the `JoinRequestCard` status will refresh automatically when the poll fetches updated message data. No additional SignalR handler needed — the polling already covers it.

- [ ] **Step 6: Verify frontend build**

```bash
cd /Users/alexandreroy/repos/expressiondansebeauport/src/Web/vue-app && npx vue-tsc --noEmit
```

- [ ] **Step 7: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialConversation.vue
git commit -m "feat(social): render JoinRequestCard in conversation messages"
```

---

### Task 10: Frontend — Update SocialPortal.vue modal

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialPortal.vue`

- [ ] **Step 1: Add new refs for join request state**

Add after the existing `joiningFromModal` ref:

```typescript
const joinModalMode = ref<'choice' | 'code'>('choice')
const requestingJoin = ref(false)
const pendingJoinRequestId = ref<string | null>(null)
const checkingPending = ref(false)
```

- [ ] **Step 2: Update onGroupClick to check for pending request**

Replace the `onGroupClick` function:

```typescript
async function onGroupClick(group: Group) {
  if (myGroupIds.value.has(group.id)) {
    router.push({ name: 'socialGroup', params: { id: group.id } })
    return
  }

  joinModalGroup.value = group
  joinModalCode.value = ''
  joinModalError.value = ''
  joinModalMode.value = 'choice'
  pendingJoinRequestId.value = null
  showJoinModal.value = true

  checkingPending.value = true
  try {
    const result = await socialService.getMyJoinRequest(group.id)
    if (result?.found) {
      pendingJoinRequestId.value = result.joinRequestId
    }
  } catch { /* */ }
  checkingPending.value = false
}
```

- [ ] **Step 3: Add requestToJoin function**

Add after `joinFromModal`:

```typescript
async function requestToJoin() {
  if (!joinModalGroup.value) return
  requestingJoin.value = true
  joinModalError.value = ''
  try {
    const result = await socialService.requestJoinGroup(joinModalGroup.value.id)
    if (result?.succeeded) {
      closeJoinModal()
      toast.success('Demande envoyée!')
    } else {
      joinModalError.value = result?.errors?.[0]?.errorMessage || 'Erreur.'
    }
  } catch {
    joinModalError.value = 'Erreur de connexion.'
  }
  requestingJoin.value = false
}
```

- [ ] **Step 4: Update closeJoinModal to reset new state**

Replace `closeJoinModal`:

```typescript
function closeJoinModal() {
  showJoinModal.value = false
  joinModalGroup.value = null
  joinModalCode.value = ''
  joinModalError.value = ''
  joinModalMode.value = 'choice'
  pendingJoinRequestId.value = null
}
```

- [ ] **Step 5: Replace the modal template**

Replace the entire content inside `<div class="portal-modal__card">` (from the icon-ring to the actions div) with:

```html
<div class="portal-modal__icon-ring">
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#1a1a1a" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
    <rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>
  </svg>
</div>
<h3 class="portal-modal__title">Rejoindre « {{ joinModalGroup?.name }} »</h3>

<!-- Choice mode -->
<template v-if="joinModalMode === 'choice'">
  <div v-if="checkingPending" class="flex justify-center py-4">
    <div class="h-5 w-5 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
  </div>
  <template v-else>
    <button
      v-if="!pendingJoinRequestId"
      @click="requestToJoin"
      :disabled="requestingJoin"
      class="portal-modal__btn portal-modal__btn--primary w-full mb-3"
    >
      {{ requestingJoin ? 'Envoi...' : 'Demander à rejoindre' }}
    </button>
    <button
      v-else
      disabled
      class="portal-modal__btn portal-modal__btn--primary w-full mb-3 opacity-50 cursor-not-allowed"
    >
      Demande envoyée ✓
    </button>

    <div class="flex items-center gap-3 my-3">
      <div class="flex-1 h-px bg-gray-200"></div>
      <span class="text-xs text-gray-400">ou</span>
      <div class="flex-1 h-px bg-gray-200"></div>
    </div>

    <button
      @click="joinModalMode = 'code'"
      class="text-sm text-gray-500 hover:text-gray-800 transition cursor-pointer"
    >
      J'ai un code d'invitation
    </button>
  </template>
</template>

<!-- Code mode -->
<template v-if="joinModalMode === 'code'">
  <p class="portal-modal__text">Entrez le code d'invitation pour rejoindre ce groupe.</p>
  <input
    v-model="joinModalCode"
    type="text"
    class="portal-modal__input"
    placeholder="Code d'invitation"
    @keyup.enter="joinFromModal"
  />
  <div v-if="joinModalError" class="portal-modal__error">{{ joinModalError }}</div>
  <div class="portal-modal__actions">
    <button @click="joinModalMode = 'choice'" class="portal-modal__btn portal-modal__btn--cancel">Retour</button>
    <button @click="joinFromModal" :disabled="!joinModalCode.trim() || joiningFromModal" class="portal-modal__btn portal-modal__btn--primary">
      {{ joiningFromModal ? 'Rejoindre...' : 'Rejoindre' }}
    </button>
  </div>
</template>

<!-- Error in choice mode -->
<div v-if="joinModalMode === 'choice' && joinModalError" class="portal-modal__error mt-2">{{ joinModalError }}</div>

<!-- Close button -->
<button
  v-if="joinModalMode === 'choice'"
  @click="closeJoinModal"
  class="text-xs text-gray-400 hover:text-gray-600 mt-4 cursor-pointer"
>
  Annuler
</button>
```

- [ ] **Step 6: Verify frontend build**

```bash
cd /Users/alexandreroy/repos/expressiondansebeauport/src/Web/vue-app && npx vue-tsc --noEmit
```

- [ ] **Step 7: Test in browser**

Start the dev server and verify:
1. Go to "Tous les groupes" tab
2. Click a group you haven't joined
3. Modal shows "Demander à rejoindre" button + "J'ai un code d'invitation" link
4. Click "J'ai un code" — shows the existing code input
5. Go back, click "Demander à rejoindre" — sends request, shows success toast
6. Re-open the same group modal — button shows "Demande envoyée ✓" (disabled)
7. As a professor, check conversation — see the JoinRequestCard with Accept/Reject buttons
8. Click Accept — member added to group, card updates to "Accepté par..."
9. As the member, check messages — auto-reply visible

- [ ] **Step 8: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialPortal.vue
git commit -m "feat(social): update portal modal with join request + invite code options"
```

---

### Task 11: Register SignalR event for JoinRequestResolved

**Files:**
- Modify: `src/Web/vue-app/src/composables/useSignalR.ts`

- [ ] **Step 1: Add JoinRequestResolved event forwarding**

In `useSignalR.ts`, add a new callback set and event handler. After the `messageCallbacks` set declaration, add:

```typescript
const joinRequestCallbacks = new Set<(data: any) => void>()
```

Inside the `start()` function, after the `connection.value.on('ReceiveMessage', ...)` block, add:

```typescript
connection.value.on('JoinRequestResolved', (data: any) => {
  joinRequestCallbacks.forEach(cb => cb(data))
})
```

Add two new functions before the `return` statement:

```typescript
function onJoinRequestResolved(callback: (data: any) => void) {
  joinRequestCallbacks.add(callback)
}

function offJoinRequestResolved(callback: (data: any) => void) {
  joinRequestCallbacks.delete(callback)
}
```

Update the return statement to include the new functions:

```typescript
return { connection, isConnected, start, stop, onMessage, offMessage, onJoinRequestResolved, offJoinRequestResolved }
```

- [ ] **Step 2: Commit**

```bash
git add src/Web/vue-app/src/composables/useSignalR.ts
git commit -m "feat(social): add JoinRequestResolved SignalR event handler"
```

---

### Task 12: Final verification + cleanup

- [ ] **Step 1: Full backend build**

```bash
dotnet build /Users/alexandreroy/repos/expressiondansebeauport/src/Web
```

Expected: Build succeeded.

- [ ] **Step 2: Full frontend build**

```bash
cd /Users/alexandreroy/repos/expressiondansebeauport/src/Web/vue-app && npx vue-tsc --noEmit && npm run build
```

Expected: No type errors, build succeeds.

- [ ] **Step 3: Run existing tests**

```bash
cd /Users/alexandreroy/repos/expressiondansebeauport && dotnet test
```

Expected: All existing tests pass.

- [ ] **Step 4: Manual end-to-end test**

Start the app and test the full flow:
1. As a member, go to groups → click non-joined group → "Demander à rejoindre"
2. Verify request appears in professor's conversation
3. As professor, accept → verify member added to group
4. Verify auto-reply appears in member's conversation
5. Test reject flow
6. Test duplicate prevention (request already pending)
7. Test re-request after rejection
8. Test "J'ai un code" path still works

using Microsoft.EntityFrameworkCore;
using Datos.Models; 

namespace Datos.Data
{
    public class EventflowDbContext : DbContext
    {
        public EventflowDbContext(DbContextOptions<EventflowDbContext> options) : base(options)
        {
        }

        // 1. DEFINICIÓN DE TABLAS (DbSets)
        public DbSet<User> Users { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // Tablas de relaciones
        public DbSet<UserCommunity> UserCommunities { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }

        // Chat
        public DbSet<EventChatMessage> EventChatMessages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }


        // 2. CONFIGURACIÓN DE RELACIONES Y CLAVES (OnModelCreating)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CONFIGURACIÓN DE CLAVES COMPUESTAS ---

            // User - Community (Muchos a Muchos)
            modelBuilder.Entity<UserCommunity>()
                .HasKey(uc => new { uc.UserId, uc.CommunityId });

            modelBuilder.Entity<UserCommunity>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.Communities)
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserCommunity>()
                .HasOne(uc => uc.Community)
                .WithMany(c => c.Members)
                .HasForeignKey(uc => uc.CommunityId);

            // User - Event (Asistencia)
            modelBuilder.Entity<EventAttendee>()
                .HasKey(ea => new { ea.UserId, ea.EventId });

            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.User)
                .WithMany(u => u.EventAttendances)
                .HasForeignKey(ea => ea.UserId);

            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.Event)
                .WithMany(e => e.Attendees)
                .HasForeignKey(ea => ea.EventId);

            // Likes (User - Post)
            modelBuilder.Entity<PostLike>()
                .HasKey(pl => new { pl.UserId, pl.PostId });

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(pl => pl.UserId); 

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PostId);

            // Chat: Participantes de conversación
            modelBuilder.Entity<ConversationParticipant>()
                .HasKey(cp => new { cp.ConversationId, cp.UserId });


            // --- CONFIGURACIONES ADICIONALES (Delete Behavior) ---
            modelBuilder.Entity<Community>()
                .HasOne(c => c.Owner)
                .WithMany(u => u.OwnedCommunities)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrar al usuario si tiene comunidades

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post: Relación opcional con Community y Event
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Community)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CommunityId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade); // Si borro la comunidad, borro sus posts

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Event)
                .WithMany(e => e.Posts)
                .HasForeignKey(p => p.EventId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
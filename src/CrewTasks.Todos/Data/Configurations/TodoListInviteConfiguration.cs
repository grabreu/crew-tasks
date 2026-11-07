using CrewTasks.Todos.Entities;

namespace CrewTasks.Todos.Data.Configurations;

public class TodoListInviteConfiguration : IEntityTypeConfiguration<TodoListInvite>
{
    public void Configure(EntityTypeBuilder<TodoListInvite> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasOne(x => x.TodoList)
            .WithMany(x => x.Invites)
            .HasForeignKey(x => x.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => x.TodoListId);
    }
}

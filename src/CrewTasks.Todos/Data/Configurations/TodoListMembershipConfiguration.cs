using CrewTasks.Todos.Entities;

namespace CrewTasks.Todos.Data.Configurations;

public class TodoListMembershipConfiguration : IEntityTypeConfiguration<TodoListMembership>
{
    public void Configure(EntityTypeBuilder<TodoListMembership> builder)
    {
        builder.HasKey(x => new { x.TodoListId, x.UserId });

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(x => x.TodoList)
            .WithMany(x => x.Memberships)
            .HasForeignKey(x => x.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
    }
}

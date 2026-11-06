using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewTasks.Todos.Data.Migrations;

/// <inheritdoc />
public partial class AddTodoListSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Todos");

        migrationBuilder.CreateTable(
            name: "TodoLists",
            schema: "Todos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TodoLists", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TodoListMemberships",
            schema: "Todos",
            columns: table => new
            {
                TodoListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TodoListMemberships", x => new { x.TodoListId, x.UserId });
                table.ForeignKey(
                    name: "FK_TodoListMemberships_TodoLists_TodoListId",
                    column: x => x.TodoListId,
                    principalSchema: "Todos",
                    principalTable: "TodoLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TodoListMemberships_UserId",
            schema: "Todos",
            table: "TodoListMemberships",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TodoListMemberships",
            schema: "Todos");

        migrationBuilder.DropTable(
            name: "TodoLists",
            schema: "Todos");
    }
}

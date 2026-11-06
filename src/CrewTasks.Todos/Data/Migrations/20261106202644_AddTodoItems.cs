using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewTasks.Todos.Data.Migrations;

/// <inheritdoc />
public partial class AddTodoItems : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TodoItems",
            schema: "Todos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TodoListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TodoItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_TodoItems_TodoLists_TodoListId",
                    column: x => x.TodoListId,
                    principalSchema: "Todos",
                    principalTable: "TodoLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TodoItems_TodoListId",
            schema: "Todos",
            table: "TodoItems",
            column: "TodoListId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TodoItems",
            schema: "Todos");
    }
}

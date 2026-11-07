using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewTasks.Todos.Data.Migrations;

/// <inheritdoc />
public partial class AddTodoListInvites : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TodoListInvites",
            schema: "Todos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TodoListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TodoListInvites", x => x.Id);
                table.ForeignKey(
                    name: "FK_TodoListInvites_TodoLists_TodoListId",
                    column: x => x.TodoListId,
                    principalSchema: "Todos",
                    principalTable: "TodoLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TodoListInvites_TodoListId",
            schema: "Todos",
            table: "TodoListInvites",
            column: "TodoListId");

        migrationBuilder.CreateIndex(
            name: "IX_TodoListInvites_Token",
            schema: "Todos",
            table: "TodoListInvites",
            column: "Token",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TodoListInvites",
            schema: "Todos");
    }
}

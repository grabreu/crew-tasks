using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewTasks.Todos.Data.Migrations;

/// <inheritdoc />
public partial class AddTodoItemRowVersion : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            schema: "Todos",
            table: "TodoItems",
            type: "rowversion",
            rowVersion: true,
            nullable: false,
            defaultValue: Array.Empty<byte>());
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RowVersion",
            schema: "Todos",
            table: "TodoItems");
    }
}

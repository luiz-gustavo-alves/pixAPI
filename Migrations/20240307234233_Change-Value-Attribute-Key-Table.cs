using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pixAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeValueAttributeKeyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "PixKey",
                type: "text",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Value",
                table: "PixKey",
                type: "real",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}

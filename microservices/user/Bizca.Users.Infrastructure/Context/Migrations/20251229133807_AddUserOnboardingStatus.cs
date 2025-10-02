using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bizca.Users.Infrastructure.Context.Migrations;

public partial class AddUserOnboardingStatus : Migration
{
	private static readonly string[] columns = ["statusId", "description", "label"];

	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
			name: "statusId",
			schema: "usr",
			table: "user",
			type: "int",
			nullable: false,
			defaultValue: 0);

		migrationBuilder.AlterColumn<string>(
			name: "country",
			schema: "usr",
			table: "address",
			type: "varchar(100)",
			unicode: false,
			maxLength: 100,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "varchar(100)",
			oldUnicode: false,
			oldMaxLength: 100,
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "city",
			schema: "usr",
			table: "address",
			type: "varchar(100)",
			unicode: false,
			maxLength: 100,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "varchar(100)",
			oldUnicode: false,
			oldMaxLength: 100,
			oldNullable: true);

		migrationBuilder.CreateTable(
			name: "status",
			schema: "usr",
			columns: static table => new
			{
				statusId = table.Column<int>(type: "int", nullable: false),
				label = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
				description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
			},
			constraints: static table =>
			{
				table.PrimaryKey("pk_status_ref", static x => x.statusId);
			});

		migrationBuilder.InsertData(
			schema: "usr",
			table: "status",
			columns: columns,
			values: new object[,]
			{
					{ 1, "Draft", "Draft" },
					{ 2, "KycPending", "KycPending" },
					{ 4, "KycVerified", "KycVerified" },
					{ 8, "Active", "Active" }
			});

		migrationBuilder.CreateIndex(
			name: "ix_user_statusId",
			schema: "usr",
			table: "user",
			column: "statusId");

		migrationBuilder.AddForeignKey(
			name: "fk_user_statusId",
			schema: "usr",
			table: "user",
			column: "statusId",
			principalSchema: "usr",
			principalTable: "status",
			principalColumn: "statusId",
			onDelete: ReferentialAction.Cascade);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_user_statusId",
			schema: "usr",
			table: "user");

		migrationBuilder.DropTable(
			name: "status",
			schema: "usr");

		migrationBuilder.DropIndex(
			name: "ix_user_statusId",
			schema: "usr",
			table: "user");

		migrationBuilder.DropColumn(
			name: "statusId",
			schema: "usr",
			table: "user");

		migrationBuilder.AlterColumn<string>(
			name: "country",
			schema: "usr",
			table: "address",
			type: "varchar(100)",
			unicode: false,
			maxLength: 100,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "varchar(100)",
			oldUnicode: false,
			oldMaxLength: 100);

		migrationBuilder.AlterColumn<string>(
			name: "city",
			schema: "usr",
			table: "address",
			type: "varchar(100)",
			unicode: false,
			maxLength: 100,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "varchar(100)",
			oldUnicode: false,
			oldMaxLength: 100);
	}
}

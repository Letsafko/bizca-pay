using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bizca.Users.Infrastructure.Context.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
	private static readonly string[] channelColumns = ["channelTypeId", "description", "label"];
	private static readonly string[] civilityColumns = ["civilityId", "description", "label"];

	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.EnsureSchema(
			name: "usr");

		migrationBuilder.CreateTable(
			name: "channelType",
			schema: "usr",
			columns: static table => new
			{
				channelTypeId = table.Column<int>(type: "int", nullable: false),
				label = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
				description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
			},
			constraints: static table =>
			{
				table.PrimaryKey("pk_channelType_ref", static x => x.channelTypeId);
			});

		migrationBuilder.CreateTable(
			name: "civility",
			schema: "usr",
			columns: static table => new
			{
				civilityId = table.Column<int>(type: "int", nullable: false),
				label = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
				description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
			},
			constraints: static table =>
			{
				table.PrimaryKey("pk_civility_ref", static x => x.civilityId);
			});

		migrationBuilder.CreateTable(
			name: "user",
			schema: "usr",
			columns: static table => new
			{
				userId = table.Column<int>(type: "int", nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				externalUserId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 40, nullable: false),
				civilityId = table.Column<int>(type: "int", nullable: false),
				active = table.Column<bool>(type: "bit", nullable: false),
				firstName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
				lastName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
				birthDate = table.Column<DateOnly>(type: "date", nullable: true),
				birthCity = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
				birthCountry = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
				birthCountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
				securityStamp = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
				passwordHash = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
				version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
				createdOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getdate())"),
				lastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getdate())")
			},
			constraints: static table =>
			{
				table.PrimaryKey("pk_user", static x => x.userId);
				table.ForeignKey(
					name: "fk_user_civilityId",
					column: static x => x.civilityId,
					principalSchema: "usr",
					principalTable: "civility",
					principalColumn: "civilityId",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateTable(
			name: "address",
			schema: "usr",
			columns: static table => new
			{
				addressId = table.Column<int>(type: "int", nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				city = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
				zipcode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
				street = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
				country = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
				countryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
				userId = table.Column<int>(type: "int", nullable: false),
				createdOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getdate())"),
				lastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getdate())")
			},
			constraints: static table =>
			{
				table.PrimaryKey("pk_address", static x => x.addressId);
				table.ForeignKey(
					name: "fk_user_address",
					column: static x => x.userId,
					principalSchema: "usr",
					principalTable: "user",
					principalColumn: "userId");
			});

		migrationBuilder.CreateTable(
			name: "userChannel",
			schema: "usr",
			columns: static table => new
			{
				userChannelId = table.Column<int>(type: "int", nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				channelValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
				channelTypeId = table.Column<int>(type: "int", nullable: false),
				confirmed = table.Column<bool>(type: "bit", nullable: false),
				userId = table.Column<int>(type: "int", nullable: false),
				createdOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getdate())"),
				lastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getdate())")
			},
			constraints: static table =>
			{
				table.PrimaryKey("pk_userChannel", static x => x.userChannelId);
				table.ForeignKey(
					name: "fk_userChannel_channelTypeId",
					column: static x => x.channelTypeId,
					principalSchema: "usr",
					principalTable: "channelType",
					principalColumn: "channelTypeId",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_user_userChannel",
					column: static x => x.userId,
					principalSchema: "usr",
					principalTable: "user",
					principalColumn: "userId");
			});

		migrationBuilder.CreateTable(
			name: "userChannelConfirmation",
			schema: "usr",
			columns: static table => new
			{
				userChannelConfirmationId = table.Column<int>(type: "int", nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				confirmationCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
				expirationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
				userChannelId = table.Column<int>(type: "int", nullable: false),
				createdOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getdate())")
			},
			constraints: static table =>
			{
				table.PrimaryKey("pk_channelConfirmation", static x => x.userChannelConfirmationId);
				table.ForeignKey(
					name: "fk_userChannel_userChannelConfirmation",
					column: static x => x.userChannelId,
					principalSchema: "usr",
					principalTable: "userChannel",
					principalColumn: "userChannelId");
			});

		migrationBuilder.InsertData(
			schema: "usr",
			table: "channelType",
			columns: channelColumns,
			values: new object[,]
			{
					{ 1, "SMS", "SMS" },
					{ 2, "Whatsapp", "Whatsapp" },
					{ 3, "Email", "Email" }
			});

		migrationBuilder.InsertData(
			schema: "usr",
			table: "civility",
			columns: civilityColumns,
			values: new object[,]
			{
					{ 1, "Mr", "Mr" },
					{ 2, "Ms", "Ms" },
					{ 3, "Other", "Other" }
			});

		migrationBuilder.CreateIndex(
			name: "IX_address_userId",
			schema: "usr",
			table: "address",
			column: "userId",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "ix_user_civilityId",
			schema: "usr",
			table: "user",
			column: "civilityId");

		migrationBuilder.CreateIndex(
			name: "ix_userChannel_channelTypeId",
			schema: "usr",
			table: "userChannel",
			column: "channelTypeId");

		migrationBuilder.CreateIndex(
			name: "IX_userChannel_userId",
			schema: "usr",
			table: "userChannel",
			column: "userId");

		migrationBuilder.CreateIndex(
			name: "IX_userChannelConfirmation_userChannelId",
			schema: "usr",
			table: "userChannelConfirmation",
			column: "userChannelId");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "address",
			schema: "usr");

		migrationBuilder.DropTable(
			name: "userChannelConfirmation",
			schema: "usr");

		migrationBuilder.DropTable(
			name: "userChannel",
			schema: "usr");

		migrationBuilder.DropTable(
			name: "channelType",
			schema: "usr");

		migrationBuilder.DropTable(
			name: "user",
			schema: "usr");

		migrationBuilder.DropTable(
			name: "civility",
			schema: "usr");
	}
}

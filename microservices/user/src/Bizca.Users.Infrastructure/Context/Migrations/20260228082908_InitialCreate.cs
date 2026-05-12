using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bizca.Users.Infrastructure.Context.Migrations;

public partial class InitialCreate : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.EnsureSchema(name: "usr");

		migrationBuilder.CreateTable(
			name: "channelType",
			schema: "usr",
			columns: table => new
			{
				channelTypeId = table.Column<int>(type: "integer", nullable: false),
				label = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
				description = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_channelType_ref", x => x.channelTypeId);
			});

		migrationBuilder.CreateTable(
			name: "civility",
			schema: "usr",
			columns: table => new
			{
				civilityId = table.Column<int>(type: "integer", nullable: false),
				label = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
				description = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_civility_ref", x => x.civilityId);
			});

		migrationBuilder.CreateTable(
			name: "status",
			schema: "usr",
			columns: table => new
			{
				statusId = table.Column<int>(type: "integer", nullable: false),
				label = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
				description = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_status_ref", x => x.statusId);
			});

		migrationBuilder.CreateTable(
			name: "user",
			schema: "usr",
			columns: table => new
			{
				userId = table.Column<int>(type: "integer", nullable: false)
							.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				externalUserId = table.Column<Guid>(type: "uuid", maxLength: 40, nullable: false),
				civilityId = table.Column<int>(type: "integer", nullable: false),
				active = table.Column<bool>(type: "boolean", nullable: false),
				statusId = table.Column<int>(type: "integer", nullable: false),
				firstName = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
				lastName = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
				birthDate = table.Column<DateOnly>(type: "date", nullable: true),
				birthCity = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
				birthCountry = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
				birthCountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
				securityStamp = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
				passwordHash = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
				version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
				createdOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
				lastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_user", x => x.userId);
				table.ForeignKey(
					name: "fk_user_civilityId",
					column: x => x.civilityId,
					principalSchema: "usr",
					principalTable: "civility",
					principalColumn: "civilityId",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_user_statusId",
					column: x => x.statusId,
					principalSchema: "usr",
					principalTable: "status",
					principalColumn: "statusId",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateTable(
			name: "address",
			schema: "usr",
			columns: table => new
			{
				addressId = table.Column<int>(type: "integer", nullable: false)
								.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				city = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
				zipcode = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: true),
				street = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: true),
				country = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
				countryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
				user_id = table.Column<int>(type: "integer", nullable: false),
				createdOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
				lastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_address", x => x.addressId);
				table.ForeignKey(
					name: "fk_user_address",
					column: x => x.user_id,
					principalSchema: "usr",
					principalTable: "user",
					principalColumn: "userId");
			});

		migrationBuilder.CreateTable(
			name: "userChannel",
			schema: "usr",
			columns: table => new
			{
				userChannelId = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				channelValue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
				channelTypeId = table.Column<int>(type: "integer", nullable: false),
				confirmed = table.Column<bool>(type: "boolean", nullable: false),
				user_id = table.Column<int>(type: "integer", nullable: false),
				createdOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
				lastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_userChannel", x => x.userChannelId);
				table.ForeignKey(
					name: "fk_userChannel_channelTypeId",
					column: x => x.channelTypeId,
					principalSchema: "usr",
					principalTable: "channelType",
					principalColumn: "channelTypeId",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_user_userChannel",
					column: x => x.user_id,
					principalSchema: "usr",
					principalTable: "user",
					principalColumn: "userId");
			});

		migrationBuilder.CreateTable(
			name: "userChannelConfirmation",
			schema: "usr",
			columns: table => new
			{
				userChannelConfirmationId = table.Column<int>(type: "integer", nullable: false)
												.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				confirmationCode = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
				expirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
				user_channel_id = table.Column<int>(type: "integer", nullable: false),
				createdOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_channelConfirmation", x => x.userChannelConfirmationId);
				table.ForeignKey(
					name: "fk_userChannel_userChannelConfirmation",
					column: x => x.user_channel_id,
					principalSchema: "usr",
					principalTable: "userChannel",
					principalColumn: "userChannelId");
			});

		migrationBuilder.InsertData(
			schema: "usr",
			table: "channelType",
			columns: [ "channelTypeId", "description", "label" ],
			values: new object[,]
			{
				{ 0, "None", "None" },
				{ 1, "Sms", "Sms" },
				{ 2, "Whatsapp", "Whatsapp" },
				{ 3, "Email", "Email" }
			});

		migrationBuilder.InsertData(
			schema: "usr",
			table: "civility",
			columns: [ "civilityId", "description", "label" ],
			values: new object[,]
			{
				{ 0, "None", "None" },
				{ 1, "Mr", "Mr" },
				{ 2, "Ms", "Ms" },
				{ 3, "Other", "Other" }
			});

		migrationBuilder.InsertData(
			schema: "usr",
			table: "status",
			columns: ["statusId", "description", "label" ],
			values: new object[,]
			{
				{ 0, "None", "None" },
				{ 1, "Draft", "Draft" },
				{ 2, "KycPending", "KycPending" },
				{ 4, "KycVerified", "KycVerified" },
				{ 8, "Active", "Active" }
			});

		migrationBuilder.CreateIndex(
			name: "ix_address_user_id",
			schema: "usr",
			table: "address",
			column: "user_id",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "ix_user_civilityId",
			schema: "usr",
			table: "user",
			column: "civilityId");

		migrationBuilder.CreateIndex(
			name: "ix_user_statusId",
			schema: "usr",
			table: "user",
			column: "statusId");

		migrationBuilder.CreateIndex(
			name: "ix_user_channel_user_id",
			schema: "usr",
			table: "userChannel",
			column: "user_id");

		migrationBuilder.CreateIndex(
			name: "ix_userChannel_channelTypeId",
			schema: "usr",
			table: "userChannel",
			column: "channelTypeId");

		migrationBuilder.CreateIndex(
			name: "ix_user_channel_confirmation_user_channel_id",
			schema: "usr",
			table: "userChannelConfirmation",
			column: "user_channel_id");
	}

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

		migrationBuilder.DropTable(
			name: "status",
			schema: "usr");
	}
}
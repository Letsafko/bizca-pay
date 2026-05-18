using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bizca.Users.Infrastructure.Context.Migrations
{
    /// <inheritdoc />
    public partial class Address_City_Country_NotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_channel_id",
                schema: "usr",
                table: "userChannelConfirmation",
                newName: "userChannelId");

            migrationBuilder.RenameIndex(
                name: "ix_user_channel_confirmation_user_channel_id",
                schema: "usr",
                table: "userChannelConfirmation",
                newName: "iX_userChannelConfirmation_userChannelId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "usr",
                table: "userChannel",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "ix_user_channel_user_id",
                schema: "usr",
                table: "userChannel",
                newName: "iX_userChannel_userId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "usr",
                table: "address",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "ix_address_user_id",
                schema: "usr",
                table: "address",
                newName: "iX_address_userId");

            migrationBuilder.AlterColumn<int>(
                name: "userId",
                schema: "usr",
                table: "address",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userChannelId",
                schema: "usr",
                table: "userChannelConfirmation",
                newName: "user_channel_id");

            migrationBuilder.RenameIndex(
                name: "iX_userChannelConfirmation_userChannelId",
                schema: "usr",
                table: "userChannelConfirmation",
                newName: "ix_user_channel_confirmation_user_channel_id");

            migrationBuilder.RenameColumn(
                name: "userId",
                schema: "usr",
                table: "userChannel",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "iX_userChannel_userId",
                schema: "usr",
                table: "userChannel",
                newName: "ix_user_channel_user_id");

            migrationBuilder.RenameColumn(
                name: "userId",
                schema: "usr",
                table: "address",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "iX_address_userId",
                schema: "usr",
                table: "address",
                newName: "ix_address_user_id");

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                schema: "usr",
                table: "address",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}

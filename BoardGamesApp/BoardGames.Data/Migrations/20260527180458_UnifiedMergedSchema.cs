using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BoardGames.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedMergedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountRoles_Account_AccountId",
                table: "AccountRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_FailedLoginAttempt_Account_AccountId",
                table: "FailedLoginAttempt");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Account_owner_id",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Account_user_id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Account_OwnerId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Account_RenterId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Games_GameId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_OfferingUserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_OwnerId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Account_RenterId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Rentals",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Rentals",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "Rentals",
                newName: "game_id");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Rentals",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "rental_id",
                table: "Rentals",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RenterId",
                table: "Rentals",
                newName: "PaymentTransactionIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_RenterId",
                table: "Rentals",
                newName: "IX_Rentals_PaymentTransactionIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_OwnerId",
                table: "Rentals",
                newName: "IX_Rentals_owner_id");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_GameId",
                table: "Rentals",
                newName: "IX_Rentals_game_id");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "Games",
                newName: "id");

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "game_id",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "client_id",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "total_price",
                table: "Rentals",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    main_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    names = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    latitude = table.Column<double>(type: "float", nullable: false),
                    longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_account",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pam_user_id = table.Column<int>(type: "int", nullable: false),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    is_suspended = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    street_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    street_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_account", x => x.id);
                    table.UniqueConstraint("AK_user_account_pam_user_id", x => x.pam_user_id);
                });

            migrationBuilder.CreateTable(
                name: "conversation_participants",
                columns: table => new
                {
                    conversation_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    last_message_read_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    unread_messages_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_participants", x => new { x.conversation_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_conversation_participants_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conversation_participants_user_account_user_id",
                        column: x => x.user_id,
                        principalTable: "user_account",
                        principalColumn: "pam_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    paid_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_of_transaction = table.Column<DateTime>(type: "datetime2", nullable: true),
                    date_confirmed_buyer = table.Column<DateTime>(type: "datetime2", nullable: true),
                    date_confirmed_seller = table.Column<DateTime>(type: "datetime2", nullable: true),
                    payment_state = table.Column<int>(type: "int", nullable: false),
                    receipt_file_path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    client_id = table.Column<int>(type: "int", nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    PaymentCategory = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    game_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    owner_name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_Rentals_request_id",
                        column: x => x.request_id,
                        principalTable: "Rentals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_user_account_client_id",
                        column: x => x.client_id,
                        principalTable: "user_account",
                        principalColumn: "pam_user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_user_account_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user_account",
                        principalColumn: "pam_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    message_sent_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    message_content_as_string = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    conversation_id = table.Column<int>(type: "int", nullable: false),
                    message_sender_id = table.Column<int>(type: "int", nullable: false),
                    message_receiver_id = table.Column<int>(type: "int", nullable: false),
                    MessageCategory = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    cash_payment_id = table.Column<int>(type: "int", nullable: true),
                    is_cash_agreement_resolved = table.Column<bool>(type: "bit", nullable: true),
                    is_cash_agreement_accepted_by_buyer = table.Column<bool>(type: "bit", nullable: true),
                    is_cash_agreement_accepted_by_seller = table.Column<bool>(type: "bit", nullable: true),
                    message_image_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rental_request_id = table.Column<int>(type: "int", nullable: true),
                    is_request_resolved = table.Column<bool>(type: "bit", nullable: true),
                    is_request_accepted = table.Column<bool>(type: "bit", nullable: true),
                    request_content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    message_content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text_message_content = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_Rentals_rental_request_id",
                        column: x => x.rental_request_id,
                        principalTable: "Rentals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_payments_cash_payment_id",
                        column: x => x.cash_payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_user_account_message_receiver_id",
                        column: x => x.message_receiver_id,
                        principalTable: "user_account",
                        principalColumn: "pam_user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_user_account_message_sender_id",
                        column: x => x.message_sender_id,
                        principalTable: "user_account",
                        principalColumn: "pam_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "user_account",
                columns: new[] { "id", "avatar_url", "balance", "city", "country", "created_at", "display_name", "email", "is_suspended", "pam_user_id", "password_hash", "phone_number", "street_name", "street_number", "updated_at", "username" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000010"), "", 0m, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrator", "admin@boardrent.com", false, 4, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "", 0m, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Darius Turcu", "darius@boardrent.com", false, 1, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "darius" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "", 0m, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mihai Tira", "mihai@boardrent.com", false, 2, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mihai" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_client_id",
                table: "Rentals",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_participants_user_id",
                table: "conversation_participants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_cash_payment_id",
                table: "messages",
                column: "cash_payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_conversation_id",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_message_receiver_id",
                table: "messages",
                column: "message_receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_message_sender_id",
                table: "messages",
                column: "message_sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_rental_request_id",
                table: "messages",
                column: "rental_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_client_id",
                table: "payments",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_owner_id",
                table: "payments",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_request_id",
                table: "payments",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_account_email",
                table: "user_account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_account_username",
                table: "user_account",
                column: "username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRoles_user_account_AccountId",
                table: "AccountRoles",
                column: "AccountId",
                principalTable: "user_account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FailedLoginAttempt_user_account_AccountId",
                table: "FailedLoginAttempt",
                column: "AccountId",
                principalTable: "user_account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_user_account_owner_id",
                table: "Games",
                column: "owner_id",
                principalTable: "user_account",
                principalColumn: "pam_user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_user_account_user_id",
                table: "Notifications",
                column: "user_id",
                principalTable: "user_account",
                principalColumn: "pam_user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Games_game_id",
                table: "Rentals",
                column: "game_id",
                principalTable: "Games",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_payments_PaymentTransactionIdentifier",
                table: "Rentals",
                column: "PaymentTransactionIdentifier",
                principalTable: "payments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_user_account_client_id",
                table: "Rentals",
                column: "client_id",
                principalTable: "user_account",
                principalColumn: "pam_user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_user_account_owner_id",
                table: "Rentals",
                column: "owner_id",
                principalTable: "user_account",
                principalColumn: "pam_user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_user_account_OfferingUserId",
                table: "Requests",
                column: "OfferingUserId",
                principalTable: "user_account",
                principalColumn: "pam_user_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_user_account_OwnerId",
                table: "Requests",
                column: "OwnerId",
                principalTable: "user_account",
                principalColumn: "pam_user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_user_account_RenterId",
                table: "Requests",
                column: "RenterId",
                principalTable: "user_account",
                principalColumn: "pam_user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountRoles_user_account_AccountId",
                table: "AccountRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_FailedLoginAttempt_user_account_AccountId",
                table: "FailedLoginAttempt");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_user_account_owner_id",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_user_account_user_id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Games_game_id",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_payments_PaymentTransactionIdentifier",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_user_account_client_id",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_user_account_owner_id",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_user_account_OfferingUserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_user_account_OwnerId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_user_account_RenterId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropTable(
                name: "conversation_participants");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "user_account");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_client_id",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "total_price",
                table: "Rentals");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "Rentals",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "Rentals",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "Rentals",
                newName: "GameId");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "Rentals",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Rentals",
                newName: "rental_id");

            migrationBuilder.RenameColumn(
                name: "PaymentTransactionIdentifier",
                table: "Rentals",
                newName: "RenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_PaymentTransactionIdentifier",
                table: "Rentals",
                newName: "IX_Rentals_RenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_owner_id",
                table: "Rentals",
                newName: "IX_Rentals_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_game_id",
                table: "Rentals",
                newName: "IX_Rentals_GameId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Games",
                newName: "game_id");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Rentals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "Rentals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    PamUserId = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StreetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StreetNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.Id);
                    table.UniqueConstraint("AK_Account_PamUserId", x => x.PamUserId);
                });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "AvatarUrl", "City", "Country", "CreatedAt", "DisplayName", "Email", "IsSuspended", "PamUserId", "PasswordHash", "PhoneNumber", "StreetName", "StreetNumber", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000010"), "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrator", "admin@boardrent.com", false, 4, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Darius Turcu", "darius@boardrent.com", false, 1, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "darius" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mihai Tira", "mihai@boardrent.com", false, 2, "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=", "", "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mihai" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Email",
                table: "Account",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_Username",
                table: "Account",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRoles_Account_AccountId",
                table: "AccountRoles",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FailedLoginAttempt_Account_AccountId",
                table: "FailedLoginAttempt",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Account_owner_id",
                table: "Games",
                column: "owner_id",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Account_user_id",
                table: "Notifications",
                column: "user_id",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Account_OwnerId",
                table: "Rentals",
                column: "OwnerId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Account_RenterId",
                table: "Rentals",
                column: "RenterId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Games_GameId",
                table: "Rentals",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "game_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_OfferingUserId",
                table: "Requests",
                column: "OfferingUserId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_OwnerId",
                table: "Requests",
                column: "OwnerId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Account_RenterId",
                table: "Requests",
                column: "RenterId",
                principalTable: "Account",
                principalColumn: "PamUserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisasterApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisasterType",
                columns: table => new
                {
                    disaster_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    category = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Disaster__731C64A80F5A5268", x => x.disaster_type_id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__760965CCCEA47220", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    auth_provider = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    auth_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    photo_url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    is_blacklisted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__B9BE370FF42BE9EA", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    chat_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    sender_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    receiver_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    is_read = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Chat__FD040B17E1346961", x => x.chat_id);
                    table.ForeignKey(
                        name: "FK_Chat_Receiver",
                        column: x => x.receiver_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Chat_Sender",
                        column: x => x.sender_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "DisasterReport",
                columns: table => new
                {
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    severity = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "Pending"),
                    verified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    disaster_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Disaster__779B7C58C8BC03EE", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_DisasterReport_DisasterType",
                        column: x => x.disaster_type_id,
                        principalTable: "DisasterType",
                        principalColumn: "disaster_type_id");
                    table.ForeignKey(
                        name: "FK_DisasterReport_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_DisasterReport_VerifiedBy",
                        column: x => x.verified_by,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    notification_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    related_entity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__E059842F4A100656", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_Notification_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    refresh_token_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RefreshT__B0A1F7C71F7A268A", x => x.refresh_token_id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_UserRole_Role",
                        column: x => x.role_id,
                        principalTable: "Role",
                        principalColumn: "role_id");
                    table.ForeignKey(
                        name: "FK_UserRole_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "ImpactDetail",
                columns: table => new
                {
                    impact_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    impact_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ImpactDe__BBC672B339BEF2C0", x => x.impact_id);
                    table.ForeignKey(
                        name: "FK_ImpactDetail_Report",
                        column: x => x.report_id,
                        principalTable: "DisasterReport",
                        principalColumn: "report_id");
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    location_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    google_place_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Location__771831EA31F9FED4", x => x.location_id);
                    table.ForeignKey(
                        name: "FK_Location_Report",
                        column: x => x.report_id,
                        principalTable: "DisasterReport",
                        principalColumn: "report_id");
                });

            migrationBuilder.CreateTable(
                name: "Photo",
                columns: table => new
                {
                    photo_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Photo__CB48C83D9C040954", x => x.photo_id);
                    table.ForeignKey(
                        name: "FK_Photo_Report",
                        column: x => x.report_id,
                        principalTable: "DisasterReport",
                        principalColumn: "report_id");
                });

            migrationBuilder.CreateTable(
                name: "SupportRequest",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    support_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantity_needed = table.Column<int>(type: "int", nullable: true),
                    urgency = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SupportR__18D3B90F6E2CEB46", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_SupportRequest_Report",
                        column: x => x.report_id,
                        principalTable: "DisasterReport",
                        principalColumn: "report_id");
                    table.ForeignKey(
                        name: "FK_SupportRequest_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "AssistanceProvided",
                columns: table => new
                {
                    assistance_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    request_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provided_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "Pending"),
                    provider_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Assistan__13BD40CA36EA92BA", x => x.assistance_id);
                    table.ForeignKey(
                        name: "FK_AssistanceProvided_Provider",
                        column: x => x.provider_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_AssistanceProvided_Request",
                        column: x => x.request_id,
                        principalTable: "SupportRequest",
                        principalColumn: "request_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceProvided_provider_id",
                table: "AssistanceProvided",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceProvided_Request",
                table: "AssistanceProvided",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReceiverUnread",
                table: "Chat",
                columns: new[] { "receiver_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_sender_id",
                table: "Chat",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterReport_disaster_type_id",
                table: "DisasterReport",
                column: "disaster_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterReport_User",
                table: "DisasterReport",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterReport_verified_by",
                table: "DisasterReport",
                column: "verified_by");

            migrationBuilder.CreateIndex(
                name: "UQ__Disaster__72E12F1B58BC6E7D",
                table: "DisasterType",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImpactDetail_report_id",
                table: "ImpactDetail",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Location__779B7C5926D8CAE2",
                table: "Location",
                column: "report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_User",
                table: "Notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Photo_report_id",
                table: "Photo",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_user_id",
                table: "RefreshToken",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__RefreshT__CA90DA7A24B1CB16",
                table: "RefreshToken",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Role__72E12F1B6DD5B5D7",
                table: "Role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_Report",
                table: "SupportRequest",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_user_id",
                table: "SupportRequest",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__User__AB6E61647E5028D0",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_User_AuthProviderId",
                table: "User",
                columns: new[] { "auth_provider", "auth_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_role_id",
                table: "UserRole",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssistanceProvided");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "ImpactDetail");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Photo");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "SupportRequest");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "DisasterReport");

            migrationBuilder.DropTable(
                name: "DisasterType");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}

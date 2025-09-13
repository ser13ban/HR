using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HrAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Department = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Position = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HireDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProfilePictureUrl = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "NOW(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "NOW(6) ON UPDATE NOW(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AbsenceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "NOW(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "NOW(6) ON UPDATE NOW(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceRequests_Employees_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AbsenceRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromEmployeeId = table.Column<int>(type: "int", nullable: false),
                    ToEmployeeId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PolishedContent = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPolished = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "NOW(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "NOW(6) ON UPDATE NOW(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.CheckConstraint("CK_Feedback_SelfReference", "FromEmployeeId != ToEmployeeId");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Employees_FromEmployeeId",
                        column: x => x.FromEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Employees_ToEmployeeId",
                        column: x => x.ToEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Bio", "CreatedAt", "Department", "Email", "FirstName", "HireDate", "LastName", "PhoneNumber", "Position", "ProfilePictureUrl", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Experienced software developer with expertise in .NET and Angular.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "john.doe@company.com", "John", new DateTime(2020, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Doe", "+1234567890", "Senior Developer", null, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Passionate about clean code and user experience.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "jane.smith@company.com", "Jane", new DateTime(2021, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Smith", "+1234567891", "Developer", null, 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Dedicated to creating a positive work environment.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HR", "mike.johnson@company.com", "Mike", new DateTime(2019, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Johnson", "+1234567892", "HR Manager", null, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "AbsenceRequests",
                columns: new[] { "Id", "ApprovalNotes", "ApprovedAt", "ApprovedById", "CreatedAt", "EmployeeId", "EndDate", "Reason", "StartDate", "Status", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2024, 12, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family vacation", new DateTime(2024, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2024, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Doctor appointment", new DateTime(2024, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "Id", "Content", "CreatedAt", "FromEmployeeId", "IsAnonymous", "IsPolished", "PolishedContent", "Rating", "ToEmployeeId", "Type", "UpdatedAt" },
                values: new object[] { 1, "Jane is an excellent team player with great communication skills.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, false, false, null, 9, 2, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRequests_ApprovedById",
                table: "AbsenceRequests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRequests_EmployeeId",
                table: "AbsenceRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_FromEmployeeId",
                table: "Feedbacks",
                column: "FromEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ToEmployeeId",
                table: "Feedbacks",
                column: "ToEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsenceRequests");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trabahub.Migrations
{
    /// <inheritdoc />
    public partial class InititalSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, collation: "SQL_Latin1_General_CP1_CS_AS"),
                    Username = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "Listing",
                columns: table => new
                {
                    ESTABNAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ESTABDESC = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ESTABADD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ESTABPRICE = table.Column<int>(type: "int", nullable: false),
                    STARTTIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ENDTIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ESTABIMAGEPATH = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listing", x => x.ESTABNAME);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "Listing");
        }
    }
}

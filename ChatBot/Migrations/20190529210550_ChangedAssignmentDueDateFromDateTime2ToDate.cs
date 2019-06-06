using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatBot.Migrations
{
    public partial class ChangedAssignmentDueDateFromDateTime2ToDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Assignments",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Assignments",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");
        }
    }
}

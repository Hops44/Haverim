using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Haverim.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    PublishDate = table.Column<DateTime>(nullable: false),
                    PublisherId = table.Column<string>(nullable: true),
                    _comments = table.Column<string>(nullable: true),
                    _upvoteUsers = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Username = table.Column<string>(nullable: false),
                    BirthDate = table.Column<DateTime>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    IsMale = table.Column<bool>(nullable: false),
                    JoinDate = table.Column<DateTime>(nullable: false),
                    Password = table.Column<string>(nullable: true),
                    ProfilePagePicture = table.Column<string>(nullable: true),
                    ProfilePic = table.Column<string>(nullable: true),
                    _activityFeed = table.Column<string>(nullable: true),
                    _followers = table.Column<string>(nullable: true),
                    _following = table.Column<string>(nullable: true),
                    _notifications = table.Column<string>(nullable: true),
                    _postFeed = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Username);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

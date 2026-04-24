using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddComprehensiveBusFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusType",
                table: "Buses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CancellationPolicy",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChildPolicy",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasBlankets",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasCCTV",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasChargingPoint",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasEmergencyExit",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasGPS",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasReadingLight",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasToilet",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWaterBottle",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWiFi",
                table: "Buses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LiquorPolicy",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LuggagePolicy",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OnTimeTrips",
                table: "Buses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PetPolicy",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Buses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ReschedulePolicy",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToiletPolicy",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalRatings",
                table: "Buses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalTrips",
                table: "Buses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BoardingPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    PointName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Landmark = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardingPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardingPoints_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardingPoints_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StaffBehavior = table.Column<int>(type: "integer", nullable: false),
                    Punctuality = table.Column<int>(type: "integer", nullable: false),
                    Cleanliness = table.Column<int>(type: "integer", nullable: false),
                    SeatComfort = table.Column<int>(type: "integer", nullable: false),
                    Driving = table.Column<int>(type: "integer", nullable: false),
                    AC = table.Column<int>(type: "integer", nullable: false),
                    LiveTracking = table.Column<int>(type: "integer", nullable: false),
                    RestStopHygiene = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusReviews_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DroppingPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    PointName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Landmark = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DroppingPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DroppingPoints_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DroppingPoints_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    StopName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DepartureTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Facilities = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestStops_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RestStops_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardingPoints_LocationId",
                table: "BoardingPoints",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardingPoints_ScheduleId",
                table: "BoardingPoints",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_BusReviews_BusId",
                table: "BusReviews",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_BusReviews_UserId",
                table: "BusReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DroppingPoints_LocationId",
                table: "DroppingPoints",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DroppingPoints_ScheduleId",
                table: "DroppingPoints",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_RestStops_LocationId",
                table: "RestStops",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RestStops_ScheduleId",
                table: "RestStops",
                column: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardingPoints");

            migrationBuilder.DropTable(
                name: "BusReviews");

            migrationBuilder.DropTable(
                name: "DroppingPoints");

            migrationBuilder.DropTable(
                name: "RestStops");

            migrationBuilder.DropColumn(
                name: "BusType",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "CancellationPolicy",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "ChildPolicy",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasBlankets",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasCCTV",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasChargingPoint",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasEmergencyExit",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasGPS",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasReadingLight",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasToilet",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasWaterBottle",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "HasWiFi",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "LiquorPolicy",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "LuggagePolicy",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "OnTimeTrips",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "PetPolicy",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "ReschedulePolicy",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "ToiletPolicy",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "TotalRatings",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "TotalTrips",
                table: "Buses");
        }
    }
}

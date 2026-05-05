using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTestCoupons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""Coupons"" (""OperatorId"", ""Code"", ""DiscountPercent"", ""DiscountAmount"", ""ValidFrom"", ""ValidTo"", ""IsActive"")
                VALUES 
                    (1, 'SAVE20', 20, 0, CURRENT_TIMESTAMP - INTERVAL '1 day', CURRENT_TIMESTAMP + INTERVAL '30 days', true),
                    (1, 'SAVE10', 10, 0, CURRENT_TIMESTAMP - INTERVAL '1 day', CURRENT_TIMESTAMP + INTERVAL '30 days', true),
                    (1, 'SUMMER', 15, 0, CURRENT_TIMESTAMP - INTERVAL '1 day', CURRENT_TIMESTAMP + INTERVAL '30 days', true)
                ON CONFLICT DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

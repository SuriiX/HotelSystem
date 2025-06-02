namespace HotelAuroraDreams.Api_Framework.Migrations.IdentityMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialIdentitySchema : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "Salario", c => c.Decimal(nullable: false, precision: 10, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "Salario", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}

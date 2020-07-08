namespace shop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class next : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblCategories", "Sorting", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblCategories", "Sorting", c => c.String());
        }
    }
}

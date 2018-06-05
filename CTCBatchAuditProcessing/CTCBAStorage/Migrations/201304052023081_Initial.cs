namespace CTCBAStorage.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StudentAudits",
                c => new
                    {
                        RecordId = c.Int(nullable: false, identity: true),
                        StudentId = c.String(maxLength: 20),
                        AuditTimestamp = c.DateTime(),
                        ProgramCode = c.String(maxLength: 10),
                        CreditsRequired = c.Decimal(precision: 18, scale: 2),
                        CreditsApplied = c.Decimal(precision: 18, scale: 2),
                        DegreeTitle = c.String(maxLength: 200),
                        AuditText = c.String(),
                    })
                .PrimaryKey(t => t.RecordId);
            
            CreateTable(
                "dbo.UnusedCourses",
                c => new
                    {
                        RecordId = c.Int(nullable: false, identity: true),
                        CourseStatus = c.String(maxLength: 1),
                        CourseId = c.String(maxLength: 10),
                        CourseTitle = c.String(maxLength: 200),
                        EnrolledCredits = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LetterGrade = c.String(maxLength: 2),
                        QuarterLabel = c.String(maxLength: 8),
                        CTCYrq = c.String(maxLength: 4),
                        StudentAudit_RecordId = c.Int(),
                    })
                .PrimaryKey(t => t.RecordId)
                .ForeignKey("dbo.StudentAudits", t => t.StudentAudit_RecordId)
                .Index(t => t.StudentAudit_RecordId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UnusedCourses", new[] { "StudentAudit_RecordId" });
            DropForeignKey("dbo.UnusedCourses", "StudentAudit_RecordId", "dbo.StudentAudits");
            DropTable("dbo.UnusedCourses");
            DropTable("dbo.StudentAudits");
        }
    }
}

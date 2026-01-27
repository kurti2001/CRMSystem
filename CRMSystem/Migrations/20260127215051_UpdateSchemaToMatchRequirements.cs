using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CRMSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaToMatchRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_AspNetUsers_AssignedToId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_ContactStatuses_ContactStatusId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_AspNetUsers_AuthorId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Contacts_ContactId",
                table: "Notes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notes",
                table: "Notes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactStatuses",
                table: "ContactStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contacts",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notes");

            migrationBuilder.RenameTable(
                name: "Notes",
                newName: "notes");

            migrationBuilder.RenameTable(
                name: "ContactStatuses",
                newName: "contact_status");

            migrationBuilder.RenameTable(
                name: "Contacts",
                newName: "contact");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "notes",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "notes",
                newName: "Contact");

            migrationBuilder.RenameColumn(
                name: "NoteId",
                table: "notes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "notes",
                newName: "Is_New_Todo");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "notes",
                newName: "Todo_Due_Date");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "notes",
                newName: "Sales_Rep");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_ContactId",
                table: "notes",
                newName: "IX_notes_Contact");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_AuthorId",
                table: "notes",
                newName: "IX_notes_Sales_Rep");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "Name_Last");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "AspNetUsers",
                newName: "Name_First");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "contact_status",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "ContactStatusId",
                table: "contact_status",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_ContactStatuses_Name",
                table: "contact_status",
                newName: "IX_contact_status_status");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "contact",
                newName: "Contact_Last");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "contact",
                newName: "Contact_First");

            migrationBuilder.RenameColumn(
                name: "ContactStatusId",
                table: "contact",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "contact",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "AssignedToId",
                table: "contact",
                newName: "Sales_Rep");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_Email",
                table: "contact",
                newName: "IX_contact_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_ContactStatusId",
                table: "contact",
                newName: "IX_contact_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_AssignedToId",
                table: "contact",
                newName: "IX_contact_Sales_Rep");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "notes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "Task_Status",
                table: "notes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Task_Update",
                table: "notes",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Todo_Desc_ID",
                table: "notes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Todo_Type_ID",
                table: "notes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_Middle",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_Title",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "User_Roles",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "User_Status",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "contact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "contact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_State",
                table: "contact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street_1",
                table: "contact",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street_2",
                table: "contact",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Zip",
                table: "contact",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Background_Info",
                table: "contact",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "contact",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact_Middle",
                table: "contact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact_Title",
                table: "contact",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date_of_Initial_Contact",
                table: "contact",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Deliverables",
                table: "contact",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "contact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lead_Referral_Source",
                table: "contact",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedIn_Profile",
                table: "contact",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Project_Description",
                table: "contact",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Project_Type",
                table: "contact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Proposal_Due_Date",
                table: "contact",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "contact",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "contact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "contact",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_notes",
                table: "notes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_contact_status",
                table: "contact_status",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_contact",
                table: "contact",
                column: "id");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "task_status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_status", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "todo_desc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todo_desc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "todo_type",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todo_type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_status", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "contact_status",
                keyColumn: "id",
                keyValue: 1,
                column: "status",
                value: "lead");

            migrationBuilder.UpdateData(
                table: "contact_status",
                keyColumn: "id",
                keyValue: 2,
                column: "status",
                value: "proposal");

            migrationBuilder.UpdateData(
                table: "contact_status",
                keyColumn: "id",
                keyValue: 3,
                column: "status",
                value: "customer/won");

            migrationBuilder.InsertData(
                table: "contact_status",
                columns: new[] { "id", "status" },
                values: new object[] { 4, "archive" });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "RoleName" },
                values: new object[,]
                {
                    { 1, "Sales Rep" },
                    { 2, "Manager" }
                });

            migrationBuilder.InsertData(
                table: "task_status",
                columns: new[] { "Id", "Status" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Completed" }
                });

            migrationBuilder.InsertData(
                table: "todo_desc",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 1, "Follow Up Email" },
                    { 2, "Phone Call" },
                    { 3, "Conference" },
                    { 4, "Meetup" },
                    { 5, "Tech Demo" }
                });

            migrationBuilder.InsertData(
                table: "todo_type",
                columns: new[] { "Id", "Type" },
                values: new object[,]
                {
                    { 1, "Task" },
                    { 2, "Meeting" }
                });

            migrationBuilder.InsertData(
                table: "user_status",
                columns: new[] { "Id", "Status" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Inactive" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_notes_Task_Status",
                table: "notes",
                column: "Task_Status");

            migrationBuilder.CreateIndex(
                name: "IX_notes_Todo_Desc_ID",
                table: "notes",
                column: "Todo_Desc_ID");

            migrationBuilder.CreateIndex(
                name: "IX_notes_Todo_Type_ID",
                table: "notes",
                column: "Todo_Type_ID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_User_Status",
                table: "AspNetUsers",
                column: "User_Status");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_user_status_User_Status",
                table: "AspNetUsers",
                column: "User_Status",
                principalTable: "user_status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_contact_AspNetUsers_Sales_Rep",
                table: "contact",
                column: "Sales_Rep",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_contact_contact_status_Status",
                table: "contact",
                column: "Status",
                principalTable: "contact_status",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notes_AspNetUsers_Sales_Rep",
                table: "notes",
                column: "Sales_Rep",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notes_contact_Contact",
                table: "notes",
                column: "Contact",
                principalTable: "contact",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notes_task_status_Task_Status",
                table: "notes",
                column: "Task_Status",
                principalTable: "task_status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notes_todo_desc_Todo_Desc_ID",
                table: "notes",
                column: "Todo_Desc_ID",
                principalTable: "todo_desc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notes_todo_type_Todo_Type_ID",
                table: "notes",
                column: "Todo_Type_ID",
                principalTable: "todo_type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_user_status_User_Status",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_contact_AspNetUsers_Sales_Rep",
                table: "contact");

            migrationBuilder.DropForeignKey(
                name: "FK_contact_contact_status_Status",
                table: "contact");

            migrationBuilder.DropForeignKey(
                name: "FK_notes_AspNetUsers_Sales_Rep",
                table: "notes");

            migrationBuilder.DropForeignKey(
                name: "FK_notes_contact_Contact",
                table: "notes");

            migrationBuilder.DropForeignKey(
                name: "FK_notes_task_status_Task_Status",
                table: "notes");

            migrationBuilder.DropForeignKey(
                name: "FK_notes_todo_desc_Todo_Desc_ID",
                table: "notes");

            migrationBuilder.DropForeignKey(
                name: "FK_notes_todo_type_Todo_Type_ID",
                table: "notes");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "task_status");

            migrationBuilder.DropTable(
                name: "todo_desc");

            migrationBuilder.DropTable(
                name: "todo_type");

            migrationBuilder.DropTable(
                name: "user_status");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notes",
                table: "notes");

            migrationBuilder.DropIndex(
                name: "IX_notes_Task_Status",
                table: "notes");

            migrationBuilder.DropIndex(
                name: "IX_notes_Todo_Desc_ID",
                table: "notes");

            migrationBuilder.DropIndex(
                name: "IX_notes_Todo_Type_ID",
                table: "notes");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_User_Status",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contact_status",
                table: "contact_status");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contact",
                table: "contact");

            migrationBuilder.DeleteData(
                table: "contact_status",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "Date",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "Task_Status",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "Task_Update",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "Todo_Desc_ID",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "Todo_Type_ID",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "Name_Middle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Name_Title",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "User_Roles",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "User_Status",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_State",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Street_1",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Street_2",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Zip",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Background_Info",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Contact_Middle",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Contact_Title",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Date_of_Initial_Contact",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Deliverables",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Lead_Referral_Source",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "LinkedIn_Profile",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Project_Description",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Project_Type",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Proposal_Due_Date",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "contact");

            migrationBuilder.RenameTable(
                name: "notes",
                newName: "Notes");

            migrationBuilder.RenameTable(
                name: "contact_status",
                newName: "ContactStatuses");

            migrationBuilder.RenameTable(
                name: "contact",
                newName: "Contacts");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Notes",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "Contact",
                table: "Notes",
                newName: "ContactId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Notes",
                newName: "NoteId");

            migrationBuilder.RenameColumn(
                name: "Todo_Due_Date",
                table: "Notes",
                newName: "DueDate");

            migrationBuilder.RenameColumn(
                name: "Sales_Rep",
                table: "Notes",
                newName: "AuthorId");

            migrationBuilder.RenameColumn(
                name: "Is_New_Todo",
                table: "Notes",
                newName: "IsCompleted");

            migrationBuilder.RenameIndex(
                name: "IX_notes_Sales_Rep",
                table: "Notes",
                newName: "IX_Notes_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_notes_Contact",
                table: "Notes",
                newName: "IX_Notes_ContactId");

            migrationBuilder.RenameColumn(
                name: "Name_Last",
                table: "AspNetUsers",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Name_First",
                table: "AspNetUsers",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "ContactStatuses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ContactStatuses",
                newName: "ContactStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_contact_status_status",
                table: "ContactStatuses",
                newName: "IX_ContactStatuses_Name");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Contacts",
                newName: "ContactStatusId");

            migrationBuilder.RenameColumn(
                name: "Contact_Last",
                table: "Contacts",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Contact_First",
                table: "Contacts",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Contacts",
                newName: "ContactId");

            migrationBuilder.RenameColumn(
                name: "Sales_Rep",
                table: "Contacts",
                newName: "AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_contact_Status",
                table: "Contacts",
                newName: "IX_Contacts_ContactStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_contact_Sales_Rep",
                table: "Contacts",
                newName: "IX_Contacts_AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_contact_Email",
                table: "Contacts",
                newName: "IX_Contacts_Email");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Notes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notes",
                table: "Notes",
                column: "NoteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactStatuses",
                table: "ContactStatuses",
                column: "ContactStatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contacts",
                table: "Contacts",
                column: "ContactId");

            migrationBuilder.UpdateData(
                table: "ContactStatuses",
                keyColumn: "ContactStatusId",
                keyValue: 1,
                column: "Name",
                value: "Lead");

            migrationBuilder.UpdateData(
                table: "ContactStatuses",
                keyColumn: "ContactStatusId",
                keyValue: 2,
                column: "Name",
                value: "Opportunity");

            migrationBuilder.UpdateData(
                table: "ContactStatuses",
                keyColumn: "ContactStatusId",
                keyValue: 3,
                column: "Name",
                value: "Customer");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_AspNetUsers_AssignedToId",
                table: "Contacts",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_ContactStatuses_ContactStatusId",
                table: "Contacts",
                column: "ContactStatusId",
                principalTable: "ContactStatuses",
                principalColumn: "ContactStatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_AspNetUsers_AuthorId",
                table: "Notes",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Contacts_ContactId",
                table: "Notes",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "ContactId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

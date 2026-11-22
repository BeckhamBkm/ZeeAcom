using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZeeAcom.Migrations
{
    /// <summary>
    /// Renames the "Entity" table to "Property" (and Picture.EntityId to PropertyId) in place,
    /// keeping existing rows. Hand-edited: EF scaffolded a drop/create, which would lose data.
    /// </summary>
    public partial class RenameEntityToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Picture_Entity_EntityId", table: "Picture");
            migrationBuilder.DropForeignKey(name: "FK_Entity_Owner_OwnerId", table: "Entity");
            migrationBuilder.DropPrimaryKey(name: "PK_Entity", table: "Entity");

            migrationBuilder.RenameTable(name: "Entity", newName: "Property");
            migrationBuilder.RenameIndex(name: "IX_Entity_OwnerId", table: "Property", newName: "IX_Property_OwnerId");
            migrationBuilder.AddPrimaryKey(name: "PK_Property", table: "Property", column: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_Property_Owner_OwnerId",
                table: "Property",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.RenameColumn(name: "EntityId", table: "Picture", newName: "PropertyId");
            migrationBuilder.RenameIndex(name: "IX_Picture_EntityId", table: "Picture", newName: "IX_Picture_PropertyId");
            migrationBuilder.AddForeignKey(
                name: "FK_Picture_Property_PropertyId",
                table: "Picture",
                column: "PropertyId",
                principalTable: "Property",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Picture_Property_PropertyId", table: "Picture");
            migrationBuilder.DropForeignKey(name: "FK_Property_Owner_OwnerId", table: "Property");
            migrationBuilder.DropPrimaryKey(name: "PK_Property", table: "Property");

            migrationBuilder.RenameTable(name: "Property", newName: "Entity");
            migrationBuilder.RenameIndex(name: "IX_Property_OwnerId", table: "Entity", newName: "IX_Entity_OwnerId");
            migrationBuilder.AddPrimaryKey(name: "PK_Entity", table: "Entity", column: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_Entity_Owner_OwnerId",
                table: "Entity",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.RenameColumn(name: "PropertyId", table: "Picture", newName: "EntityId");
            migrationBuilder.RenameIndex(name: "IX_Picture_PropertyId", table: "Picture", newName: "IX_Picture_EntityId");
            migrationBuilder.AddForeignKey(
                name: "FK_Picture_Entity_EntityId",
                table: "Picture",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

namespace ZeeAcom.Common.Models.OwnerModels
{
    public class CreateOwnerModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Collection of owner's entities
        /// </summary>
        public ICollection<Entity?>? Entities { get; set; } = new List<Entity?>();
    }
}

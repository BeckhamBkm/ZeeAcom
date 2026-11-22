using System.ComponentModel.DataAnnotations;

namespace ZeeAcom.Common.Models.OwnerModels
{
    public class CreateOwnerModel
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, Phone]
        public string CellPhone { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Collection of owner's entities
        /// </summary>
        public ICollection<Entity?>? Entities { get; set; } = new List<Entity?>();
    }
}

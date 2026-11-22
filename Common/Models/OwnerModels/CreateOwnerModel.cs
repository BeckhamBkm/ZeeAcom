using System.ComponentModel.DataAnnotations;
using ZeeAcom.Common.Models.PropertyModels;

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
        /// Collection of owner's properties
        /// </summary>
        public ICollection<CreatePropertyModel> Properties { get; set; } = new List<CreatePropertyModel>();
    }
}

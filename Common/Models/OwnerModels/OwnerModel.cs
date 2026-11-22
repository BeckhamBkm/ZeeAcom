using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.PropertyModels;

namespace ZeeAcom.Common.Models.OwnerModels
{
    public sealed class OwnerModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        //public DateTime DateCreated { get; set; }

        /// <summary>Set by the admin: only Approved landlords' listings are public.</summary>
        public OwnerStatus Status { get; set; }

        /// <summary>
        /// Collection of owner's properties
        /// </summary>
        public ICollection<PropertyModel> Properties { get; set; } = new List<PropertyModel>();
    }
}

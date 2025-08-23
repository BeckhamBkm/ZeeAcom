using System.Numerics;
using ZeeAcom.Common.Models.EntityModels;

namespace ZeeAcom.Common.Models.OwnerModels
{
    public sealed class OwnerModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Collection of owner's entities
        /// </summary>
        public ICollection<Entity?>? Entities { get; set; } = new List<Entity?>();
    }
}

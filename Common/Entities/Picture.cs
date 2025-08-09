using ZeeAcom.Common.Models.EntityModels;

namespace ZeeAcom.Common.Entities
{
    public sealed class Picture
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime DateUploaded { get; set; }

        /// <summary>
        /// Navigation Property of The Entity
        /// </summary>
        public Guid EntityId { get; set; }
        public Entity Entity { get; set; } = null!;
    }
}

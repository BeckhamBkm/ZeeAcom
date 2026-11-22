namespace ZeeAcom.Common.Models.EntityModels
{
    public class SearchOwnerModel
    {
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? CellPhone { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        //public DateTime? DateCreated { get; set; }
        public string? OrderBy { get; set; }

        public PagingArgs PagingArgs { get; set; } = PagingArgs.NoPaging;

    }
}

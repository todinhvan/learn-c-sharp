namespace CoreConcept.AuthDemo.Models
{
    public class ProfileModel
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = [];
    }
}

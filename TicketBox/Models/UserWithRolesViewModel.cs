namespace TicketBox.Models
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public IList<string> Roles { get; set; }
    }
}

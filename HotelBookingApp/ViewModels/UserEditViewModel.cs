using System.Collections.Generic;

namespace HotelBookingApp.ViewModels
{
    public class UserEditViewModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string SelectedRole { get; set; } 

        public List<string> AllRoles { get; set; } = new();
    }
}

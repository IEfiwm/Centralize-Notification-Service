using System;
using System.Collections.Generic;
using System.Text;

namespace CNS.Infrastructure.Providers.Sms.GhasedMehr.Models
{
    public class GhasedmehrAuthResponse
    {
        public GhasedmehrData Data { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }

    public class GhasedmehrData
    {
        public GhasedmehrToken Token { get; set; }
        public List<GhasedmehrRole> Roles { get; set; }
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public object Avatar { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public List<GhasedmehrMenu> Menus { get; set; }
        public List<GhasedmehrApplication> Applications { get; set; }
    }

    public class GhasedmehrToken
    {
        public string Accesstoken { get; set; }
        public string RefreshToken { get; set; }
        public string RefreshTokenExpirationDate { get; set; }
    }

    public class GhasedmehrRole
    {
        public string Name { get; set; }
        public string NameFa { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public bool IsAdmin { get; set; }
        public int? ApplicationID { get; set; }
        public int ID { get; set; }
        public string InsertDateTime { get; set; }
    }

    public class GhasedmehrMenu
    {
        public string Name { get; set; }
        public string NameFa { get; set; }
        public string Url { get; set; }
    }

    public class GhasedmehrApplication
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameFa { get; set; }
    }
}

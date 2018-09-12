using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SanveoAIO.Models
{
    public class Login
    {

        [Required(ErrorMessage = "Enter Password")]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Enter User name")]
        public string Username { get; set; }
    }
}
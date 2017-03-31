using BidARide.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BidARide.ViewModels
{
    public class LoginViewModel
    {
        public AccountLogin account { get; set; }

        [Display(Name = "Remember Me?") ]
        public bool remember { get; set; } 
    }

    public class RegisterViewModel
    {
        public AccountRegister account { get; set; }
    }
}
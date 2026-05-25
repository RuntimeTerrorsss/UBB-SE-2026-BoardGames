// <copyright file="LoginViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace BoardGames.Web.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username or email is required.")]
        [Display(Name = "Username or email")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

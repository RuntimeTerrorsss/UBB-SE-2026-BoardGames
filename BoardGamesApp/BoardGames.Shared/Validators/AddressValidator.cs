// <copyright file="AddressValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using BoardGames.Shared.DTO;

namespace BoardGames.Shared.Validators
{
    /// <summary>
    /// Validates address fields on an <see cref="UpdateProfileDTO"/>.
    /// Returns a map of field-name → error message (empty when the address is valid).
    /// </summary>
    public class AddressValidator : IValidator<Dictionary<string, string>, UpdateProfileDTO>
    {
        private const string RequiredFieldMessage = "is required";

        public Dictionary<string, string> Validate(UpdateProfileDTO profile)
        {
            var validationErrors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(profile.Country))
            {
                validationErrors[nameof(profile.Country)] = $"{nameof(profile.Country)} {RequiredFieldMessage}";
            }

            if (string.IsNullOrWhiteSpace(profile.City))
            {
                validationErrors[nameof(profile.City)] = $"{nameof(profile.City)} {RequiredFieldMessage}";
            }

            if (string.IsNullOrWhiteSpace(profile.StreetName))
            {
                validationErrors[nameof(profile.StreetName)] = $"{nameof(profile.StreetName)} {RequiredFieldMessage}";
            }

            if (string.IsNullOrWhiteSpace(profile.StreetNumber))
            {
                validationErrors[nameof(profile.StreetNumber)] = $"{nameof(profile.StreetNumber)} {RequiredFieldMessage}";
            }

            return validationErrors;
        }
    }
}

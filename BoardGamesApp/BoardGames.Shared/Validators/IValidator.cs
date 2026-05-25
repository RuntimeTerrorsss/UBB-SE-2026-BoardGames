// <copyright file="IValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Shared.Validators
{
    public interface IValidator<T, TE>
    {
        T Validate(TE element);
    }
}

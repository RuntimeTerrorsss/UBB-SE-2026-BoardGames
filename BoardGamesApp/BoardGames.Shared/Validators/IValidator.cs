// <copyright file="IValidator.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.Validators
{
    public interface IValidator<T, TE>
    {
        T Validate(TE element);
    }
}

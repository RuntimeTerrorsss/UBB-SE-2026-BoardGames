// <copyright file="IValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGames.Shared.Validators
{
    public interface IValidator<T, TE>
    {
        T Validate(TE element);
    }
}

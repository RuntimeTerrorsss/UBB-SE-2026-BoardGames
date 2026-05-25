// <copyright file="Result.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Api.Services
{
    public sealed class Result<TSuccess, TError>
    {
        private readonly TSuccess successPayloadValue;
        private readonly TError failureErrorValue;

        private Result(bool isSuccess, TSuccess successPayloadValue, TError failureErrorValue)
        {
            this.IsSuccess = isSuccess;
            this.successPayloadValue = successPayloadValue;
            this.failureErrorValue = failureErrorValue;
        }

        public bool IsSuccess { get; }

        public TSuccess Value
        {
            get
            {
                if (!this.IsSuccess)
                {
                    throw new InvalidOperationException("Cannot read Value on a failed Result.");
                }

                return this.successPayloadValue;
            }
        }

        public TError Error
        {
            get
            {
                if (this.IsSuccess)
                {
                    throw new InvalidOperationException("Cannot read Error on a successful Result.");
                }

                return this.failureErrorValue;
            }
        }

        public static Result<TSuccess, TError> Success(TSuccess successPayload)
        {
            return new Result<TSuccess, TError>(true, successPayload, default!);
        }

        public static Result<TSuccess, TError> Failure(TError failureError)
        {
            return new Result<TSuccess, TError>(false, default!, failureError);
        }
    }
}

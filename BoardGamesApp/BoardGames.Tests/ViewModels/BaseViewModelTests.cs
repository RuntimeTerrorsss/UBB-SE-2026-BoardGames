// <copyright file="BaseViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class BaseViewModelTests
    {
        [Test]
        public void IsLoading_ValueChanges_RaisesPropertyChanged()
        {
            var viewModel = new BaseViewModel();
            bool propertyChangedRaised = false;

            viewModel.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BaseViewModel.IsLoading))
                {
                    propertyChangedRaised = true;
                }
            };

            viewModel.IsLoading = true;

            Assert.That(viewModel.IsLoading, Is.True);
            Assert.That(propertyChangedRaised, Is.True);
        }

        [Test]
        public void ErrorMessage_ValueAssigned_StoresTheNewValue()
        {
            var viewModel = new BaseViewModel();
            string expectedMessage = "Invalid credentials provided.";

            viewModel.ErrorMessage = expectedMessage;

            Assert.That(viewModel.ErrorMessage, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void Constructor_DefaultState_UsesDefaultValues()
        {
            var viewModel = new BaseViewModel();

            Assert.That(viewModel.IsLoading, Is.False);
            Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        }
    }
}

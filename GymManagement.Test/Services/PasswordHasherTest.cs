using System;
using System.Collections.Generic;
using System.Text;
using GymManagement.Api.Services;
using FluentAssertions;

namespace GymManagement.Test.Services
{
    public class PasswordHasherTest
    {
        [Fact]
        public void HashPassword_Should_Return_NonEmptyString()
        {
            // Arrange
            var passwordHasher = new PasswordHasher();
            var password = "TestPassword123!";
            // Act
            var hashedPassword = passwordHasher.Hash(password);
            // Assert
            hashedPassword.Should().NotBeNullOrEmpty();
        }
        [Fact]
        public void VerifyPassword_Should_Return_True_For_Correct_Password()
        {
            var passwordHasher = new PasswordHasher();
            var password = "Admin123!";
            var hashedPassword = passwordHasher.Hash(password);

            var isVerified = passwordHasher.Verify(hashedPassword, password);

            isVerified.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_Should_Return_False_For_Incorrect_Password()
        {
            var passwordHasher = new PasswordHasher();
            var correctPassword = "TestPassword123!";
            var incorrectPassword = "WrongPassword456!";
            var hashedPassword = passwordHasher.Hash(correctPassword);

            var isVerified = passwordHasher.Verify(hashedPassword, incorrectPassword);

            isVerified.Should().BeFalse();
        }
    }
}

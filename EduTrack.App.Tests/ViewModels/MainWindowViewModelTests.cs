using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduTrack.App.ViewModels;
using FluentAssertions;

namespace EduTrack.App.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        [Fact]
        public void Greeting_Should_Return_WelcomeMessage()
        {
            // Arrange
            var vm = new MainWindowViewModel();

            // Act
            var result = vm.Greeting;

            // Assert
            result.Should().Be("Welcome to Avalonia!");
        }
    }
}

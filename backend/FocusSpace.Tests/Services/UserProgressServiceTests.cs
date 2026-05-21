using FocusSpace.Application.Interfaces;
using FocusSpace.Application.Services;
using FocusSpace.Domain.Entities;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace FocusSpace.Tests.Services
{
    public class UserProgressServiceTests
    {
        private static UserProgressService CreateService(
            Mock<IUserRepository> userRepo,
            Mock<IPlanetRepository> planetRepo) =>
            new(userRepo.Object, planetRepo.Object);

        private static User BuildUser(int id = 1, long totalMinutes = 0, int currentPlanetId = 1) =>
            new() { Id = id, TotalFocusMinutes = totalMinutes, CurrentPlanetId = currentPlanetId };

        private static List<Planet> BuildPlanets() =>
        [
            new Planet { Id = 1, Name = "Mercury", OrderNumber = 1 },
            new Planet { Id = 2, Name = "Venus",   OrderNumber = 2 },
            new Planet { Id = 3, Name = "Earth",   OrderNumber = 3 },
            new Planet { Id = 4, Name = "Mars",    OrderNumber = 4 },
            new Planet { Id = 5, Name = "Jupiter", OrderNumber = 5 },
            new Planet { Id = 6, Name = "Saturn",  OrderNumber = 6 },
            new Planet { Id = 7, Name = "Uranus",  OrderNumber = 7 },
            new Planet { Id = 8, Name = "Neptune", OrderNumber = 8 },
        ];

        [Fact]
        public async Task AddFocusMinutesAndCheckPlanetAsync_ZeroMinutes_ThrowsArgumentException()
        {
            var userRepo = new Mock<IUserRepository>();
            var planetRepo = new Mock<IPlanetRepository>();
            var service = CreateService(userRepo, planetRepo);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AddFocusMinutesAndCheckPlanetAsync(1, 0));
        }

        [Fact]
        public async Task AddFocusMinutesAndCheckPlanetAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);
            var planetRepo = new Mock<IPlanetRepository>();
            var service = CreateService(userRepo, planetRepo);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.AddFocusMinutesAndCheckPlanetAsync(99, 30));
        }

        [Fact]
        public async Task AddFocusMinutesAndCheckPlanetAsync_BelowNextThreshold_NoAdvancement()
        {
            var user = BuildUser(totalMinutes: 0, currentPlanetId: 1);
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var planetRepo = new Mock<IPlanetRepository>();
            planetRepo.Setup(r => r.GetAllOrderedAsync()).ReturnsAsync(BuildPlanets());
            var service = CreateService(userRepo, planetRepo);

            var result = await service.AddFocusMinutesAndCheckPlanetAsync(1, 60);

            Assert.False(result.Advanced);
            Assert.Equal(1, result.CurrentPlanetId);
            Assert.Equal(60, result.TotalFocusMinutes);
        }

        [Fact]
        public async Task AddFocusMinutesAndCheckPlanetAsync_ReachesVenusThreshold_AdvancesToVenus()
        {
            var user = BuildUser(totalMinutes: 100, currentPlanetId: 1);
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var planetRepo = new Mock<IPlanetRepository>();
            planetRepo.Setup(r => r.GetAllOrderedAsync()).ReturnsAsync(BuildPlanets());
            var service = CreateService(userRepo, planetRepo);

            // 100 + 30 = 130 >= 120 (Venus threshold)
            var result = await service.AddFocusMinutesAndCheckPlanetAsync(1, 30);

            Assert.True(result.Advanced);
            Assert.Equal(2, result.CurrentPlanetId);
            Assert.Equal("Venus", result.CurrentPlanetName);
            Assert.Equal(130, result.TotalFocusMinutes);
        }

        [Fact]
        public async Task AddFocusMinutesAndCheckPlanetAsync_AlreadyOnEarnedPlanet_NoAdvancement()
        {
            // User is already on Venus with 150 total minutes — adding 30 doesn't reach Earth (300)
            var user = BuildUser(totalMinutes: 150, currentPlanetId: 2);
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var planetRepo = new Mock<IPlanetRepository>();
            planetRepo.Setup(r => r.GetAllOrderedAsync()).ReturnsAsync(BuildPlanets());
            var service = CreateService(userRepo, planetRepo);

            var result = await service.AddFocusMinutesAndCheckPlanetAsync(1, 30);

            Assert.False(result.Advanced);
            Assert.Equal(2, result.CurrentPlanetId);
        }

        [Fact]
        public async Task AddFocusMinutesAndCheckPlanetAsync_ReturnsMinutesToNextPlanet()
        {
            var user = BuildUser(totalMinutes: 0, currentPlanetId: 1);
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var planetRepo = new Mock<IPlanetRepository>();
            planetRepo.Setup(r => r.GetAllOrderedAsync()).ReturnsAsync(BuildPlanets());
            var service = CreateService(userRepo, planetRepo);

            var result = await service.AddFocusMinutesAndCheckPlanetAsync(1, 50);

            // 50 total, Venus requires 120 → 70 remaining
            Assert.Equal(70, result.MinutesToNextPlanet);
        }

        [Fact]
        public async Task AddFocusMinutesAndCheckPlanetAsync_AtMaxPlanet_MinutesToNextIsNull()
        {
            // 5000+ minutes puts user on Neptune (order 8), no next planet
            var user = BuildUser(totalMinutes: 5000, currentPlanetId: 8);
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var planetRepo = new Mock<IPlanetRepository>();
            planetRepo.Setup(r => r.GetAllOrderedAsync()).ReturnsAsync(BuildPlanets());
            var service = CreateService(userRepo, planetRepo);

            var result = await service.AddFocusMinutesAndCheckPlanetAsync(1, 60);

            Assert.Null(result.MinutesToNextPlanet);
            Assert.False(result.Advanced);
        }

        [Fact]
        public async Task AddFocusMinutesAndCheckPlanetAsync_SkipsMultiplePlanets_LandsOnHighest()
        {
            // 0 + 700 = 700 → clears Venus (120) and Earth (300) and Mars (600)
            var user = BuildUser(totalMinutes: 0, currentPlanetId: 1);
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var planetRepo = new Mock<IPlanetRepository>();
            planetRepo.Setup(r => r.GetAllOrderedAsync()).ReturnsAsync(BuildPlanets());
            var service = CreateService(userRepo, planetRepo);

            var result = await service.AddFocusMinutesAndCheckPlanetAsync(1, 700);

            Assert.True(result.Advanced);
            Assert.Equal(4, result.CurrentPlanetId);
            Assert.Equal("Mars", result.CurrentPlanetName);
        }
    }
}

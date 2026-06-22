using AutoMapper;
using FluentAssertions;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Application.Services;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;
using Moq;
using Xunit;

namespace HabitContract.Tests;

public class CheckInServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IFrequencyRuleCache> _mockFrequencyCache;
    private readonly Mock<INotificationSender> _mockNotificationSender;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly FrequencyParser _frequencyParser;
    private readonly CheckInService _checkInService;

    private readonly List<CheckIn> _checkIns = new();
    private readonly List<ContractViolation> _violations = new();
    private readonly List<ContractPartner> _partners = new();
    private readonly List<User> _users = new();

    public CheckInServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockFrequencyCache = new Mock<IFrequencyRuleCache>();
        _mockNotificationSender = new Mock<INotificationSender>();
        _mockPermissionService = new Mock<IPermissionService>();
        _frequencyParser = new FrequencyParser();

        _checkIns.Clear();
        _violations.Clear();
        _partners.Clear();
        _users.Clear();

        SetupRepositories();
        SetupNotificationSender();
        SetupPermissionService();

        var senders = new[] { _mockNotificationSender.Object };
        _checkInService = new CheckInService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockFrequencyCache.Object,
            senders,
            _mockPermissionService.Object);
    }

    private void SetupRepositories()
    {
        var mockCheckInRepo = new Mock<ICheckInRepository>();
        mockCheckInRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(_checkIns.AsEnumerable());
        mockCheckInRepo.Setup(r => r.AddAsync(It.IsAny<CheckIn>()))
            .ReturnsAsync((CheckIn c) =>
            {
                c.Id = _checkIns.Count + 1;
                c.CreatedAt = DateTime.UtcNow;
                _checkIns.Add(c);
                return c;
            });
        mockCheckInRepo.Setup(r => r.GetConsecutiveDaysAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(1);
        mockCheckInRepo.Setup(r => r.GetByContractAndUserIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int cid, int uid) => _checkIns.Where(ci => ci.ContractId == cid && ci.UserId == uid).ToList());
        _mockUnitOfWork.Setup(u => u.CheckIns).Returns(mockCheckInRepo.Object);

        var mockContractRepo = new Mock<IRepository<Contract, int>>();
        mockContractRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Contract
            {
                Id = id,
                OwnerId = 1,
                HabitName = "测试习惯",
                Frequency = "每天1次",
                Status = ContractStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30)
            });
        _mockUnitOfWork.Setup(u => u.Contracts).Returns(mockContractRepo.Object);

        var mockViolationRepo = new Mock<IRepository<ContractViolation, int>>();
        mockViolationRepo.Setup(r => r.AddAsync(It.IsAny<ContractViolation>()))
            .ReturnsAsync((ContractViolation v) =>
            {
                v.Id = _violations.Count + 1;
                v.CreatedAt = DateTime.UtcNow;
                _violations.Add(v);
                return v;
            });
        _mockUnitOfWork.Setup(u => u.ContractViolations).Returns(mockViolationRepo.Object);

        var mockPartnerRepo = new Mock<IRepository<ContractPartner, int>>();
        mockPartnerRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(_partners.AsEnumerable());
        _mockUnitOfWork.Setup(u => u.ContractPartners).Returns(mockPartnerRepo.Object);

        var mockUserRepo = new Mock<IRepository<User, int>>();
        mockUserRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(_users.AsEnumerable());
        mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => _users.FirstOrDefault(u => u.Id == id));
        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);
    }

    private void SetupNotificationSender()
    {
        _mockNotificationSender.Setup(s => s.Channel)
            .Returns(ReminderChannel.InApp);
        _mockNotificationSender.Setup(s => s.SendAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    private void SetupPermissionService()
    {
        _mockPermissionService.Setup(s => s.CheckPermissionAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ContractOperation>()))
            .ReturnsAsync((true, string.Empty));
    }

    private void SetupFrequencyCache(string frequency)
    {
        var rule = _frequencyParser.Parse(frequency);
        _mockFrequencyCache.Setup(c => c.GetOrCreateAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan?>()))
            .ReturnsAsync(rule);
    }

    private void SetupContractFrequency(string frequency)
    {
        var mockContractRepo = new Mock<IRepository<Contract, int>>();
        mockContractRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Contract
            {
                Id = id,
                OwnerId = 1,
                HabitName = "测试习惯",
                Frequency = frequency,
                Status = ContractStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30)
            });
        _mockUnitOfWork.Setup(u => u.Contracts).Returns(mockContractRepo.Object);
    }

    private static CheckInCreateDto CreateCheckInDto(int contractId, DateTime? date = null, string? proofText = null)
    {
        return new CheckInCreateDto
        {
            ContractId = contractId,
            CheckInDate = date ?? DateTime.UtcNow.Date,
            ProofText = proofText ?? "打卡证明"
        };
    }

    [Fact]
    public async Task CreateCheckInAsync_DailySingleTime_FirstCheckIn_Succeeds()
    {
        var userId = 1;
        var contractId = 1;
        var today = DateTime.UtcNow.Date;
        SetupFrequencyCache("每天1次");
        SetupContractFrequency("每天1次");
        _users.Add(new User { Id = userId, Username = "testuser" });

        var dto = CreateCheckInDto(contractId, today, "第一次打卡");
        SetupMapper(dto);

        var result = await _checkInService.CreateCheckInAsync(userId, dto);

        result.Should().NotBeNull();
        _checkIns.Should().HaveCount(1);
        _checkIns[0].UserId.Should().Be(userId);
        _checkIns[0].ContractId.Should().Be(contractId);
        _violations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCheckInAsync_DailySingleTime_SecondCheckIn_FailsWithFrequencyError()
    {
        var userId = 1;
        var contractId = 1;
        var today = DateTime.UtcNow.Date;
        SetupFrequencyCache("每天1次");
        SetupContractFrequency("每天1次");
        _users.Add(new User { Id = userId, Username = "testuser" });
        _checkIns.Add(new CheckIn
        {
            Id = 1,
            ContractId = contractId,
            UserId = userId,
            CheckInDate = today.AddHours(8),
            ProofText = "第一次打卡",
            CreatedAt = today.AddHours(8)
        });

        var dto = CreateCheckInDto(contractId, today, "第二次打卡");

        Func<Task> act = async () => await _checkInService.CreateCheckInAsync(userId, dto);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*今日打卡次数已达上限*");
        _checkIns.Should().HaveCount(1);
        _violations.Should().HaveCount(1);
        _violations[0].Reason.Should().Contain("今日打卡次数已达上限");
    }

    [Fact]
    public async Task CreateCheckInAsync_DailyMultipleTimes_AllCheckInsSucceed()
    {
        var userId = 1;
        var contractId = 1;
        var today = DateTime.UtcNow.Date;
        SetupFrequencyCache("每天3次");
        SetupContractFrequency("每天3次");
        _users.Add(new User { Id = userId, Username = "testuser" });

        for (int i = 1; i <= 3; i++)
        {
            var dto = CreateCheckInDto(contractId, today, $"第{i}次打卡");
            SetupMapper(dto);
            var result = await _checkInService.CreateCheckInAsync(userId, dto);
            result.Should().NotBeNull();
        }

        _checkIns.Should().HaveCount(3);
        _violations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCheckInAsync_DailyMultipleTimes_ExceedingLimit_Fails()
    {
        var userId = 1;
        var contractId = 1;
        var today = DateTime.UtcNow.Date;
        SetupFrequencyCache("每天2次");
        SetupContractFrequency("每天2次");
        _users.Add(new User { Id = userId, Username = "testuser" });

        for (int i = 1; i <= 2; i++)
        {
            var dto = CreateCheckInDto(contractId, today, $"第{i}次打卡");
            SetupMapper(dto);
            await _checkInService.CreateCheckInAsync(userId, dto);
        }

        var thirdDto = CreateCheckInDto(contractId, today, "第3次打卡");
        Func<Task> act = async () => await _checkInService.CreateCheckInAsync(userId, thirdDto);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*今日打卡次数已达上限*");
        _checkIns.Should().HaveCount(2);
        _violations.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateCheckInAsync_WeeklyThreeTimes_AllCheckInsSucceed()
    {
        var userId = 1;
        var contractId = 1;
        var monday = new DateTime(2024, 6, 17);

        SetupFrequencyCache("每周3次");
        SetupContractFrequency("每周3次");
        _users.Add(new User { Id = userId, Username = "testuser" });

        var dates = new[] { monday, monday.AddDays(1), monday.AddDays(2) };
        for (int i = 0; i < 3; i++)
        {
            var dto = CreateCheckInDto(contractId, dates[i], $"第{i + 1}次打卡");
            SetupMapper(dto);
            var result = await _checkInService.CreateCheckInAsync(userId, dto);
            result.Should().NotBeNull();
        }

        _checkIns.Should().HaveCount(3);
        _violations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCheckInAsync_WeeklySpecificDays_OnAllowedDay_Succeeds()
    {
        var userId = 1;
        var contractId = 1;
        var monday = new DateTime(2024, 6, 17);

        SetupFrequencyCache("每周一、三、五");
        SetupContractFrequency("每周一、三、五");
        _users.Add(new User { Id = userId, Username = "testuser" });

        var dto = CreateCheckInDto(contractId, monday, "周一打卡");
        SetupMapper(dto);

        var result = await _checkInService.CreateCheckInAsync(userId, dto);

        result.Should().NotBeNull();
        _checkIns.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateCheckInAsync_WeeklySpecificDays_OnDisallowedDay_Fails()
    {
        var userId = 1;
        var contractId = 1;
        var tuesday = new DateTime(2024, 6, 18);

        SetupFrequencyCache("每周一、三、五");
        SetupContractFrequency("每周一、三、五");
        _users.Add(new User { Id = userId, Username = "testuser" });

        var dto = CreateCheckInDto(contractId, tuesday, "周二打卡");

        Func<Task> act = async () => await _checkInService.CreateCheckInAsync(userId, dto);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*今天不允许打卡*");
        _checkIns.Should().BeEmpty();
        _violations.Should().HaveCount(1);
        _violations[0].Reason.Should().Contain("今天不允许打卡");
    }

    [Fact]
    public async Task CreateCheckInAsync_DailyMultipleTimes_NotifiesPartnersOnViolation()
    {
        var userId = 1;
        var partnerId = 2;
        var contractId = 1;
        var today = DateTime.UtcNow.Date;

        SetupFrequencyCache("每天1次");
        SetupContractFrequency("每天1次");

        _users.Add(new User { Id = userId, Username = "owner" });
        _users.Add(new User { Id = partnerId, Username = "partner1" });

        _partners.Add(new ContractPartner
        {
            Id = 1,
            ContractId = contractId,
            PartnerId = partnerId,
            Status = PartnerStatus.Accepted
        });

        _checkIns.Add(new CheckIn
        {
            Id = 1,
            ContractId = contractId,
            UserId = userId,
            CheckInDate = today.AddHours(8),
            ProofText = "第一次打卡"
        });

        var dto = CreateCheckInDto(contractId, today, "第二次打卡");

        Func<Task> act = async () => await _checkInService.CreateCheckInAsync(userId, dto);
        await act.Should().ThrowAsync<BusinessException>();

        _mockNotificationSender.Verify(
            s => s.SendAsync(
                It.Is<User>(u => u.Id == partnerId),
                It.Is<string>(t => t.Contains("违约提醒")),
                It.Is<string>(c => c.Contains("违反了频率规则"))),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateCheckInAsync_DailyMultipleTimes_DifferentUsers_BothSucceed()
    {
        var user1Id = 1;
        var user2Id = 2;
        var contractId = 1;
        var today = DateTime.UtcNow.Date;

        SetupFrequencyCache("每天1次");
        SetupContractFrequency("每天1次");

        _users.Add(new User { Id = user1Id, Username = "user1" });
        _users.Add(new User { Id = user2Id, Username = "user2" });

        var dto1 = CreateCheckInDto(contractId, today, "用户1打卡");
        SetupMapper(dto1);
        var result1 = await _checkInService.CreateCheckInAsync(user1Id, dto1);

        var dto2 = CreateCheckInDto(contractId, today, "用户2打卡");
        SetupMapper(dto2);
        var result2 = await _checkInService.CreateCheckInAsync(user2Id, dto2);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        _checkIns.Should().HaveCount(2);
        _violations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCheckInAsync_DailyMultipleTimes_DifferentDays_Succeed()
    {
        var userId = 1;
        var contractId = 1;
        var day1 = DateTime.UtcNow.Date;
        var day2 = day1.AddDays(1);

        SetupFrequencyCache("每天1次");
        SetupContractFrequency("每天1次");
        _users.Add(new User { Id = userId, Username = "testuser" });

        var dto1 = CreateCheckInDto(contractId, day1, "第一天打卡");
        SetupMapper(dto1);
        var result1 = await _checkInService.CreateCheckInAsync(userId, dto1);

        var dto2 = CreateCheckInDto(contractId, day2, "第二天打卡");
        SetupMapper(dto2);
        var result2 = await _checkInService.CreateCheckInAsync(userId, dto2);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        _checkIns.Should().HaveCount(2);
        _violations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCheckInAsync_InactiveContract_Fails()
    {
        var userId = 1;
        var contractId = 1;

        var mockContractRepo = new Mock<IRepository<Contract, int>>();
        mockContractRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Contract
            {
                Id = id,
                OwnerId = 1,
                HabitName = "测试习惯",
                Frequency = "每天1次",
                Status = ContractStatus.Paused,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30)
            });
        _mockUnitOfWork.Setup(u => u.Contracts).Returns(mockContractRepo.Object);

        var dto = CreateCheckInDto(contractId);

        Func<Task> act = async () => await _checkInService.CreateCheckInAsync(userId, dto);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*进行中的契约*");
    }

    private void SetupMapper(CheckInCreateDto dto)
    {
        _mockMapper.Setup(m => m.Map<CheckIn>(dto))
            .Returns(new CheckIn
            {
                ContractId = dto.ContractId,
                CheckInDate = dto.CheckInDate,
                ProofText = dto.ProofText,
                ProofPhoto = dto.ProofPhoto
            });

        _mockMapper.Setup(m => m.Map<CheckInDto>(It.IsAny<CheckIn>()))
            .Returns((CheckIn c) => new CheckInDto
            {
                Id = c.Id,
                ContractId = c.ContractId,
                UserId = c.UserId,
                CheckInDate = c.CheckInDate,
                ProofText = c.ProofText,
                ProofPhoto = c.ProofPhoto,
                CreatedAt = c.CreatedAt
            });
    }
}

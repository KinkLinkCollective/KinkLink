using System.Text.Json;
using KinkLinkCommon.Dependencies.Glamourer;
using KinkLinkCommon.Dependencies.Glamourer.Components;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Wardrobe;
using KinkLinkServer.Domain;
using KinkLinkServer.Services;
using KinkLinkServerTests.Database;
using KinkLinkServerTests.TestInfrastructure;
using Microsoft.Extensions.Logging;

namespace KinkLinkServerTests.ServiceTests;

[Collection("DatabaseCollection")]
public class WardrobeServiceTests : DatabaseServiceTestBase
{
    private readonly WardrobeDataService _wardrobeService;

    public WardrobeServiceTests(TestDatabaseFixture fixture)
        : base(fixture)
    {
        var config = new Configuration(
            Fixture.ConnectionString,
            "test_signing_key_that_is_long_enough_for_hs256",
            "http://localhost:5006"
        );

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<WardrobeDataService>();

        _wardrobeService = new WardrobeDataService(config, logger);
    }

    private static JsonElement CreateItemData(GlamourerItem item) =>
        JsonSerializer.SerializeToElement(new { item, mods = new List<GlamourerMod>(), materials = new Dictionary<string, GlamourerMaterial>() });

    private static JsonElement CreateSetData(GlamourerDesign design) =>
        JsonSerializer.SerializeToElement(new { item = (object?)null, mods = new List<GlamourerMod>(), materials = new Dictionary<string, GlamourerMaterial>(), design });

    private static JsonElement CreateModItemData(List<GlamourerMod> mods) =>
        JsonSerializer.SerializeToElement(new { item = (object?)null, mods, materials = new Dictionary<string, GlamourerMaterial>() });

    #region GetAllWardrobeByTypeAsync Tests

    [Fact]
    public async Task GetAllWardrobeByType_ItemType_ReturnsItems()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111111, "WARDTEST1");

        var itemId = Guid.NewGuid();
        await TestHarness.InsertTestWardrobeAsync(new InsertTestWardrobeParams
        {
            Id = itemId,
            ProfileId = profileId,
            Name = "Test Item",
            Type = "item",
            Priority = 1,
            Data = CreateItemData(new GlamourerItem { ItemId = 12345, Apply = true })
        });

        var result = await _wardrobeService.GetAllWardrobeByTypeAsync(profileId, "item");

        Assert.Single(result);
        Assert.Equal("Test Item", result[0].Name);
        Assert.NotNull(result[0].Item);
        Assert.Equal(12345u, result[0].Item.ItemId);
    }

    [Fact]
    public async Task GetAllWardrobeByType_SetType_ReturnsSets()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111112, "WARDTEST2");

        var setId = Guid.NewGuid();
        await TestHarness.InsertTestWardrobeAsync(new InsertTestWardrobeParams
        {
            Id = setId,
            ProfileId = profileId,
            Name = "Test Set",
            Type = "set",
            Priority = 1,
            Data = CreateSetData(new GlamourerDesign())
        });

        var result = await _wardrobeService.GetAllWardrobeByTypeAsync(profileId, "set");

        Assert.Single(result);
        Assert.Equal("Test Set", result[0].Name);
    }

    [Fact]
    public async Task GetAllWardrobeByType_ModItemType_ReturnsModItems()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111113, "WARDTEST3");

        var modItemId = Guid.NewGuid();
        await TestHarness.InsertTestWardrobeAsync(new InsertTestWardrobeParams
        {
            Id = modItemId,
            ProfileId = profileId,
            Name = "Test ModItem",
            Type = "moditem",
            Priority = 1,
            Data = CreateModItemData([new GlamourerMod { Name = "TestMod", Enabled = true }])
        });

        var result = await _wardrobeService.GetAllWardrobeByTypeAsync(profileId, "moditem");

        Assert.Single(result);
        Assert.Equal("Test ModItem", result[0].Name);
    }

    [Fact]
    public async Task GetAllWardrobeByType_NoItems_ReturnsEmpty()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111114, "WARDTEST4");

        var result = await _wardrobeService.GetAllWardrobeByTypeAsync(profileId, "item");

        Assert.Empty(result);
    }

    #endregion

    #region GetWardrobeItemByGuid Tests

    [Fact]
    public async Task GetWardrobeItemByGuid_Exists_ReturnsItem()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111115, "WARDTEST5");

        var itemId = Guid.NewGuid();
        await TestHarness.InsertTestWardrobeAsync(new InsertTestWardrobeParams
        {
            Id = itemId,
            ProfileId = profileId,
            Name = "Get Test Item",
            Type = "item",
            Priority = 1,
            Data = CreateItemData(new GlamourerItem { ItemId = 54321, Apply = true })
        });

        var result = await _wardrobeService.GetWardrobeItemByGuid(profileId, itemId);

        Assert.NotNull(result);
        Assert.Equal("Get Test Item", result.Name);
        Assert.Equal(54321u, result.Item!.ItemId);
    }

    [Fact]
    public async Task GetWardrobeItemByGuid_NotExists_ReturnsNull()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111116, "WARDTEST6");

        var result = await _wardrobeService.GetWardrobeItemByGuid(profileId, Guid.NewGuid());

        Assert.Null(result);
    }

    #endregion

    #region CreateOrUpdateWardrobeItemsByNameAsync Tests

    [Fact]
    public async Task CreateOrUpdate_NewItem_CreatesSuccessfully()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111117, "WARDTEST7");

        var itemId = Guid.NewGuid();
        var dto = new WardrobeDto
        {
            Id = itemId,
            Name = "Created Item",
            Description = "Test description",
            Type = "item",
            Slot = GlamourerEquipmentSlot.Head,
            Item = new GlamourerItem { ItemId = 11111, Apply = true },
            Priority = RelationshipPriority.Casual
        };

        var result = await _wardrobeService.CreateOrUpdateWardrobeItemsByNameAsync(profileId, itemId, dto);

        Assert.True(result);

        var saved = await _wardrobeService.GetWardrobeItemByGuid(profileId, itemId);
        Assert.NotNull(saved);
        Assert.Equal("Created Item", saved.Name);
        Assert.Equal("Test description", saved.Description);
    }

    [Fact]
    public async Task CreateOrUpdate_ExistingItem_UpdatesSuccessfully()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111118, "WARDTEST8");

        var itemId = Guid.NewGuid();
        await TestHarness.InsertTestWardrobeAsync(new InsertTestWardrobeParams
        {
            Id = itemId,
            ProfileId = profileId,
            Name = "Original Name",
            Type = "item",
            Priority = 0,
            Data = CreateItemData(new GlamourerItem { ItemId = 11111, Apply = true })
        });

        var dto = new WardrobeDto
        {
            Id = itemId,
            Name = "Updated Name",
            Description = "Updated description",
            Type = "item",
            Slot = GlamourerEquipmentSlot.Body,
            Item = new GlamourerItem { ItemId = 22222, Apply = false },
            Priority = RelationshipPriority.Devotional
        };

        var result = await _wardrobeService.CreateOrUpdateWardrobeItemsByNameAsync(profileId, itemId, dto);

        Assert.True(result);

        var saved = await _wardrobeService.GetWardrobeItemByGuid(profileId, itemId);
        Assert.NotNull(saved);
        Assert.Equal("Updated Name", saved.Name);
        Assert.Equal(22222u, saved.Item!.ItemId);
    }

    [Fact]
    public async Task CreateOrUpdate_NewSet_CreatesSuccessfully()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111119, "WARDTEST9");

        var setId = Guid.NewGuid();
        var dto = new WardrobeDto
        {
            Id = setId,
            Name = "Created Set",
            Type = "set",
            Priority = RelationshipPriority.Casual
        };

        var result = await _wardrobeService.CreateOrUpdateWardrobeItemsByNameAsync(profileId, setId, dto);

        Assert.True(result);

        var saved = await _wardrobeService.GetWardrobeItemByGuid(profileId, setId);
        Assert.NotNull(saved);
        Assert.Equal("Created Set", saved.Name);
    }

    [Fact]
    public async Task CreateOrUpdate_ExistingSet_UpdatesSuccessfully()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111120, "WARDTEST10");

        var setId = Guid.NewGuid();
        await TestHarness.InsertTestWardrobeAsync(new InsertTestWardrobeParams
        {
            Id = setId,
            ProfileId = profileId,
            Name = "Original Set",
            Type = "set",
            Priority = 0,
            Data = CreateSetData(new GlamourerDesign())
        });

        var dto = new WardrobeDto
        {
            Id = setId,
            Name = "Updated Set",
            Type = "set",
            Priority = RelationshipPriority.Devotional
        };

        var result = await _wardrobeService.CreateOrUpdateWardrobeItemsByNameAsync(profileId, setId, dto);

        Assert.True(result);

        var saved = await _wardrobeService.GetWardrobeItemByGuid(profileId, setId);
        Assert.NotNull(saved);
        Assert.Equal("Updated Set", saved.Name);
    }

    [Fact]
    public async Task CreateOrUpdate_NewModItem_CreatesSuccessfully()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111121, "WARDTEST11");

        var modItemId = Guid.NewGuid();
        var dto = new WardrobeDto
        {
            Id = modItemId,
            Name = "Created ModItem",
            Type = "moditem",
            Mods = [new GlamourerMod { Name = "TestMod", Enabled = true }],
            Priority = RelationshipPriority.Casual
        };

        var result = await _wardrobeService.CreateOrUpdateWardrobeItemsByNameAsync(profileId, modItemId, dto);

        Assert.True(result);

        var saved = await _wardrobeService.GetWardrobeItemByGuid(profileId, modItemId);
        Assert.NotNull(saved);
        Assert.Equal("Created ModItem", saved.Name);
    }

    [Fact]
    public async Task CreateOrUpdate_ExistingModItem_UpdatesSuccessfully()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111122, "WARDTEST12");

        var modItemId = Guid.NewGuid();
        await TestHarness.InsertTestWardrobeAsync(new InsertTestWardrobeParams
        {
            Id = modItemId,
            ProfileId = profileId,
            Name = "Original ModItem",
            Type = "moditem",
            Priority = 0,
            Data = CreateModItemData([new GlamourerMod { Name = "OldMod", Enabled = false }])
        });

        var dto = new WardrobeDto
        {
            Id = modItemId,
            Name = "Updated ModItem",
            Type = "moditem",
            Mods = [new GlamourerMod { Name = "NewMod", Enabled = true }],
            Priority = RelationshipPriority.Devotional
        };

        var result = await _wardrobeService.CreateOrUpdateWardrobeItemsByNameAsync(profileId, modItemId, dto);

        Assert.True(result);

        var saved = await _wardrobeService.GetWardrobeItemByGuid(profileId, modItemId);
        Assert.NotNull(saved);
        Assert.Equal("Updated ModItem", saved.Name);
    }

    #endregion

    #region UpdateWardrobeStateAsync Tests

    [Fact]
    public async Task UpdateWardrobeState_NewState_CreatesSuccessfully()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111123, "WARDTEST13");

        var state = new WardrobeStateDto
        {
            _baseLayer = new GlamourerDesign(),
            _equipment = new Dictionary<GlamourerEquipmentSlot, WardrobeItem?>
            {
                [GlamourerEquipmentSlot.Head] = new WardrobeItem { Id = Guid.NewGuid(), ItemId = 1000, Name = "Head Item" }
            },
            _characterItems = new Dictionary<Guid, WardrobeItem>()
        };

        var result = await _wardrobeService.UpdateWardrobeStateAsync(profileId, state);

        Assert.True(result);

        var saved = await _wardrobeService.GetWardrobeStateAsync(profileId);
        Assert.NotNull(saved);
        Assert.NotNull(saved._baseLayer);
        Assert.True(saved._equipment.ContainsKey(GlamourerEquipmentSlot.Head));
    }

    [Fact]
    public async Task UpdateWardrobeState_ExistingState_UpdatesSuccessfully()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111124, "WARDTEST14");

        var initialState = new WardrobeStateDto
        {
            _baseLayer = null,
            _equipment = new Dictionary<GlamourerEquipmentSlot, WardrobeItem?>
            {
                [GlamourerEquipmentSlot.Head] = new WardrobeItem { Id = Guid.NewGuid(), ItemId = 1000, Name = "Original Head" }
            },
            _characterItems = new Dictionary<Guid, WardrobeItem>()
        };

        await _wardrobeService.UpdateWardrobeStateAsync(profileId, initialState);

        var updatedState = new WardrobeStateDto
        {
            _baseLayer = new GlamourerDesign(),
            _equipment = new Dictionary<GlamourerEquipmentSlot, WardrobeItem?>
            {
                [GlamourerEquipmentSlot.Head] = new WardrobeItem { Id = Guid.NewGuid(), ItemId = 2000, Name = "Updated Head" },
                [GlamourerEquipmentSlot.Body] = new WardrobeItem { Id = Guid.NewGuid(), ItemId = 2001, Name = "Body Item" }
            },
            _characterItems = new Dictionary<Guid, WardrobeItem>()
        };

        var result = await _wardrobeService.UpdateWardrobeStateAsync(profileId, updatedState);

        Assert.True(result);

        var saved = await _wardrobeService.GetWardrobeStateAsync(profileId);
        Assert.NotNull(saved);
        Assert.NotNull(saved._baseLayer);
        Assert.True(saved._equipment.ContainsKey(GlamourerEquipmentSlot.Head));
        Assert.True(saved._equipment.ContainsKey(GlamourerEquipmentSlot.Body));
    }

    #endregion

    #region GetWardrobeStateAsync Tests

    [Fact]
    public async Task GetWardrobeState_Exists_ReturnsState()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111125, "WARDTEST15");

        await TestHarness.InsertTestActiveWardrobeAsync(new InsertTestActiveWardrobeParams
        {
            ProfileId = profileId,
            Glamourerset = JsonSerializer.SerializeToElement(new GlamourerDesign()),
            Head = JsonSerializer.SerializeToElement(new WardrobeItem { Id = Guid.NewGuid(), ItemId = 3000, Name = "Test Head" })
        });

        var result = await _wardrobeService.GetWardrobeStateAsync(profileId);

        Assert.NotNull(result);
        Assert.NotNull(result._baseLayer);
        Assert.True(result._equipment.ContainsKey(GlamourerEquipmentSlot.Head));
    }

    [Fact]
    public async Task GetWardrobeState_NotExists_ReturnsNull()
    {
        await Fixture.ResetDatabaseAsync();

        var (profileId, _, _) = await CreateTestUserWithProfileAsync(111111111111111126, "WARDTEST16");

        var result = await _wardrobeService.GetWardrobeStateAsync(profileId);

        Assert.Null(result);
    }

    #endregion
}

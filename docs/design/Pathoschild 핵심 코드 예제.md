# 💻 Pathoschild 핵심 코드 예제

> **목적**: Pathoschild 모드들에서 추출한 핵심 코드 예제들을 통해 실제 구현 방법을 학습합니다.

## 📋 목차

1. [공통 라이브러리 패턴](#공통-라이브러리-패턴)
2. [자동화 시스템 패턴](#자동화-시스템-패턴)
3. [UI 시스템 패턴](#ui-시스템-패턴)
4. [데이터 분석 패턴](#데이터-분석-패턴)
5. [성능 최적화 패턴](#성능-최적화-패턴)

---

## 🔧 공통 라이브러리 패턴

### 1. **CommonHelper - 유틸리티 메서드**

```csharp
/// <summary>Provides common utility methods for interacting with the game code.</summary>
internal static class CommonHelper
{
    /*********
    ** Fields
    *********/
    /// <summary>A blank pixel which can be colorized and stretched to draw geometric shapes.</summary>
    private static readonly Lazy<Texture2D> LazyPixel = new(() =>
    {
        Texture2D pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
        pixel.SetData([Color.White]);
        return pixel;
    });

    /*********
    ** Accessors
    *********/
    /// <summary>A blank pixel which can be colorized and stretched to draw geometric shapes.</summary>
    public static Texture2D Pixel => CommonHelper.LazyPixel.Value;

    /*********
    ** Public methods
    *********/
    
    /// <summary>Get the player's current tile position.</summary>
    /// <param name="player">The player to check.</param>
    public static Vector2 GetPlayerTilePosition(this Farmer player)
    {
        return new Vector2(
            (int)(player.Position.X / Game1.tileSize),
            (int)(player.Position.Y / Game1.tileSize)
        );
    }

    /// <summary>Get the player's current tile position.</summary>
    /// <param name="player">The player to check.</param>
    /// <param name="location">The location to check.</param>
    public static Vector2 GetPlayerTilePosition(this Farmer player, GameLocation location)
    {
        return new Vector2(
            (int)(player.Position.X / Game1.tileSize),
            (int)(player.Position.Y / Game1.tileSize)
        );
    }

    /// <summary>Get all tiles in a rectangular area.</summary>
    /// <param name="topLeft">The top-left corner of the area.</param>
    /// <param name="bottomRight">The bottom-right corner of the area.</param>
    public static IEnumerable<Vector2> GetTileArea(Vector2 topLeft, Vector2 bottomRight)
    {
        for (int x = (int)topLeft.X; x <= (int)bottomRight.X; x++)
        {
            for (int y = (int)topLeft.Y; y <= (int)bottomRight.Y; y++)
            {
                yield return new Vector2(x, y);
            }
        }
    }
}
```

### 2. **BasePatcher - Harmony 패치 기본 클래스**

```csharp
/// <summary>A base class for applying Harmony patches to the game.</summary>
/// <typeparam name="TMod">The mod type. This is used for logging.</typeparam>
internal abstract class BasePatcher<TMod> : IPatcher
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod's manifest.</summary>
    private readonly IManifest Manifest;

    /// <summary>The mod's logger.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The Harmony instance to use for patching.</summary>
    private readonly IHarmonyHelper HarmonyHelper;


    /*********
    ** Public methods
    *********/
    /// <summary>Apply the Harmony patches for this mod.</summary>
    public void Apply(Harmony harmony, IMonitor monitor)
    {
        this.Monitor = monitor;
        this.HarmonyHelper = harmony;

        try
        {
            this.ApplyImpl();
            this.Monitor.Log($"✓ Applied {typeof(TMod).Name} patches.", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"✗ Failed to apply {typeof(TMod).Name} patches.\n{ex}", LogLevel.Error);
            throw;
        }
    }

    /// <summary>Apply the Harmony patches for this mod.</summary>
    protected abstract void ApplyImpl();

    /// <summary>Get a method to patch.</summary>
    /// <param name="type">The type containing the method.</param>
    /// <param name="name">The method name.</param>
    /// <param name="parameters">The method parameters.</param>
    protected MethodInfo RequireMethod(Type type, string name, Type[] parameters = null)
    {
        return this.HarmonyHelper.RequireMethod(type, name, parameters);
    }

    /// <summary>Get a method to patch.</summary>
    /// <param name="type">The type containing the method.</param>
    /// <param name="name">The method name.</param>
    /// <param name="parameters">The method parameters.</param>
    protected MethodInfo RequireMethod<T>(string name, Type[] parameters = null)
    {
        return this.RequireMethod(typeof(T), name, parameters);
    }
}
```

---

## ⚙️ 자동화 시스템 패턴

### 1. **IMachine - 자동화 가능한 기계 인터페이스**

```csharp
/// <summary>A machine that can process input through a recipe.</summary>
public interface IMachine : IAutomatable
{
    /*********
    ** Accessors
    *********/
    /// <summary>The location which contains the machine.</summary>
    GameLocation Location { get; }

    /// <summary>The tile area covered by the machine.</summary>
    Rectangle TileArea { get; }

    /// <summary>The machine's processing state.</summary>
    MachineState GetState();

    /// <summary>Get the machine's output information.</summary>
    /// <param name="location">The location to check.</param>
    /// <param name="tile">The tile to check.</param>
    /// <param name="machine">The machine to check.</param>
    /// <param name="item">The output item.</param>
    /// <param name="recipe">The recipe that produced the output.</param>
    /// <returns>Returns the output data, or <c>null</c> if the machine doesn't output anything.</returns>
    TrackedItem? GetOutput(GameLocation location, Vector2 tile, IMachine machine, Item item, IRecipe recipe);

    /// <summary>Provide input to the machine.</summary>
    /// <param name="location">The location to check.</param>
    /// <param name="tile">The tile to check.</param>
    /// <param name="machine">The machine to check.</param>
    /// <param name="input">The available items.</param>
    /// <param name="recipe">The recipe to check.</param>
    /// <param name="output">The output items.</param>
    /// <param name="amount">The amount of items to process.</param>
    /// <returns>Returns the amount of items processed.</returns>
    int ProcessInput(GameLocation location, Vector2 tile, IMachine machine, Item input, IRecipe recipe, out Item output, out int amount);
}
```

### 2. **MachineManager - 기계 관리 시스템**

```csharp
/// <summary>Manages machine groups and automation.</summary>
internal class MachineManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod configuration.</summary>
    private readonly ModConfig Config;

    /// <summary>The automation factories.</summary>
    private readonly IList<IAutomationFactory> AutomationFactories;

    /// <summary>The machine groups by location.</summary>
    private readonly IDictionary<GameLocation, MachineGroup[]> MachineGroups = new Dictionary<GameLocation, MachineGroup[]>();

    /// <summary>The last automation update tick.</summary>
    private int LastAutomationTick = -1;


    /*********
    ** Public methods
    *********/
    /// <summary>Get all machine groups in a location.</summary>
    /// <param name="location">The location to check.</param>
    public MachineGroup[] GetMachineGroups(GameLocation location)
    {
        if (location == null)
            return Array.Empty<MachineGroup>();

        if (!this.MachineGroups.TryGetValue(location, out MachineGroup[] groups))
        {
            groups = this.BuildMachineGroups(location);
            this.MachineGroups[location] = groups;
        }

        return groups;
    }

    /// <summary>Update automation for all machine groups.</summary>
    /// <param name="location">The location to update.</param>
    public void UpdateAutomation(GameLocation location)
    {
        if (Game1.currentGameTime.TotalGameTime.TotalMilliseconds - this.LastAutomationTick < this.Config.AutomationInterval)
            return;

        this.LastAutomationTick = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;

        MachineGroup[] groups = this.GetMachineGroups(location);
        foreach (MachineGroup group in groups)
        {
            group.Automate();
        }
    }

    /// <summary>Build machine groups for a location.</summary>
    /// <param name="location">The location to check.</param>
    private MachineGroup[] BuildMachineGroups(GameLocation location)
    {
        var groups = new List<MachineGroup>();
        var processedTiles = new HashSet<Vector2>();

        // Find all machine groups in the location
        for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
        {
            for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
            {
                Vector2 tile = new Vector2(x, y);
                if (processedTiles.Contains(tile))
                    continue;

                MachineGroup group = this.BuildMachineGroup(location, tile, processedTiles);
                if (group.Machines.Any())
                    groups.Add(group);
            }
        }

        return groups.ToArray();
    }
}
```

---

## 🎨 UI 시스템 패턴

### 1. **복잡한 메뉴 시스템 (ChestsAnywhere)**

```csharp
/// <summary>A menu overlay that lets the player navigate and edit chests.</summary>
internal abstract class StorageOverlayBase : IClickableMenu, IStorageOverlay
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying chest menu being overlaid.</summary>
    protected readonly IClickableMenu Menu;

    /// <summary>The storage being edited.</summary>
    protected readonly ManagedChest Storage;

    /// <summary>The chest factory.</summary>
    protected readonly ChestFactory ChestFactory;

    /// <summary>The mod configuration.</summary>
    protected readonly ModConfig Config;

    /// <summary>The search textbox.</summary>
    private readonly SearchTextBox SearchTextBox;

    /// <summary>The chest list.</summary>
    private readonly ChestList ChestList;

    /// <summary>The selected chest.</summary>
    private ManagedChest? SelectedChest;

    /// <summary>Whether the overlay is currently active.</summary>
    public bool IsActive { get; private set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="menu">The underlying chest menu being overlaid.</param>
    /// <param name="storage">The storage being edited.</param>
    /// <param name="chestFactory">The chest factory.</param>
    /// <param name="config">The mod configuration.</param>
    protected StorageOverlayBase(IClickableMenu menu, ManagedChest storage, ChestFactory chestFactory, ModConfig config)
    {
        this.Menu = menu;
        this.Storage = storage;
        this.ChestFactory = chestFactory;
        this.Config = config;

        this.SearchTextBox = new SearchTextBox();
        this.ChestList = new ChestList(this.ChestFactory.GetChests());

        this.InitializeLayout();
    }

    /// <summary>Handle a button press.</summary>
    /// <param name="button">The button that was pressed.</param>
    public override void receiveKeyPress(Keys key)
    {
        if (key == Keys.Escape)
        {
            this.Close();
            return;
        }

        if (this.SearchTextBox.Selected)
        {
            this.SearchTextBox.ReceiveKeyPress(key);
            this.UpdateSearch();
            return;
        }

        base.receiveKeyPress(key);
    }

    /// <summary>Handle a mouse click.</summary>
    /// <param name="x">The X position of the click.</param>
    /// <param name="y">The Y position of the click.</param>
    /// <param name="playSound">Whether to play a sound.</param>
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.SearchTextBox.ContainsPoint(x, y))
        {
            this.SearchTextBox.Selected = true;
            return;
        }

        if (this.ChestList.ContainsPoint(x, y))
        {
            this.SelectedChest = this.ChestList.GetSelectedChest(x, y);
            if (this.SelectedChest != null)
            {
                this.TransferItems();
                return;
            }
        }

        base.receiveLeftClick(x, y, playSound);
    }

    /// <summary>Draw the overlay.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    public override void draw(SpriteBatch spriteBatch)
    {
        // Draw the underlying menu first
        this.Menu.draw(spriteBatch);

        // Draw the overlay
        base.draw(spriteBatch);

        // Draw the search box
        this.SearchTextBox.Draw(spriteBatch);

        // Draw the chest list
        this.ChestList.Draw(spriteBatch);
    }

    /// <summary>Update the search results.</summary>
    private void UpdateSearch()
    {
        string search = this.SearchTextBox.Text;
        this.ChestList.UpdateSearch(search);
    }
}
```

### 2. **검색 및 필터링 시스템**

```csharp
/// <summary>A text box for searching chests.</summary>
internal class SearchTextBox
{
    /*********
    ** Fields
    *********/
    /// <summary>The text box component.</summary>
    private readonly TextBox TextBox;

    /// <summary>The search text.</summary>
    private string SearchText = "";

    /// <summary>Whether the text box is selected.</summary>
    public bool Selected { get; set; }


    /*********
    ** Accessors
    *********/
    /// <summary>The search text.</summary>
    public string Text => this.TextBox.Text;

    /// <summary>Whether the text box contains a point.</summary>
    /// <param name="x">The X position to check.</param>
    /// <param name="y">The Y position to check.</param>
    public bool ContainsPoint(int x, int y)
    {
        return this.TextBox.Bounds.Contains(x, y);
    }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public SearchTextBox()
    {
        this.TextBox = new TextBox(null, null, Game1.smallFont, Game1.textColor);
        this.TextBox.X = 100;
        this.TextBox.Y = 100;
        this.TextBox.Width = 300;
        this.TextBox.Height = 48;
    }

    /// <summary>Handle a key press.</summary>
    /// <param name="key">The key that was pressed.</param>
    public void ReceiveKeyPress(Keys key)
    {
        this.TextBox.RecieveTextInput(key);
    }

    /// <summary>Draw the search box.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw the background
        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.menuTexture,
            new Rectangle(0, 256, 60, 60),
            this.TextBox.X - 12,
            this.TextBox.Y - 12,
            this.TextBox.Width + 24,
            this.TextBox.Height + 24,
            Color.White,
            drawShadow: true
        );

        // Draw the text box
        this.TextBox.Draw(spriteBatch);
    }
}
```

---

## 📊 데이터 분석 패턴

### 1. **GameHelper - 게임 데이터 분석**

```csharp
/// <summary>Provides utility methods for working with game data.</summary>
internal static class GameHelper
{
    /*********
    ** Public methods
    *********/
    /// <summary>Get comprehensive debug information about an item.</summary>
    /// <param name="item">The item to analyze.</param>
    public static string GetDebugInfo(Item item)
    {
        if (item == null)
            return "null item";

        var info = new List<string>();

        // Basic info
        info.Add($"Name: {item.Name}");
        info.Add($"Display Name: {item.DisplayName}");
        info.Add($"Category: {item.Category}");
        info.Add($"Stack: {item.Stack}");
        info.Add($"Price: {item.salePrice()}g");

        // Type-specific info
        if (item is Tool tool)
            info.AddRange(GetToolDebugInfo(tool));
        else if (item is StardewValley.Object obj)
            info.AddRange(GetObjectDebugInfo(obj));

        return string.Join(Environment.NewLine, info);
    }

    /// <summary>Get debug info for a tool.</summary>
    /// <param name="tool">The tool to analyze.</param>
    private static IEnumerable<string> GetToolDebugInfo(Tool tool)
    {
        yield return $"Tool Type: {tool.GetType().Name}";
        yield return $"Upgrade Level: {tool.UpgradeLevel}";
        
        if (tool is MeleeWeapon weapon)
        {
            yield return $"Damage: {weapon.minDamage.Value}-{weapon.maxDamage.Value}";
            yield return $"Speed: {weapon.speed.Value}";
            yield return $"Crit Chance: {weapon.critChance.Value}";
        }
    }

    /// <summary>Get debug info for an object.</summary>
    /// <param name="obj">The object to analyze.</param>
    private static IEnumerable<string> GetObjectDebugInfo(StardewValley.Object obj)
    {
        yield return $"Object Type: {obj.GetType().Name}";
        yield return $"Category: {obj.getCategoryName()}";
        
        if (obj is StardewValley.Object crop)
        {
            yield return $"Is Seed: {crop.Category == StardewValley.Object.SeedsCategory}";
            yield return $"Can Be Planted: {crop.canBePlacedHere(Game1.currentLocation, Vector2.Zero)}";
        }
    }

    /// <summary>Get comprehensive debug information about a location.</summary>
    /// <param name="location">The location to analyze.</param>
    public static string GetDebugInfo(GameLocation location)
    {
        if (location == null)
            return "null location";

        var info = new List<string>();

        // Basic info
        info.Add($"Name: {location.Name}");
        info.Add($"Display Name: {location.displayName}");
        info.Add($"Type: {location.GetType().Name}");
        info.Add($"Map Size: {location.Map.Layers[0].LayerWidth}x{location.Map.Layers[0].LayerHeight}");

        // Objects
        info.Add($"Objects: {location.objects.Count()} items");
        foreach (var obj in location.objects.Values.Take(10))
        {
            info.Add($"  - {obj.Name} at {obj.TileLocation}");
        }

        // NPCs
        if (location.characters.Any())
        {
            info.Add($"NPCs: {location.characters.Count} characters");
            foreach (var npc in location.characters.Take(5))
            {
                info.Add($"  - {npc.Name} at {npc.Position}");
            }
        }

        return string.Join(Environment.NewLine, info);
    }
}
```

### 2. **DataParser - 데이터 파싱 시스템**

```csharp
/// <summary>Parses game data for display in lookup menus.</summary>
internal static class DataParser
{
    /*********
    ** Public methods
    *********/
    /// <summary>Parse crop data for display.</summary>
    /// <param name="crop">The crop to parse.</param>
    public static string ParseCropData(Crop crop)
    {
        if (crop == null)
            return "No crop data available.";

        var info = new List<string>();

        // Basic info
        info.Add($"Crop: {crop.GetHarvestName()}");
        info.Add($"Growth Stage: {crop.currentPhase}/{crop.phaseDays.Count}");
        info.Add($"Days in Current Phase: {crop.dayOfCurrentPhase}");

        // Growth info
        if (crop.fullyGrown.Value)
        {
            info.Add("Status: Ready for harvest");
        }
        else
        {
            int daysLeft = crop.phaseDays.Count - crop.currentPhase;
            info.Add($"Days Until Harvest: {daysLeft}");
        }

        // Quality info
        if (crop.fullyGrown.Value)
        {
            info.Add("Quality: Normal");
            if (Game1.player.professions.Contains(5)) // Tiller profession
                info.Add("Quality: Silver (Tiller profession)");
            if (Game1.player.professions.Contains(6)) // Artisan profession
                info.Add("Quality: Gold (Artisan profession)");
        }

        return string.Join(Environment.NewLine, info);
    }

    /// <summary>Parse animal data for display.</summary>
    /// <param name="animal">The animal to parse.</param>
    public static string ParseAnimalData(FarmAnimal animal)
    {
        if (animal == null)
            return "No animal data available.";

        var info = new List<string>();

        // Basic info
        info.Add($"Animal: {animal.displayName}");
        info.Add($"Type: {animal.type.Value}");
        info.Add($"Age: {animal.age.Value} days");
        info.Add($"Happiness: {animal.happiness.Value}/200");

        // Production info
        info.Add($"Days Since Last Produce: {animal.daysSinceLastLay.Value}");
        if (animal.currentProduce.Value != null)
        {
            info.Add($"Next Produce: {animal.currentProduce.Value} (in {animal.daysSinceLastLay.Value} days)");
        }

        // Mood info
        string mood = animal.moodMessage.Value;
        if (!string.IsNullOrEmpty(mood))
        {
            info.Add($"Mood: {mood}");
        }

        return string.Join(Environment.NewLine, info);
    }
}
```

---

## ⚡ 성능 최적화 패턴

### 1. **지연 로딩 (Lazy Loading)**

```csharp
/// <summary>Provides lazy-loaded game data.</summary>
internal static class LazyGameData
{
    /*********
    ** Fields
    *********/
    /// <summary>Lazy-loaded crop data.</summary>
    private static readonly Lazy<Dictionary<int, CropData>> LazyCropData = new(() =>
    {
        var cropData = new Dictionary<int, CropData>();
        
        // Load crop data from game files
        foreach (var entry in Game1.cropData)
        {
            cropData[entry.Key] = new CropData(entry.Value);
        }
        
        return cropData;
    });

    /// <summary>Lazy-loaded object data.</summary>
    private static readonly Lazy<Dictionary<int, ObjectData>> LazyObjectData = new(() =>
    {
        var objectData = new Dictionary<int, ObjectData>();
        
        // Load object data from game files
        foreach (var entry in Game1.objectData)
        {
            objectData[entry.Key] = new ObjectData(entry.Value);
        }
        
        return objectData;
    });


    /*********
    ** Accessors
    *********/
    /// <summary>Get crop data.</summary>
    public static Dictionary<int, CropData> CropData => LazyGameData.LazyCropData.Value;

    /// <summary>Get object data.</summary>
    public static Dictionary<int, ObjectData> ObjectData => LazyGameData.LazyObjectData.Value;
}
```

### 2. **캐싱 시스템**

```csharp
/// <summary>Provides caching for expensive operations.</summary>
internal class CacheManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The cached data.</summary>
    private readonly Dictionary<string, object> Cache = new();

    /// <summary>The cache expiration times.</summary>
    private readonly Dictionary<string, DateTime> ExpirationTimes = new();

    /// <summary>The default cache duration.</summary>
    private readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);


    /*********
    ** Public methods
    *********/
    /// <summary>Get a cached value or compute it if not cached.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to compute the value.</param>
    /// <param name="duration">The cache duration.</param>
    public T GetOrSet<T>(string key, Func<T> factory, TimeSpan? duration = null)
    {
        // Check if cached value exists and is not expired
        if (this.Cache.TryGetValue(key, out object cachedValue) && 
            this.ExpirationTimes.TryGetValue(key, out DateTime expirationTime) &&
            DateTime.Now < expirationTime)
        {
            return (T)cachedValue;
        }

        // Compute new value
        T newValue = factory();
        
        // Cache the new value
        this.Cache[key] = newValue;
        this.ExpirationTimes[key] = DateTime.Now + (duration ?? this.DefaultCacheDuration);

        return newValue;
    }

    /// <summary>Clear expired cache entries.</summary>
    public void ClearExpired()
    {
        var expiredKeys = this.ExpirationTimes
            .Where(kvp => DateTime.Now >= kvp.Value)
            .Select(kvp => kvp.Key)
            .ToArray();

        foreach (string key in expiredKeys)
        {
            this.Cache.Remove(key);
            this.ExpirationTimes.Remove(key);
        }
    }

    /// <summary>Clear all cache entries.</summary>
    public void Clear()
    {
        this.Cache.Clear();
        this.ExpirationTimes.Clear();
    }
}
```

### 3. **배치 처리 시스템**

```csharp
/// <summary>Processes items in batches for better performance.</summary>
internal class BatchProcessor
{
    /*********
    ** Fields
    *********/
    /// <summary>The items to process.</summary>
    private readonly Queue<Item> ProcessingQueue = new();

    /// <summary>The maximum batch size.</summary>
    private readonly int MaxBatchSize;

    /// <summary>The processing interval.</summary>
    private readonly TimeSpan ProcessingInterval;

    /// <summary>The last processing time.</summary>
    private DateTime LastProcessingTime = DateTime.MinValue;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="maxBatchSize">The maximum number of items to process in one batch.</param>
    /// <param name="processingInterval">The minimum time between batch processing.</param>
    public BatchProcessor(int maxBatchSize = 100, TimeSpan? processingInterval = null)
    {
        this.MaxBatchSize = maxBatchSize;
        this.ProcessingInterval = processingInterval ?? TimeSpan.FromMilliseconds(100);
    }

    /// <summary>Add an item to the processing queue.</summary>
    /// <param name="item">The item to process.</param>
    public void QueueItem(Item item)
    {
        if (item != null)
        {
            this.ProcessingQueue.Enqueue(item);
        }
    }

    /// <summary>Process queued items in batches.</summary>
    /// <param name="processor">The function to process each item.</param>
    public void ProcessBatch(Action<Item> processor)
    {
        if (DateTime.Now - this.LastProcessingTime < this.ProcessingInterval)
            return;

        int batchSize = Math.Min(this.MaxBatchSize, this.ProcessingQueue.Count);
        for (int i = 0; i < batchSize; i++)
        {
            if (this.ProcessingQueue.TryDequeue(out Item item))
            {
                processor(item);
            }
        }

        this.LastProcessingTime = DateTime.Now;
    }

    /// <summary>Get the number of queued items.</summary>
    public int QueuedCount => this.ProcessingQueue.Count;
}
```

---

## 🎯 우리 프로젝트 적용 예제

### 1. **DrawingSkill 모드에 자동화 시스템 적용**

```csharp
/// <summary>Manages automatic inspiration collection.</summary>
internal class InspirationAutomationManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod configuration.</summary>
    private readonly ModConfig Config;

    /// <summary>The inspiration collectors.</summary>
    private readonly IList<IInspirationCollector> Collectors;

    /// <summary>The batch processor for inspiration collection.</summary>
    private readonly BatchProcessor BatchProcessor;

    /// <summary>The cache manager for location data.</summary>
    private readonly CacheManager CacheManager;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The mod configuration.</param>
    public InspirationAutomationManager(ModConfig config)
    {
        this.Config = config;
        this.Collectors = new List<IInspirationCollector>();
        this.BatchProcessor = new BatchProcessor(50, TimeSpan.FromMilliseconds(200));
        this.CacheManager = new CacheManager();
    }

    /// <summary>Update automation for the current location.</summary>
    /// <param name="location">The current location.</param>
    public void UpdateAutomation(GameLocation location)
    {
        if (!this.Config.EnableAutomation || location == null)
            return;

        // Get cached location data
        var locationData = this.CacheManager.GetOrSet(
            $"location_{location.Name}",
            () => this.AnalyzeLocation(location),
            TimeSpan.FromMinutes(1)
        );

        // Queue inspiration collection
        foreach (var tile in locationData.InspirationTiles)
        {
            var inspiration = new InspirationItem(tile, location);
            this.BatchProcessor.QueueItem(inspiration);
        }

        // Process batch
        this.BatchProcessor.ProcessBatch(this.ProcessInspiration);
    }

    /// <summary>Process a single inspiration item.</summary>
    /// <param name="inspiration">The inspiration to process.</param>
    private void ProcessInspiration(Item inspiration)
    {
        if (inspiration is InspirationItem item)
        {
            // Check if player can collect this inspiration
            if (this.CanCollectInspiration(item))
            {
                this.CollectInspiration(item);
            }
        }
    }
}
```

### 2. **FarmStatistics 모드에 데이터 분석 시스템 적용**

```csharp
/// <summary>Provides comprehensive farm statistics analysis.</summary>
internal class FarmStatisticsAnalyzer
{
    /*********
    ** Fields
    *********/
    /// <summary>The cache manager for statistics data.</summary>
    private readonly CacheManager CacheManager;

    /// <summary>The batch processor for data analysis.</summary>
    private readonly BatchProcessor BatchProcessor;


    /*********
    ** Public methods
    *********/
    /// <summary>Get comprehensive farm statistics.</summary>
    /// <param name="farm">The farm to analyze.</param>
    public FarmStatistics GetFarmStatistics(Farm farm)
    {
        return this.CacheManager.GetOrSet(
            $"farm_stats_{farm.Name}",
            () => this.AnalyzeFarm(farm),
            TimeSpan.FromMinutes(5)
        );
    }

    /// <summary>Analyze farm data in detail.</summary>
    /// <param name="farm">The farm to analyze.</param>
    private FarmStatistics AnalyzeFarm(Farm farm)
    {
        var stats = new FarmStatistics();

        // Analyze crops
        this.AnalyzeCrops(farm, stats);

        // Analyze animals
        this.AnalyzeAnimals(farm, stats);

        // Analyze buildings
        this.AnalyzeBuildings(farm, stats);

        // Analyze time usage
        this.AnalyzeTimeUsage(stats);

        return stats;
    }

    /// <summary>Analyze crop data.</summary>
    /// <param name="farm">The farm to analyze.</param>
    /// <param name="stats">The statistics object to populate.</param>
    private void AnalyzeCrops(Farm farm, FarmStatistics stats)
    {
        var crops = new List<CropStatistic>();

        foreach (var obj in farm.objects.Values.OfType<Crop>())
        {
            var cropStat = new CropStatistic
            {
                Name = obj.GetHarvestName(),
                GrowthStage = obj.currentPhase,
                DaysToHarvest = obj.phaseDays.Count - obj.currentPhase,
                Quality = this.GetCropQuality(obj),
                Revenue = this.CalculateCropRevenue(obj)
            };

            crops.Add(cropStat);
        }

        stats.Crops = crops;
        stats.TotalCropRevenue = crops.Sum(c => c.Revenue);
        stats.CropCount = crops.Count;
    }
}
```

---

## 📚 학습 포인트 요약

### 🔥 **핵심 패턴**
1. **인터페이스 기반 설계**: 확장 가능한 시스템 구축
2. **캐싱 시스템**: 성능 최적화를 위한 데이터 캐싱
3. **배치 처리**: 대량 데이터 처리 시 효율성
4. **지연 로딩**: 메모리 사용량 최적화
5. **이벤트 기반 아키텍처**: 느슨한 결합과 확장성

### 🎯 **우리 프로젝트 적용**
1. **DrawingSkill**: 자동화 시스템 + 캐싱 + 배치 처리
2. **FarmStatistics**: 데이터 분석 + 캐싱 + 성능 최적화
3. **공통 라이브러리**: 유틸리티 메서드 + 패치 시스템

### 📖 **참고 자료**
- [Pathoschild StardewMods](https://github.com/Pathoschild/StardewMods)
- [SMAPI 공식 문서](https://stardewvalleywiki.com/Modding:SMAPI)
- [C# 성능 최적화 가이드](https://docs.microsoft.com/en-us/dotnet/framework/performance/)

---

**업데이트 날짜**: 2024년 7월 25일  
**작성자**: jinhyy  
**참고 자료**: [Pathoschild StardewMods](https://github.com/Pathoschild/StardewMods)

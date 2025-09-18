using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace StardewUI.Framework;

/// <summary>
/// Minimal subset of the StardewUI API needed by FarmDashboard.
/// </summary>
public interface IViewEngine
{
    /// <summary>Registers view assets located within the mod folder.</summary>
    /// <param name="assetPrefix">The asset prefix exposed to the content pipeline, e.g. <c>Mods/MyMod/Views</c>.</param>
    /// <param name="directory">Relative path within the mod directory that contains the StarML files.</param>
    void RegisterViews(string assetPrefix, string directory);

    /// <summary>Enables live reloading support for StarML assets.</summary>
    /// <param name="projectDirectory">Optional project directory for IDE sync. Provide <c>null</c> to use the default.</param>
    void EnableHotReloading(string? projectDirectory = null);

    /// <summary>Creates a controller for a menu defined by a StarML asset.</summary>
    /// <param name="assetName">The content pipeline asset name.</param>
    /// <param name="context">Binding context for the view.</param>
    IMenuController CreateMenuControllerFromAsset(string assetName, object? context = null);
}

/// <summary>
/// Minimal subset of the StardewUI menu controller API used by FarmDashboard.
/// </summary>
public interface IMenuController : IDisposable
{
    event Action Closed;
    event Action Closing;

    Func<bool>? CanClose { get; set; }

    IClickableMenu Menu { get; }

    float DimmingAmount { get; set; }

    bool CloseOnOutsideClick { get; set; }

    string CloseSound { get; set; }

    void EnableCloseButton(Texture2D? texture = null, Rectangle? sourceRect = null, float scale = 4f);

    void Close();
}

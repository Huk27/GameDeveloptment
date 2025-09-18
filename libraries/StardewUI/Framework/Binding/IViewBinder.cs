﻿using StardewUI.Framework.Content;
using StardewUI.Framework.Descriptors;
using StardewUI.Framework.Dom;

namespace StardewUI.Framework.Binding;

/// <summary>
/// Service for creating view bindings and their dependencies.
/// </summary>
public interface IViewBinder
{
    /// <summary>
    /// Creates a view binding.
    /// </summary>
    /// <param name="view">The view that will be bound.</param>
    /// <param name="element">The element data providing the literal or binding attributes.</param>
    /// <param name="context">The binding context/data, for any non-asset bindings using bindings whose
    /// <see cref="Grammar.AttributeValueType"/> is one of the recognized
    /// <see cref="Grammar.AttributeValueTypeExtensions.IsContextBinding(Grammar.AttributeValueType)"/> types.</param>
    /// <param name="resolutionScope">Scope for resolving externalized attributes, such as translation keys.</param>
    /// <returns>A view binding that can be used to propagate changes in the <paramref name="context"/> or any dependent
    /// assets to the <paramref name="view"/>.</returns>
    IViewBinding Bind(IView view, IElement element, BindingContext? context, IResolutionScope resolutionScope);

    /// <summary>
    /// Retrieves the descriptor for a view, which provides information about its properties.
    /// </summary>
    /// <remarks>
    /// Descriptors participate in view binding but may also be used for other purposes, such as updating child lists.
    /// </remarks>
    /// <param name="view">The view instance.</param>
    /// <returns>The descriptor for the <paramref name="view"/>.</returns>
    IViewDescriptor GetDescriptor(IView view);
}

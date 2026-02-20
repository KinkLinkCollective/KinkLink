using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinkLinkClient.Dependencies.Glamourer.Domain;
using KinkLinkClient.Dependencies.Glamourer.Services;
using KinkLinkClient.Domain;
using KinkLinkClient.Managers;
using KinkLinkClient.Services;
using KinkLinkClient.Utils;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Network;

namespace KinkLinkClient.UI.Views.Wardrobe;

// TODO: This class needs to be implemented
public class WardrobeViewUiController : IDisposable
{
    // Injected
    private readonly GlamourerService _glamourerService;
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;

    /// <summary>
    ///     Search for the design we'd like to send
    /// </summary>
    public string SearchTerm = string.Empty;

    /// <summary>
    ///     The currently selected Guid of the Design to send
    /// </summary>
    public Guid SelectedDesignId = Guid.Empty;

    /// <summary>
    ///     Cached list of designs
    /// </summary>
    private List<Design>? _sorted;

    /// <summary>
    ///     Filtered cached list of designs
    /// </summary>
    private List<Design>? _filtered;

    /// <summary>
    ///     The designs to display in the Ui
    /// </summary>
    public List<Design>? Designs => SearchTerm == string.Empty ? _sorted : _filtered;

    public bool ShouldApplyCustomization = true;
    public bool ShouldApplyEquipment = true;

    /// <summary>
    ///     <inheritdoc cref="TransformationViewUiController"/>
    /// </summary>
    public WardrobeViewUiController(
        GlamourerService glamourer,
        NetworkService networkService,
        SelectionManager selectionManager
    )
    {
        _glamourerService = glamourer;
        _networkService = networkService;
        _selectionManager = selectionManager;

        _glamourerService.IpcReady += OnIpcReady;
        if (_glamourerService.ApiAvailable)
            _ = RefreshGlamourerDesigns();
    }

    /// <summary>
    ///     Filters the sorted design list by search term
    /// </summary>
    public void FilterDesignsBySearchTerm()
    {
        _filtered = _sorted is not null ? FilterDesigns(_sorted, SearchTerm).ToList() : null;
    }

    /// <summary>
    ///     Recursive method to filter nodes based on both folders and content names
    /// </summar
    private List<Design> FilterDesigns(IEnumerable<Design> designs, string searchTerms)
    {
        // Reset the selected so possibly unselected designs aren't stored
        SelectedDesignId = Guid.Empty;

        var filtered = new List<Design>();
        foreach (var design in designs)
        {
            if (design.Path.Contains(searchTerms, StringComparison.OrdinalIgnoreCase))
            {
                filtered.Add(design);
            }
        }

        return filtered;
    }

    public async Task RefreshGlamourerDesigns()
    {
        SelectedDesignId = Guid.Empty;

        if (await _glamourerService.GetDesignList().ConfigureAwait(false) is not { } designs)
            return;

        // Assignment
        _sorted = designs.OrderBy(key => key.Path).ToList();
    }

    // public async Task SendDesign()
    // {
    //     if (SelectedDesignId == Guid.Empty)
    //         return;
    //
    //     var flags = GlamourerApplyFlags.Once;
    //     if (ShouldApplyCustomization)
    //         flags |= GlamourerApplyFlags.Customization;
    //     if (ShouldApplyEquipment)
    //         flags |= GlamourerApplyFlags.Equipment;
    //
    //     // Don't send one with nothing
    //     if (flags is GlamourerApplyFlags.Once)
    //         return;
    //
    //     if (
    //         await _glamourerService.GetDesignAsync(SelectedDesignId).ConfigureAwait(false)
    //         is not { } design
    //     )
    //         return;
    //
    //     await _networkCommandManager
    //         .SendTransformation(_selectionManager.GetSelectedFriendCodes(), design, flags)
    //         .ConfigureAwait(false);
    // }
    //
    /// <summary>
    ///     The dictionary returned by glamourer is not sorted, so we will recursively go through and sort the children
    /// </summary>
    private static void SortTree<T>(FolderNode<T> root)
    {
        // Copy all the children from this node and sort them by folder, then name
        var sorted = root
            .Children.Values.OrderByDescending(node => node.IsFolder)
            .ThenBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Clear all the children with the values sorted and copied
        root.Children.Clear();

        // Reintroduce because dictionaries preserve insertion order
        foreach (var node in sorted)
            root.Children[node.Name] = node;

        // Recursively sort the remaining children
        foreach (var child in root.Children.Values)
            SortTree(child);
    }

    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshGlamourerDesigns();
    }

    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}

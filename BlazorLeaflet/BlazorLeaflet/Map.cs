using System;
using BlazorLeaflet.Models;
using BlazorLeaflet.Utils;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.JSInterop;
using BlazorLeaflet.Models.Events;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorLeaflet
{
    public sealed class Map 
    {
        /// <summary>
        /// Initial geographic center of the map
        /// </summary>
        public LatLng Center { get; set; } = new LatLng();

        /// <summary>
        /// Initial map zoom level
        /// </summary>
        public float Zoom { get; set; }

        /// <summary>
        /// Minimum zoom level of the map. If not specified and at least one 
        /// GridLayer or TileLayer is in the map, the lowest of their minZoom
        /// options will be used instead.
        /// </summary>
        public float? MinZoom { get; set; }

        /// <summary>
        /// Maximum zoom level of the map. If not specified and at least one
        /// GridLayer or TileLayer is in the map, the highest of their maxZoom
        /// options will be used instead.
        /// </summary>
        public float? MaxZoom { get; set; }

        /// <summary>
        /// When this option is set, the map restricts the view to the given
        /// geographical bounds, bouncing the user back if the user tries to pan
        /// outside the view.
        /// </summary>
        public Tuple<LatLng, LatLng> MaxBounds { get; set; }

        /// <summary>
        /// Whether a zoom control is added to the map by default.
        /// <para/>
        /// Defaults to true.
        /// </summary>
        public bool ZoomControl { get; set; } = true;

        /// <summary>
        /// Event raised when the component has finished its first render.
        /// </summary>
        public event Action OnInitialized;

        public string Id { get; }

        private ObservableCollection<Layer> _layers = new ObservableCollection<Layer>();

        public IJSRuntime JsRuntime { get; }
        public IJSObjectReference JsRef { get; set; }

        private bool _isInitialized;
        private readonly List<ILayer> newLayers;
        public IEnumerable<ILayer> NewLayers => this.newLayers;

        public Map(IJSRuntime jsRuntime)
        {
            JsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            Id = StringHelper.GetRandomString(10);

            _layers.CollectionChanged += OnLayersChanged;

            this.newLayers = new List<ILayer>();
        }

        /// <summary>
        /// This method MUST be called only once by the Blazor component upon rendering, and never by the user.
        /// </summary>
        internal void RaiseOnInitialized()
        {
            _isInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Add a layer to the map.
        /// </summary>
        /// <param name="layer">The layer to be added.</param>
        /// <exception cref="System.ArgumentNullException">Throws when the layer is null.</exception>
        /// <exception cref="UninitializedMapException">Throws when the map has not been yet initialized.</exception>
        public void AddLayer(Layer layer)
        {
            if (layer is null)
            {
                throw new ArgumentNullException(nameof(layer));
            }

            if (!_isInitialized)
            {
                throw new InvalidOperationException();
            }

            _layers.Add(layer);
        }

        /// <summary>
        /// Remove a layer from the map.
        /// </summary>
        /// <param name="layer">The layer to be removed.</param>
        /// <exception cref="System.ArgumentNullException">Throws when the layer is null.</exception>
        /// <exception cref="UninitializedMapException">Throws when the map has not been yet initialized.</exception>
        public void RemoveLayer(Layer layer)
        {
            if (layer is null)
            {
                throw new ArgumentNullException(nameof(layer));
            }

            if (!_isInitialized)
            {
                throw new InvalidOperationException();
            }

            _layers.Remove(layer);
        }

        /// <summary>
        /// Get a read only collection of the current layers.
        /// </summary>
        /// <returns>A read only collection of layers.</returns>
        public IReadOnlyCollection<Layer> GetLayers()
        {
            return _layers.ToList().AsReadOnly();
        }

        private void OnLayersChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in args.NewItems)
                {
                    var layer = item as Layer;
                    LeafletInterops.AddLayer(JsRuntime, Id, layer);
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in args.OldItems)
                {
                    if (item is Layer layer)
                    {
                        LeafletInterops.RemoveLayer(JsRuntime, Id, layer.Id);
                    }
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Replace
                     || args.Action == NotifyCollectionChangedAction.Move)
            {
                foreach (var oldItem in args.OldItems)
                    if (oldItem is Layer layer)
                        LeafletInterops.RemoveLayer(JsRuntime, Id, layer.Id);

                foreach (var newItem in args.NewItems)
                    LeafletInterops.AddLayer(JsRuntime, Id, newItem as Layer);
            }
        }

        public void FitBounds(PointF corner1, PointF corner2, PointF? padding = null, float? maxZoom = null)
        {
            LeafletInterops.FitBounds(JsRuntime, Id, corner1, corner2, padding, maxZoom);
        }

        public ValueTask PanToAsync(PointF position, bool animate = false, float duration = 0.25f,
            float easeLinearity = 0.25f, bool noMoveStart = false)
        {
            return LeafletInterops.PanToAsync(JsRuntime, Id, position, animate, duration, easeLinearity, noMoveStart);
        }

        public ValueTask<LatLng> GetCenterAsync() => LeafletInterops.GetCenter(JsRuntime, Id);
        public ValueTask<float> GetZoomAsync() => LeafletInterops.GetZoom(JsRuntime, Id);

        /// <summary>
        /// Increases the zoom level by one notch.
        /// 
        /// If <c>shift</c> is held down, increases it by three.
        /// </summary>
        public ValueTask ZoomInAsync(MouseEventArgs e) => LeafletInterops.ZoomInAsync(JsRuntime, Id, e);

        /// <summary>
        /// Decreases the zoom level by one notch.
        /// 
        /// If <c>shift</c> is held down, decreases it by three.
        /// </summary>
        public async Task ZoomOutAsync(MouseEventArgs e) => await LeafletInterops.ZoomOutAsync(JsRuntime, Id, e);

        public ValueTask InvalidateSizeAsync() => LeafletInterops.InvalidateSizeAsync(JsRuntime, Id);

        public delegate void MapEventHandler(object sender, Event e);
        public delegate void MapResizeEventHandler(object sender, ResizeEvent e);

        public event MapEventHandler OnZoomLevelsChange;
        [JSInvokable]
        public void NotifyZoomLevelsChange(Event e) => OnZoomLevelsChange?.Invoke(this, e);

        public event MapResizeEventHandler OnResize;
        [JSInvokable]
        public void NotifyResize(ResizeEvent e) => OnResize?.Invoke(this, e);

        public event MapEventHandler OnUnload;
        
        [JSInvokable]
        public void NotifyUnload(Event e) => OnUnload?.Invoke(this, e);

        public event MapEventHandler OnViewReset;
        [JSInvokable]
        public void NotifyViewReset(Event e) => OnViewReset?.Invoke(this, e);

        public event MapEventHandler OnLoad;
        
        [JSInvokable]
        public void NotifyLoad(Event e) => OnLoad?.Invoke(this, e);

        public event MapEventHandler OnZoomStart;
        
        [JSInvokable]
        public void NotifyZoomStart(Event e) => OnZoomStart?.Invoke(this, e);

        public event MapEventHandler OnMoveStart;
        [JSInvokable]
        public void NotifyMoveStart(Event e) => OnMoveStart?.Invoke(this, e);

        public event MapEventHandler OnZoom;
        
        [JSInvokable]
        public void NotifyZoom(Event e) => OnZoom?.Invoke(this, e);

        public event MapEventHandler OnMove;
        
        [JSInvokable]
        public void NotifyMove(Event e) => OnMove?.Invoke(this, e);

        public event MapEventHandler OnZoomEnd;
        
        [JSInvokable]
        public void NotifyZoomEnd(Event e) => OnZoomEnd?.Invoke(this, e);

        public event MapEventHandler OnMoveEnd;
        [JSInvokable]
        public void NotifyMoveEnd(Event e) => OnMoveEnd?.Invoke(this, e);

        public event MouseEventHandler OnMouseMove;
        [JSInvokable]
        public void NotifyMouseMove(MouseEvent eventArgs) => OnMouseMove?.Invoke(this, eventArgs);

        public event MapEventHandler OnKeyPress;
        [JSInvokable]
        public void NotifyKeyPress(Event eventArgs) => OnKeyPress?.Invoke(this, eventArgs);

        public event MapEventHandler OnKeyDown;
        [JSInvokable]
        public void NotifyKeyDown(Event eventArgs) => OnKeyDown?.Invoke(this, eventArgs);

        public event MapEventHandler OnKeyUp;
        [JSInvokable]
        public void NotifyKeyUp(Event eventArgs) => OnKeyUp?.Invoke(this, eventArgs);

        public event MouseEventHandler OnPreClick;
        [JSInvokable]
        public void NotifyPreClick(MouseEvent eventArgs) => OnPreClick?.Invoke(this, eventArgs);

        // Has the same events as InteractiveLayer, but it is not a layer. 
        // Could place this code in its own class and make Layer inherit from that, but not every layer is interactive...
        // Is there a way to not duplicate this code?

        public delegate void MouseEventHandler(Map sender, MouseEvent e);

        public event MouseEventHandler OnClick;
        [JSInvokable]
        public void NotifyClick(MouseEvent eventArgs) => OnClick?.Invoke(this, eventArgs);

        public event MouseEventHandler OnDblClick;
        [JSInvokable]
        public void NotifyDblClick(MouseEvent eventArgs) => OnDblClick?.Invoke(this, eventArgs);

        public event MouseEventHandler OnMouseDown;
        [JSInvokable]
        public void NotifyMouseDown(MouseEvent eventArgs) => OnMouseDown?.Invoke(this, eventArgs);

        public event MouseEventHandler OnMouseUp;
        [JSInvokable]
        public void NotifyMouseUp(MouseEvent eventArgs) => OnMouseUp?.Invoke(this, eventArgs);

        public event MouseEventHandler OnMouseOver;
        [JSInvokable]
        public void NotifyMouseOver(MouseEvent eventArgs) => OnMouseOver?.Invoke(this, eventArgs);

        public event MouseEventHandler OnMouseOut;
        
        [JSInvokable]
        public void NotifyMouseOut(MouseEvent eventArgs) => OnMouseOut?.Invoke(this, eventArgs);

        public event MouseEventHandler OnContextMenu;

        [JSInvokable]
        public void NotifyContextMenu(MouseEvent eventArgs) => OnContextMenu?.Invoke(this, eventArgs);

        public ValueTask OpenPopupAsync(Popup popup)
        {
            return LeafletInterops.OpenPopupOnMapAsync(this.JsRuntime, this.Id,popup);
        }
        public ValueTask ClosePopupAsync(Popup popup)
        {
            return LeafletInterops.ClosePopupOnMapAsync(this.JsRuntime, this.Id,popup);
        }
        
        public async ValueTask AddNewLayerAsync(ILayer layer)
        {
            if (layer.JsRef == null)
            {
                if (layer is Layer l)
                    await LeafletInterops.RegisterAsync(JsRuntime, l);
                else
                    throw new ArgumentException("Layer is not registered");
            }
            this.newLayers.Add(layer);
            await JsRuntime.InvokeVoidAsync($"{LeafletInterops.BaseObjectContainer}.addNewLayer",
                JsRef,layer.JsRef);
        }

        public async ValueTask<bool> RemoveNewLayerAsync(ILayer layer)
        {
            await JsRuntime.InvokeVoidAsync($"{LeafletInterops.BaseObjectContainer}.removeNewLayer",
                JsRef,layer.JsRef);
            var result = this.newLayers.Remove(layer);
            return result;
        }
        
    }
}

﻿using System;
using System.Threading.Tasks;
using BlazorLeaflet.Models.Events;
using BlazorLeaflet.Utils;
using Microsoft.JSInterop;

namespace BlazorLeaflet.Models
{
    public abstract class Layer : IAsyncDisposable,ILayer
    {
        public IJSObjectReference JsRef { get; set; }
        public DotNetObjectReference<Layer> DotNetRef { get; }


        /// <summary>
        /// Unique identifier used by the interoperability service on the client side to identify layers.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// By default the layer will be added to the map's overlay pane. Overriding this option will cause the layer to be placed on another pane by default.
        /// </summary>
        public virtual string Pane { get; set; } = "overlayPane";

        /// <summary>
        /// String to be shown in the attribution control, e.g. "© OpenStreetMap contributors". It describes the layer data and is often a legal obligation towards copyright holders and tile providers.
        /// </summary>
        public string Attribution { get; set; }

        /// <summary>
        /// The tooltip assigned to this marker.
        /// </summary>
        public Tooltip Tooltip { get; set; }

        /// <summary>
        /// The popup shown when the marker is clicked.
        /// </summary>
        public Popup Popup { get; set; }


        public delegate void EventHandler(Layer sender, Event e);

        EventHandler OnAdd;

        [JSInvokable]
        public void NotifyAdd(Event eventArgs)
        {
            OnAdd?.Invoke(this, eventArgs);
        }

        EventHandler OnRemove;

        [JSInvokable]
        public void NotifyRemove(Event eventArgs)
        {
            OnRemove?.Invoke(this, eventArgs);
        }

        public delegate void PopupEventHandler(Layer sender, PopupEvent e);

        PopupEventHandler OnPopupOpen;

        [JSInvokable]
        public void NotifyPopupOpen(PopupEvent eventArgs)
        {
            OnPopupOpen?.Invoke(this, eventArgs);
        }

        PopupEventHandler OnPopupClose;

        [JSInvokable]
        public void NotifyPopupClose(PopupEvent eventArgs)
        {
            OnPopupClose?.Invoke(this, eventArgs);
        }

        public delegate void TooltipEventHandler(Layer sender, TooltipEvent e);

        TooltipEventHandler OnTooltipOpen;

        [JSInvokable]
        public void NotifyTooltipOpen(TooltipEvent eventArgs)
        {
            OnTooltipOpen?.Invoke(this, eventArgs);
        }

        TooltipEventHandler OnTooltipClose;

        [JSInvokable]
        public void NotifyTooltipClose(TooltipEvent eventArgs)
        {
            OnTooltipClose?.Invoke(this, eventArgs);
        }

        protected Layer()
        {
            Id = StringHelper.GetRandomString(20);
            DotNetRef = DotNetObjectReference.Create(this);
        }

        public virtual async ValueTask DisposeAsync()
        {
            DotNetRef.Dispose();
            await JsRef.DisposeAsync();

            OnAdd = null;
            OnRemove = null;
            OnPopupOpen = null;
            OnPopupClose = null;
            OnTooltipOpen = null;
            OnTooltipClose = null;
        }
        
        
    }
}


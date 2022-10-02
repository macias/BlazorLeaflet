using Microsoft.JSInterop;

namespace BlazorLeaflet
{
    public interface IJsHooked
    {
        IJSObjectReference JsRef { get; }
    }
}

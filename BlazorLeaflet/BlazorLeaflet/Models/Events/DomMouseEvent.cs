using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BlazorLeaflet.Models.Events
{
    public sealed class DomMouseEvent
    {
        public bool AltKey { get; set; }
        public bool CtrlKey { get; set; }
        public bool MetaKey { get; set; }
        public bool ShiftKey { get; set; }
    }
    
   
}

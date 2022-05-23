using System.Drawing;

namespace BlazorLeaflet.Models
{
    public class DivIcon : Icon
    {
        public Point? BgPos { get; set; }
        public string Html { get; set; }

        public DivIcon()
        {
            
        }
    }
}

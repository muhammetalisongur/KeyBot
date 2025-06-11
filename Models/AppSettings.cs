using System.Collections.Generic;

namespace KeyBot.Models
{
    public class AppSettings
    {
        public bool IsSingleKeyMode { get; set; } = true;
        public bool IsMouseMode { get; set; } = false;
        public string SelectedKey { get; set; } = "Space";
        public string SelectedMouse { get; set; } = "Sol Tık";
        public decimal Interval { get; set; } = 1.0m;
        public int RepeatCount { get; set; } = 10;
        public bool IsInfinite { get; set; } = false;
        public List<KeySequenceItem> KeySequence { get; set; } = new List<KeySequenceItem>();
    }
} 
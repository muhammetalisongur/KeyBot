namespace KeyBot.Models
{
    public class KeySequenceItem
    {
        public string KeyName { get; set; } = "";
        public decimal Delay { get; set; }
        public int? MouseX { get; set; }
        public int? MouseY { get; set; }
        
        public override string ToString()
        {
            if (MouseX.HasValue && MouseY.HasValue)
            {
                return $"{KeyName} ({MouseX}, {MouseY}) ({Delay}s)";
            }
            return $"{KeyName} ({Delay}s)";
        }
    }
} 
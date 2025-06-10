namespace KeyBot.Models
{
    public class KeySequenceItem
    {
        public string KeyName { get; set; } = "";
        public decimal Delay { get; set; }
        
        public override string ToString()
        {
            return $"{KeyName} ({Delay}s)";
        }
    }
} 
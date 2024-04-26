namespace Msmp.Utility.Models
{
    internal class RandomDisplay
    {
        public int DisplayId { get; set; } 
        public int DisplaySlotId { get; set; }
        public Display Display { get; set; }
        public DisplaySlot DisplaySlot { get; set; }
    }
}

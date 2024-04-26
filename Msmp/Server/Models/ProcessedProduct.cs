using System;

namespace Msmp.Server.Models
{
    [Serializable]
    internal class ProcessedProduct
    {
        public bool IsProductDisplayed { get; set; }
        public int ProductId { get; set; }
        public int DisplaySlotId { get; set; }
        public int PurchaseChance { get; set; }
        public int ExpensiveRandom { get; set; }
        public int RandomMultiplier { get; set; }
        public int ProductCount { get; set; }
    }
}

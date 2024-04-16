using BepInEx.Logging;
using MyBox;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Msmp.Client.SynchronizationContainers
{
    internal class SyncDisplayContainer
    {
        public readonly List<DisplaySyncModel> Displays = new List<DisplaySyncModel>();
        private readonly ManualLogSource _logger;

        public bool Initialized { get; private set; }   

        public void InitializeDisplays()
        {
            if (Initialized)
            {
                return;
            }

            List<Display> displays = (List<Display>)(Singleton<DisplayManager>.Instance.GetType()
               .GetField("m_Displays", BindingFlags.NonPublic | BindingFlags.Instance)
               .GetValue(Singleton<DisplayManager>.Instance));

            foreach(var display in displays)
            {
                int displayId = displays.FindIndex(x => x == display);
                InitializeDisplay(displayId);
            }

            Initialized = true;
        }

        public SyncDisplayContainer(ManualLogSource logger)
        {
            _logger = logger;   
        }

        public void Add(DisplaySyncModel display)
        {
            Displays.Add(display);
        }

        public void InitializeDisplay(int displayId)
        {
            _logger.LogInfo($"[Client] [{nameof(SyncDisplayContainer)}] Initializing Display id: {displayId}");

            List<Display> displays = (List<Display>)(Singleton<DisplayManager>.Instance.GetType()
               .GetField("m_Displays", BindingFlags.NonPublic | BindingFlags.Instance)
               .GetValue(Singleton<DisplayManager>.Instance));

            if (displays == null)
            {
                _logger.LogInfo($"[Client] [{nameof(SyncDisplayContainer)}] Displays were null");
                return;
            }

            DisplaySlot[] slots = (DisplaySlot[])displays[displayId].GetType()
             .GetField("m_DisplaySlots", BindingFlags.NonPublic | BindingFlags.Instance)
             .GetValue(displays[displayId]);

            DisplaySyncModel displaySyncModel = new DisplaySyncModel()
            {
                DisplayId = displays.FindIndex(x => x == displays[displayId]),
            };
            
            for(int i = 0; i < slots.Length; i++)
            {
                displaySyncModel.Slots.Add(new DisplaySlotSynctModel()
                {
                    DisplaySlotId = i,
                    ProductCount = slots[i].Data.FirstItemCount,
                    ProductId = slots[i].Data.FirstItemID,
                });

                _logger.LogInfo($"Display Init: pId: {slots[i].Data.FirstItemID} pCount: {slots[i].Data.FirstItemCount}");
            }

            Displays.Add(displaySyncModel);
        }

        public void RemoveProduct(int displayId, int displaySlotId )
        {
            Displays.Find(x => x.DisplayId == displayId)?.Slots.Find(x => x.DisplaySlotId == displaySlotId);
        }

        public List<DisplaySyncModel> GetModels()
            => Displays;

        public void AddProduct(int displayId, int displaySlotId, int productId)
        {
            DisplaySyncModel display;
            if((display = Displays.Find(x => x.DisplayId == displayId)) == null)
            {
                InitializeDisplay(displayId);
            }

            if (display.Slots[displaySlotId].ProductId == 0)
            {
                display.Slots[displaySlotId].ProductId = productId;
            }

            display.Slots[displaySlotId].ProductCount++;
        }

        [Serializable]
        public class DisplaySyncModel
        {
            public List<DisplaySlotSynctModel> Slots { get; set; } = new List<DisplaySlotSynctModel>();
            public int DisplayId { get; set; }
            public int PriceTag { get; set; }  
        }

        [Serializable]
        public class DisplaySlotSynctModel
        {
            public int ProductCount { get; set; }
            public int ProductId { get; set; } = -1;
            public int DisplaySlotId { get; set; }    
        }
    }
}

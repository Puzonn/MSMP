using Msmp.Utility.Models;
using MyBox;
using System;
using System.Collections.Generic;

namespace Msmp.Utility
{
    internal static class DisplayUtility
    {
        /* TODO: Redo this mess */
        private static DisplayManager m_DisplayManager;
        private static DisplayManager _displayManager
        {
            get
            {
                if(m_DisplayManager == null)
                {
                    m_DisplayManager = Singleton<DisplayManager>.Instance;
                }

                return m_DisplayManager;
            }
        }

        public static RandomDisplay GetRandomDisplay()
        {
            DisplaySlot displaySlot = _displayManager.GetRandomDisplaySlot();
            Display display = displaySlot.Display;

            return new RandomDisplay()
            {
                Display = display,
                DisplaySlot = displaySlot,
                DisplayId = GetDisplayIndex(display),
                DisplaySlotId = GetDisplaySlotIndex(displaySlot),
            };
        }

        public static int GetDisplaySlotIndex(DisplaySlot displaySlot)
        {
            DisplaySlot[] slots = GetDisplaySlots(displaySlot.Display);
            return Array.IndexOf(slots, displaySlot);
        }

        public static DisplaySlot[] GetDisplaySlots(Display display)
        {
            return display.GetType().GetPrivateField<DisplaySlot[]>("m_DisplaySlots", display);
        }

        public static int GetDisplayIndex(Display display)
        {
            return Array.IndexOf(GetDisplays().ToArray(), display);
        }

        public static List<Display> GetDisplays() 
        {
            return _displayManager.GetType().GetPrivateField<List<Display>>("m_Displays", _displayManager);
        }

        public static DisplaySlot GetDisplaySlot(int displayId, int displaySlotId)
        {
            Display display = GetDisplays()[displayId];
            return GetDisplaySlots(display)[displaySlotId];
        }
    }
}

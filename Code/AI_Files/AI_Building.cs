using System;
using ColossalFramework;


namespace RealisticPopulationRevisited
{
    public class RealisticCitizenUnits
    {
        private readonly static CitizenManager citizenManager = Singleton<CitizenManager>.instance;


        /// <summary>
        /// Send this unit away to empty to requirements
        /// EmptyBuilding
        /// </summary>
        /// <param name="data"></param>
        /// <param name="citizenNumber"></param>
        internal static void RemoveHouseHold(ref Building data, int maxHomes)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            CitizenUnit[] citizenUnitArray = instance.m_units.m_buffer;

            int loopCounter = 0;
            uint previousUnit = data.m_citizenUnits;
            uint currentUnit = data.m_citizenUnits;

            while (currentUnit != 0u)
            {
                // If this unit matches what we one, send the citizens away or remove citzens
                uint nextUnit = citizenUnitArray[currentUnit].m_nextUnit;
                bool removeCurrentUnit = false;

                // Only think about removing if it matches the flag
                if ((ushort)(CitizenUnit.Flags.Home & citizenUnitArray[currentUnit].m_flags) != 0)
                {
                    if (maxHomes > 0)
                    {
                        maxHomes--;
                    }
                    else
                    {
                        // Remove excess citizens
                        for (int i = 0; i < 5; i++)
                        {
                            // CommonBuildingAI.RemovePeople() -> CitizenManager.ReleaseUnitImplementation()
                            uint citizen = citizenUnitArray[(int)((UIntPtr)currentUnit)].GetCitizen(i);
                            citizenManager.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_homeBuilding = 0;
                        } // end for
                        removeCurrentUnit = true;
                    } // end if - above count
                } // Flag match

                // Don't need to worry about trying to remove the initial citizen unit. 
                // This should always exist and other code will always force at least one.
                if (removeCurrentUnit)
                {
                    // Link previous unit to next unit and release the item
                    citizenUnitArray[previousUnit].m_nextUnit = nextUnit;

                    ReversePatches.ReleaseUnitImplementation(instance, currentUnit, ref citizenUnitArray[currentUnit]);
                    instance.m_unitCount = (int)(instance.m_units.ItemCount() - 1);
                    // Previous unit number has not changed
                }
                else
                {
                    // Current unit is not to be removed, proceed to next
                    previousUnit = currentUnit;
                }
                currentUnit = nextUnit;

                if (++loopCounter > 524288)
                {
                    currentUnit = 0u; // Bail out loop
                }
            } // end while
        } // end RemoveHouseHold
    } // end AI_Building
}

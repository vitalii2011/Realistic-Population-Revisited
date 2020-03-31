using System;
using ColossalFramework;
using UnityEngine;
using Harmony;


namespace RealisticPopulationRevisited
{
    [HarmonyPatch(typeof(CommonBuildingAI))]
    [HarmonyPatch("HandleCrime")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(Building), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal })]
    class RealisticCrime
    {
        static bool Prefix(ref CommonBuildingAI __instance, ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            int number = 10;
            if (crimeAccumulation != 0)
            {
                ItemClass ic = data.Info.m_class;
                int percentage = (int) ((ic.m_level >= 0) ? ic.m_level : 0);

                // TODO -  Should be replaced by height of building??
                // Values to be determined and will not be added to XML yet
                if (ic.m_service == ItemClass.Service.Residential)
                {
                    number = 16;

                    if (ic.m_subService == ItemClass.SubService.ResidentialHigh)
                    {
                        crimeAccumulation /= 2;
                    }
                }
                else if (ic.m_service == ItemClass.Service.Office)
                {
                    crimeAccumulation /= 5; // Not enough?
                }
                else if (ic.m_subService == ItemClass.SubService.CommercialHigh)
                {
                    crimeAccumulation /= 3;
                }

                // Percentage reduction
                crimeAccumulation = (crimeAccumulation * (number - percentage)) / number;

                // ----- End of changes -----

                if (Singleton<SimulationManager>.instance.m_isNightTime)
                {
                    // crime multiplies by 1.25 at night
                    crimeAccumulation = crimeAccumulation * 5 >> 2;
                }

                if (data.m_eventIndex != 0)
                {
                    EventManager instance = Singleton<EventManager>.instance;
                    EventInfo info = instance.m_events.m_buffer[(int)data.m_eventIndex].Info;
                    crimeAccumulation = info.m_eventAI.GetCrimeAccumulation(data.m_eventIndex, ref instance.m_events.m_buffer[(int)data.m_eventIndex], crimeAccumulation);
                }
                crimeAccumulation = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)crimeAccumulation);
                if (!Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment))
                {
                    crimeAccumulation = 0;
                }
            }
            data.m_crimeBuffer = (ushort)Mathf.Min(citizenCount * 100, (int)data.m_crimeBuffer + crimeAccumulation);
//Debugging.writeDebugToFile(data.Info.gameObject.name + " -> number:" + citizenCount + ", crime_buffer: " + data.m_crimeBuffer + " + " + crimeAccumulation);
            int crimeBuffer = (int)data.m_crimeBuffer;
            if (citizenCount != 0 && crimeBuffer > citizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
            {
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                CalculateGuestVehicles(ref __instance, buildingID, ref data, TransferManager.TransferReason.Crime, ref num, ref num2, ref num3, ref num4);
                if (num == 0)
                {
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = crimeBuffer / Mathf.Max(1, citizenCount * 10);
                    offer.Building = buildingID;
                    offer.Position = data.m_position;
                    offer.Amount = 1;
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Crime, offer);
                }
            }
            Notification.Problem problem = Notification.RemoveProblems(data.m_problems, Notification.Problem.Crime);
            if ((int)data.m_crimeBuffer > citizenCount * 90)
            {
                problem = Notification.AddProblems(problem, Notification.Problem.Crime | Notification.Problem.MajorProblem);
            }
            else if ((int)data.m_crimeBuffer > citizenCount * 60)
            {
                problem = Notification.AddProblems(problem, Notification.Problem.Crime);
            }
            data.m_problems = problem;

            // Don't execute base method after this.
            return false;
        }


        // Copied from vanilla.  Clunky, but temporary.  Want to avoid reflection and its performance overhead, and Harmony 1.2 doesn't have reverse redirection.
        // TODO - Revist this and remove.
        static void CalculateGuestVehicles(ref CommonBuildingAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            do
            {
                if (num == 0)
                {
                    return;
                }
                if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType == material)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                    info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out int size, out int max);
                    cargo += Mathf.Min(size, max);
                    capacity += max;
                    count++;
                    if ((instance.m_vehicles.m_buffer[num].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != 0)
                    {
                        outside++;
                    }
                }
                num = instance.m_vehicles.m_buffer[num].m_nextGuestVehicle;
            }
            while (++num2 <= 16384);
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
        }

    }
}

﻿using System;
using System.Runtime.CompilerServices;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    ///  Harmony reverse patches.
    /// </summary>
    [HarmonyPatch]
    public static class ReversePatches
    {
        /// <summary>
        /// Reverse patch for ResidentAI.FinishSchoolOrWork to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="citizenID">ID of this citizen (for game method)</param>
        /// <param name="data">Citizen data (for game method)</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(BuildingAI)), "EnsureCitizenUnits")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void EnsureCitizenUnits(object instance, ushort buildingID, ref Building data, int homeCount, int workCount, int visitCount, int studentCount)
        {
            string message = "EnsureCitizenUnits reverse Harmony patch wasn't applied";
            Logging.Error(message, instance.ToString(), buildingID.ToString(), data.ToString(), homeCount.ToString(), workCount.ToString(), visitCount.ToString(), studentCount.ToString());
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Reverse patch for ResidentAI.FinishSchoolOrWork to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="citizenID">ID of this citizen (for game method)</param>
        /// <param name="data">Citizen data (for game method)</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(CitizenManager)), "ReleaseUnitImplementation")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReleaseUnitImplementation(object instance, uint unit, ref CitizenUnit data)
        {
            string message = "ReleaseUnitImplementation reverse Harmony patch wasn't applied";
            Logging.Error(message, instance.ToString(), unit.ToString(), data.ToString());
            throw new NotImplementedException(message);
        }
    }
}
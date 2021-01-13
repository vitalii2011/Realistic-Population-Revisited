using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

    
namespace RealisticPopulationRevisited
{

    /// <summary>
    /// Transpiler to patch CommonBuildingAI.HandleCrime.
    /// This implements the mods custom crime settings.
    /// </summary>
    [HarmonyPatch(typeof(CommonBuildingAI))]
    [HarmonyPatch("HandleCrime")]
    public static class HandleCrimeTranspiler
    {

        // Inserting a call to our custom replacement method after these instructions (normally at the start of the method):
        /* // if (crimeAccumulation != 0)
         * IL_0000: ldarg.3
         * IL_0001: brfalse IL_00a3
         */

        // The following code is inserted for the call "crimeAccumulation = MultiplyCrime();" (crimeAccumulation is argument 3 of HandleCrime)
        /* call HandleCrimeTranspiler.RealisticCrime
         * starg.s 3
        */

        /// <summary>
        /// StartConnectionTransferImpl transpiler.
        /// </summary>
        /// <param name="original">Original method to patch</param>
        /// <param name="instructions">Original ILCode</param>
        /// <param name="generator">ILCode generator</param>
        /// <returns>Replacement ILCode</returns>
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Transpiler meta.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            bool inserted = false;


            Debugging.Message("transpiling CommonBuildingAI.HandleCrime");

            // Iterate through ILCode instructions.
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction and add it to output.
                CodeInstruction instruction = instructionsEnumerator.Current;
                yield return instruction;

                // If we haven't inserted our code yet, we need to check to see if we're at the right spot to do so.
                if (!inserted)
                {
                    // Haven't yet patched.  Check to see if we have a pattern match - looking for ldarg.3 followed by brfalse.
                    if (instruction.opcode == OpCodes.Ldarg_3)
                    {
                        // Got an  ldarg.3 - add it to output and then check for a following brfalse.
                        instructionsEnumerator.MoveNext();
                        instruction = instructionsEnumerator.Current;
                        yield return instruction;
                        
                        if (instruction.opcode == OpCodes.Brfalse)
                        {
                            // Found our spot! Insert our custom instructions in the output.
                            Debugging.Message("inserting custom method call after ldarg.3, brfalse");

                            // Add call to patch method.
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HandleCrimeTranspiler), nameof(HandleCrimeTranspiler.RealisticCrime)));

                            // Store patch method return value as crimeAccumulation (argument 3).
                            yield return new CodeInstruction(OpCodes.Starg_S, 3);

                            // Set flag to indicate that insertion is complete.
                            inserted = true;
                        }
                    }
                }
            }

            Debugging.Message("finished HandleCrime tranpiler");
        }


        public static int RealisticCrime()
        {
            return 1000;
        }
    }
}
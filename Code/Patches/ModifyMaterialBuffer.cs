using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to fix a game bug whereby incoming goods amounts (uint16) can overflow and wrap-around.
    /// </summary>
    [HarmonyPatch(typeof(CommercialBuildingAI), nameof(CommercialBuildingAI.ModifyMaterialBuffer))]
    public static class ModifyMaterialBufferFix
    {
        /// <summary>
        /// Harmony transpiler for CommercialBuildingAI.ModifyMaterialBuffer, to insert a custom call to fix a game bug (no bounds check on uint16).
        /// </summary>
        /// <param name="original">Original method</param>
        /// <param name="instructions">Original ILCode</param>
        /// <param name="generator">IL generator</param>
        /// <returns>Patched ILCode</returns>
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            /* Inserting a call to our custom buffer overflow check method, so:
			 * amountDelta = Mathf.Clamp(amountDelta, 0, num3 - customBuffer2)
			 * Becomes:
			 * amountDelta = BufferOverflowCheck(Mathf.Clamp(amountDelta, 0, num3 - customBuffer2), customBuffer2);
			 * 
			 * This will be inserted at:
			 * sub
			 * call int32 [UnityEngine]UnityEngine.Mathf::Clamp(int32, int32, int32)
			 * 
			 * [insert here]
			 * 
			 * stind.i4
			 */

            // ILCode local variable indexes.
            const int CustomBuffer2VarIndex = 7;


            // Status flag.
            bool isPatched = false;


            // Instruction parsing.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            CodeInstruction instruction;

            // Iterate through all instructions in original method.
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction and add it to output.
                instruction = instructionsEnumerator.Current;
                yield return instruction;

                // Looking for possible precursor calls to "call Math.Max".
                if (!isPatched && instruction.opcode == OpCodes.Sub)
                {
                    // Found sub - are there following instructions?
                    if (instructionsEnumerator.MoveNext())
                    {
                        // Yes - get the next instruction.
                        instruction = instructionsEnumerator.Current;
                        yield return instruction;

                        // Is this new instruction a call to Math.Clamp?
                        if (instruction.opcode == OpCodes.Call && instruction.operand.ToString().Equals("Int32 Clamp(Int32, Int32, Int32)"))
                        {
                            // Yes - insert call to BufferOverflowCheck.
                            Logging.KeyMessage("transpiler adding call to BufferOverflowCheck in amountDelta = Mathf.Clamp(amountDelta, 0, num3 - customBuffer2)");
                            yield return new CodeInstruction(OpCodes.Ldloc_S, CustomBuffer2VarIndex);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModifyMaterialBufferFix), nameof(ModifyMaterialBufferFix.BufferOverflowCheck)));

                            // Set flag.
                            isPatched = true;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Perfoms a uint16 buffer overflow check to prevent incoming goods overflows.
        /// </summary>
        /// <param name="amountDelta">Incoming goods load amount</param>
        /// <param name="customBuffer">Current storage buffer amount</param>
        /// <returns>Incoming goods load amount reduced to prevent any overflows</returns>
        public static int BufferOverflowCheck(int amountDelta, int customBuffer)
        {
            if (customBuffer + amountDelta > 65500)
            {
                Logging.Message("caught incoming commercial goods overflow");
                amountDelta = 65500 - customBuffer;
            }

            return amountDelta;
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
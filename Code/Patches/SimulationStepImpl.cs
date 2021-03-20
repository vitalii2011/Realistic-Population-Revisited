// CommercialBuildingAI
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;


namespace RealPop2
{
	/// <summary>
	/// Hamony patch to cap commercial building incoming goods demand.
	/// </summary>
	[HarmonyPatch]
	[HarmonyPatch(typeof(CommercialBuildingAI), "SimulationStepActive")]
	public static class SimStepPatch
	{
		/// <summary>
		/// Harmony transpiler for CommercialBuildingAI.SimulationStep, to insert upper limit to perceived goods requirement.
		/// </summary>
		/// <param name="original">Original method</param>
		/// <param name="instructions">Original ILCode</param>
		/// <param name="generator">IL generator</param>
		/// <returns>Patched ILCode</returns>
		public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			/* Changing:
			 * int num6 = Mathf.Max(num5, num2 * 4);
			 * To:
			 * int num6 = Mathf.Min(Mathf.Max(num5, num2 * 4), MaxGoodsDemand;
			 * 
			 * This ensures that outstanding goods demand is capped at MaxGoodsDemand, so the building won't order more goods beyond that point.
			 * 
			 * Finding this is easy, as it's the only call in this method (of any kind) immediately after a mul.
			 */


			// Maximum goods demand level.
			const int MaxGoodsDemand = 48000;

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
				if (!isPatched && instruction.opcode == OpCodes.Mul)
				{
					// Found mul - are there following instructions?
					if (instructionsEnumerator.MoveNext())
					{
						// Yes - get the next instruction.
						instruction = instructionsEnumerator.Current;
						yield return instruction;

						// Is this new instruction a call to int32 Math.Max?

						if (instruction.opcode == OpCodes.Call && instruction.operand.ToString().Equals("Int32 Max(Int32, Int32)"))
						{
							// Yes - insert call to new Math.Min(x, MaxGoodsDemand) after original call.
							Logging.KeyMessage("transpiler adding MaxGoodsDemand of ", MaxGoodsDemand.ToString(), " after Int32 Max(Int32, Int32)");
							yield return new CodeInstruction(OpCodes.Ldc_I4, MaxGoodsDemand);
							yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Min), new Type[] { typeof(int), typeof(int) }));

							// Set flag.
							isPatched = true;
						}
					}
				}
			}
		}
	}
}

using HarmonyLib;

using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;

using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RealPop2
{
    /// <summary>
    /// Harmony Postfix patch to add vbu.
    /// </summary>
    [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "UpdateBindings")]
    public static class InfoPanelPatch
    {
        public static UILabel visitLabel;

        public static void Postfix()
        {
            ushort building = WorldInfoPanel.GetCurrentInstanceID().Building;

            if (visitLabel == null)
            {
                ZonedBuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);
                visitLabel = UIControls.AddLabel(infoPanel.component, 100f, 280f, "Visitors", textScale: 0.625f);
                visitLabel.textColor = new UnityEngine.Color32(206, 248, 0, 255);
                visitLabel.font = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Regular");

                UIComponent workerLabel = infoPanel.Find("HighlyEducatedWorkers");
                if (workerLabel != null)
                {
                    visitLabel.absolutePosition = new Vector2(workerLabel.absolutePosition.x - visitLabel.width, workerLabel.absolutePosition.y + 28f);
                }

            }

            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[building].Info;
            CommercialBuildingAI comAI = buildingInfo.GetAI() as CommercialBuildingAI;

            if (comAI == null)
            {
                visitLabel.Hide();
            }
            else
            {
                visitLabel.Show();
                int aliveCount = 0, totalCount = 0;
                Citizen.BehaviourData behaviour =new Citizen.BehaviourData();
                GetVisitBehaviour(comAI, building, ref buildingBuffer[building], ref behaviour, ref aliveCount, ref totalCount);
                visitLabel.text = "Visitors " + totalCount + " / " + comAI.CalculateVisitplaceCount((ItemClass.Level)buildingBuffer[building].m_level, new ColossalFramework.Math.Randomizer(building), buildingBuffer[building].Width, buildingBuffer[building].Length).ToString();
            }
        }


        [HarmonyReversePatch]
        [HarmonyPatch((typeof(CommonBuildingAI)), "GetVisitBehaviour")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GetVisitBehaviour(CommonBuildingAI instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount)
        {
            string message = "GetVisitBehaviour reverse Harmony patch wasn't applied";
            Logging.Error(message, instance.ToString(), buildingID.ToString(), buildingData.ToString(), behaviour.ToString(), aliveCount.ToString(), totalCount.ToString());
            throw new NotImplementedException(message);
        }
    }
}
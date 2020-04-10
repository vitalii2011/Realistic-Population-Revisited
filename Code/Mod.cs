using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;


namespace RealisticPopulationRevisited
{
    public class PopBalanceMod : IUserMod
    {
        public static string version = "1.1.1";

        public string Name => "Realistic Population Revisited " + version;
        
        public string Description => "More realistic building populations (based on building size) and utility needs.";
    }
}

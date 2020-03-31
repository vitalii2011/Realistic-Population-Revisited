using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;


namespace WG_BalancedPopMod
{
    public class PopBalanceMod : IUserMod
    {
        public string Name
        {
            get { return "Realistic Population Revisited"; }
        }
        public string Description
        {
            get { return "More realistic building populations (based on building size) and utility needs."; }
        }
    }
}

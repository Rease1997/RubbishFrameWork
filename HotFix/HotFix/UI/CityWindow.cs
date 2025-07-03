using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix
{
    class CityWindow : Window
    {
        public override void Awake(object param1 = null, object param2 = null, object param3 = null)
        {
            Debug.Log("CityWindow  Awake");
            Debug.Log("1111111111111111111");
        }

        public override void OnShow(object param1 = null, object param2 = null, object param3 = null)
        {
        }
    }
}

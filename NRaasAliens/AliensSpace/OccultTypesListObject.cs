using Sims3.Gameplay.Abstracts;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRaas.AliensSpace
{
    public class OccultTypesListObject : GameObject
    {
        public readonly List<OccultTypes> mList;

        public OccultTypesListObject(List<OccultTypes> list)
        {
            mList = list;
        }
    }
}

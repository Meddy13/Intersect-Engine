﻿/*
    Intersect Game Engine (Server)
    Copyright (C) 2015  JC Snider, Joe Bridges
    
    Website: http://ascensiongamedev.com
    Contact Email: admin@ascensiongamedev.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
using System;
using System.Collections.Generic;
using System.Threading;

namespace Intersect_Client.Classes
{
    public class Projectile : Entity
    {
        public int ProjectileNum = 0;
        public int Target = 0;

        public Projectile() : base()
        {
            Vital[(int)Enums.Vitals.Health] = 1;
            MaxVital[(int)Enums.Vitals.Health] = 1;
            HideName = 1;
            Passable = 1;
        }

        public void Load(ByteBuffer bf)
        {
            base.Load(bf);
            ProjectileNum = bf.ReadInteger();
            Stat[(int)Enums.Stats.Speed] = bf.ReadInteger();
            Dir = bf.ReadInteger();
            Target = bf.ReadInteger();
        }
    }
}
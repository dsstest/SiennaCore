﻿/*
 * Copyright (C) 2011 APS
 *	http://AllPrivateServer.com
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Common
{
    public class Unsigned7BitAttribute : ISerializableFieldAttribute
    {
        public Unsigned7BitAttribute(int Index)
            : base(Index)
        {

        }

        public override Type GetSerializableType()
        {
            return typeof(Unsigned7BitField);
        }
    }

    [Serializable]
    public class Unsigned7BitField : ISerializableField
    {
        public override void Deserialize(ref PacketInStream Data)
        {
            val = Data.ReadEncoded7Bit();
        }

        public override bool Serialize(ref PacketOutStream Data, bool Force)
        {
            if (!Force && (val == null || val.ToString() == "0"))
                return false;

            Data.WriteEncoded7Bit((long)(Convert.ChangeType(val,typeof(long))));
            return true;
        }

        public override void ApplyToFieldInfo(FieldInfo Info, ISerializablePacket Packet, Type Field)
        {
            Info.SetValue(Packet, Convert.ChangeType(val,Info.FieldType));
        }
    }
}

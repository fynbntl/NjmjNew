﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ETModel;

namespace ETHotfix
{
    public static class RoomComponentSystem
    {
        /// <summary>
        /// 根据id获取房间
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Room Get(this RoomComponent self, long id)
        {
            Room room;
            self.rooms.TryGetValue(id, out room);
            return room;
        }

        public static void Add(this RoomComponent self, Room room)
        {
            self.rooms.Add(room.Id, room);
            self.idleRooms.Add(room);
        }

        /// <summary>
        /// 从空闲集合中随机返回一个列表
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Room GetIdleRoom(this RoomComponent self)
        {
            if (self.IdleRoomCount > 0)
            {
                Room room = self.idleRooms[RandomHelper.RandomNumber(0, self.idleRooms.Count)];
                return room;
            }
            else
            {
                return null;
            }
        }
    }
}
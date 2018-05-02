﻿using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class LocationProxyComponentSystem : AwakeSystem<LocationProxyComponent>
	{
		public override void Awake(LocationProxyComponent self)
		{
			self.Awake();
		}
	}

	public static class LocationProxyComponentEx
	{
		public static void Awake(this LocationProxyComponent self)
		{
			StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
			self.AppId = startConfigComponent.StartConfig.AppId;

			StartConfig startConfig = startConfigComponent.LocationConfig;
			self.LocationAddress = startConfig.GetComponent<InnerConfig>().IPEndPoint;
		}

		public static async Task Add(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectAddRequest() { Key = key, AppId = self.AppId });
		}

		public static async Task Lock(this LocationProxyComponent self, long key, int time = 1000)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectLockRequest() { Key = key, LockAppId = self.AppId, Time = time });
		}

		public static async Task UnLock(this LocationProxyComponent self, long key, int value)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectUnLockRequest() { Key = key, UnLockAppId = self.AppId, AppId = value});
		}

		public static async Task Remove(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectRemoveRequest() { Key = key });
		}

		public static async Task<int> Get(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			ObjectGetResponse response = (ObjectGetResponse)await session.Call(new ObjectGetRequest() { Key = key });
			return response.AppId;
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ETModel;
using MongoDB.Bson.Serialization;
using NLog;

namespace App
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
		    // 异步方法全部会回掉到主线程
		    SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);

            try
			{
				Game.EventSystem.Add(DLLType.Model, typeof(Game).Assembly);
				Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());

				Options options = Game.Scene.AddComponent<OptionComponent, string[]>(args).Options;
				StartConfig startConfig = Game.Scene.AddComponent<StartConfigComponent, string, int>(options.Config, options.AppId).StartConfig;

				if (!options.AppType.Is(startConfig.AppType))
				{
					Log.Error("命令行参数apptype与配置不一致");
					return;
				}

				IdGenerater.AppId = options.AppId;

				LogManager.Configuration.Variables["appType"] = startConfig.AppType.ToString();
				LogManager.Configuration.Variables["appId"] = startConfig.AppId.ToString();
				LogManager.Configuration.Variables["appTypeFormat"] = $"{startConfig.AppType,-8}";
				LogManager.Configuration.Variables["appIdFormat"] = $"{startConfig.AppId:D3}";

				Log.Info($"server start........................ {startConfig.AppId} {startConfig.AppType}");

				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent>();

				// 根据不同的AppType添加不同的组件
				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				ClientConfig clientConfig = startConfig.GetComponent<ClientConfig>();
				
				switch (startConfig.AppType)
				{
					case AppType.Manager:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<ActorManagerComponent>();
						break;
					case AppType.Realm:
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						Game.Scene.AddComponent<ActorManagerComponent>();
					    Game.Scene.AddComponent<DBProxyComponent>();
                        Game.Scene.AddComponent<ConfigComponent>();
                        break;
					case AppType.Gate:
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						Game.Scene.AddComponent<ActorManagerComponent>();

                        //GateGlobalComponent
					    Game.Scene.AddComponent<DBComponent>();
					    Game.Scene.AddComponent<ConfigComponent>();
                        Game.Scene.AddComponent<DBProxyComponent>();
                        Game.Scene.AddComponent<RankDataComponent>();
					    Game.Scene.AddComponent<HttpComponent>();
					    Game.Scene.AddComponent<UserComponent>();
					    Game.Scene.AddComponent<NjmjGateSessionKeyComponent>();
                        break;
					case AppType.Location:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationComponent>();
						Game.Scene.AddComponent<ActorManagerComponent>();
						break;
					case AppType.DB:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
					    Game.Scene.AddComponent<DBComponent>();
					    Game.Scene.AddComponent<DBProxyComponent>();
					    Game.Scene.AddComponent<DBCacheComponent>();
                        break;
					case AppType.Map:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<UnitComponent>();
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<ServerFrameComponent>();
						Game.Scene.AddComponent<ActorManagerComponent>();
                        //MapGlobalCoponent
                        Game.Scene.AddComponent<DBProxyComponent>();
					    Game.Scene.AddComponent<DBComponent>();
                        Game.Scene.AddComponent<ConfigComponent>();
                        Game.Scene.AddComponent<RoomComponent>();
                        break;
					case AppType.AllServer:
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<UnitComponent>();
						Game.Scene.AddComponent<DBComponent>();
						Game.Scene.AddComponent<DBProxyComponent>();
						Game.Scene.AddComponent<DBCacheComponent>();
						Game.Scene.AddComponent<LocationComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						Game.Scene.AddComponent<ConfigComponent>();
						Game.Scene.AddComponent<ServerFrameComponent>();
						Game.Scene.AddComponent<ActorManagerComponent>();

                        //GateGlobalComponent
					    Game.Scene.AddComponent<RankDataComponent>();
//					    Game.Scene.AddComponent<HttpComponent>();
                        Game.Scene.AddComponent<UserComponent>();
					    Game.Scene.AddComponent<NjmjGateSessionKeyComponent>(); 

                        //MapGlobalCoponent
                        Game.Scene.AddComponent<RoomComponent>();

                        break;
					case AppType.Benchmark:
						Game.Scene.AddComponent<NetOuterComponent>();
						Game.Scene.AddComponent<BenchmarkComponent, IPEndPoint>(clientConfig.IPEndPoint);
						break;
					default:
						throw new Exception($"命令行参数没有设置正确的AppType: {startConfig.AppType}");
				}

			    Log.Info((3 << 1) +"");

                while (true)
				{
					try
					{
					    Thread.Sleep(1);
					    OneThreadSynchronizationContext.Instance.Update();
					    Game.EventSystem.Update();
                    }
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
    }
}

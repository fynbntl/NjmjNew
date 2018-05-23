﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ETModel;

namespace ETHotfix
{
    public static class RoomSystem
    {
        public static void Broadcast(this Room self, IActorMessage message)
        {
            foreach (Gamer gamer in self.gamers)
            {
                if (gamer == null || gamer.isOffline)
                {
                    continue;
                }

                ActorProxy actorProxy = gamer.GetComponent<UnitGateComponent>().GetActorProxy();
                actorProxy.Send(message);
            }
        }

        public static void GamerBroadcast(this Room self, Gamer gamer, IActorMessage message)
        {
            if (gamer == null || gamer.isOffline)
            {
                return;
            }

            ActorProxy actorProxy = gamer.GetComponent<UnitGateComponent>().GetActorProxy();
            actorProxy.Send(message);
        }

        public static void BroadGamerEnter(this Room self)
        {
            List<GamerInfo> Gamers = new List<GamerInfo>();
            foreach (var item in self.seats)
            {
                GamerInfo gamerInfo = new GamerInfo();
                gamerInfo.UserID = item.Key;
                gamerInfo.SeatIndex = item.Value;
                Gamer temp = self.Get(item.Key);
                gamerInfo.IsReady = temp.IsReady;
                Gamers.Add(gamerInfo);
            }

            self.Broadcast(new Actor_GamerEnterRoom()
            {
                Gamers = Gamers
            });
        }

        /// <summary>
        /// 超时10s自动出牌
        /// </summary>
        /// <param name="self"></param>
        public static async void StartTime(this Room self)
        {
            if (self.tokenSource != null)
            {
                self.tokenSource.Cancel();
            }

            self.tokenSource = new CancellationTokenSource();
            OrderControllerComponent controllerComponent = self.GetComponent<OrderControllerComponent>();
            Gamer gamer = self.Get(controllerComponent.CurrentAuthority);

            if (gamer.isOffline)
            {
                self.TimeOut = 1;
            }
            else
            {
                self.TimeOut = 10;
            }
            await Game.Scene.GetComponent<TimerComponent>().WaitAsync(self.TimeOut * 1000, self.tokenSource.Token);

            if (!self.tokenSource.IsCancellationRequested)
            {
                self.IsTimeOut = true;
                Log.Debug("超时");
                //超时自动出牌

                if (gamer == null)
                {
                    return;
                }

                gamer.GetComponent<HandCardsComponent>().PopCard();
               
            }
            else
            {
                self.IsTimeOut = false;
                Log.Debug("没有超时");
            }
        }

        public static async void StartOperateTime(this Room self)
        {
            if (self.tokenSource != null)
            {
                self.tokenSource.Cancel();
            }

            self.tokenSource = new CancellationTokenSource();
            await Game.Scene.GetComponent<TimerComponent>()
                .WaitAsync(self.OperationTimeOut * 1000, self.tokenSource.Token);

            if (!self.tokenSource.IsCancellationRequested)
            {
                self.IsTimeOut = true;
                //没有人碰刚
                self.IsOperate = false;
                foreach (var gamer in self.GetAll())
                {
                    gamer.IsCanGang = false;
                    gamer.IsCanGang = false;
                    gamer.IsCanHu = false;
                }

                self.GamerGrabCard();
                Log.Debug("OperateTime超时");
            }
            else
            {
                self.IsTimeOut = false;
                Log.Debug("OperateTime没有超时");
            }
        }


        public static bool CanHu(this Room self, MahjongInfo mahjongInfo, List<MahjongInfo> list)
        {
            List<MahjongInfo> temp = new List<MahjongInfo>(list);
            temp.Add(mahjongInfo);

            try
            {
                if (Logic_NJMJ.getInstance().isHuPai(temp))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error("胡牌错误:" + e);
                Log.Info(JsonHelper.ToJson(temp));
            }


            return false;
        }

        /// <summary>
        /// 发牌,包含抓牌和补花
        /// </summary>
        /// <param name="room"></param>
        public static async void GamerGrabCard(this Room room)
        {
            foreach (var gamer in room.GetAll())
            {
                gamer.isGangFaWanPai = false;
            }

            OrderControllerComponent orderController = room.GetComponent<OrderControllerComponent>();
            GameControllerComponent gameController = room.GetComponent<GameControllerComponent>();

            orderController.Turn();
            var currentGamer = room.Get(orderController.CurrentAuthority);
            HandCardsComponent cardsComponent = currentGamer.GetComponent<HandCardsComponent>();

            room.isGangEndBuPai = false;
            room.isGetYingHuaBuPai = false;
            var grabMahjong = GrabMahjong(room);

            while (grabMahjong.m_weight >= Consts.MahjongWeight.Hua_HongZhong)
            {
                room.Broadcast(new Actor_GamerBuHua()
                {
                    Uid = currentGamer.UserID,
                    weight = grabMahjong.weight
                });
                //从手牌中删除花牌
                Log.Info("补花");
                Logic_NJMJ.getInstance().RemoveCard(cardsComponent.GetAll(), grabMahjong);
                cardsComponent.FaceCards.Add(grabMahjong);

                //等待客户端显示
                await Game.Scene.GetComponent<TimerComponent>().WaitAsync(700);
                room.isGangEndBuPai = false;
                room.isGetYingHuaBuPai = true;
                grabMahjong = GrabMahjong(room);
                if (grabMahjong == null)
                {
                    //没牌
                    gameController.GameOver(0);
                    return;
                }
            }

            room.StartTime();
        }

        /// <summary>
        /// 抓牌
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static MahjongInfo GrabMahjong(this Room room)
        {
            try
            {
                OrderControllerComponent orderController = room.GetComponent<OrderControllerComponent>();
                var currentGamer = room.Get(orderController.CurrentAuthority);
                HandCardsComponent cardsComponent = currentGamer.GetComponent<HandCardsComponent>();
                DeskComponent deskComponent = room.GetComponent<DeskComponent>();


                if (deskComponent.RestLibrary.Count == 0)
                {
                    Log.Info("没牌了");
                    return null;
                }

                int number = RandomHelper.RandomNumber(0, deskComponent.RestLibrary.Count);
                MahjongInfo grabMahjong = deskComponent.RestLibrary[number];
                //发牌
                cardsComponent.AddCard(grabMahjong);
                deskComponent.RestLibrary.RemoveAt(number);
                room.my_lastMahjong = grabMahjong;
                Logic_NJMJ.getInstance().SortMahjong(cardsComponent.GetAll());

                //发送抓牌消息
                foreach (Gamer _gamer in room.gamers)
                {
                    if (_gamer == null || _gamer.isOffline)
                    {
                        continue;
                    }

                    ActorProxy actorProxy = _gamer.GetComponent<UnitGateComponent>().GetActorProxy();
                    Actor_GamerGrabCard actorGamerGrabCard;
                    if (_gamer.UserID == orderController.CurrentAuthority)
                    {
                        actorGamerGrabCard = new Actor_GamerGrabCard()
                        {
                            Uid = currentGamer.UserID,
                            weight = (int) grabMahjong.m_weight
                        };
                    }
                    else
                    {
                        actorGamerGrabCard = new Actor_GamerGrabCard()
                        {
                            Uid = currentGamer.UserID,
                        };
                    }

                    actorProxy.Send(actorGamerGrabCard);
                }

                //发完牌判断是否胡牌
                foreach (Gamer _gamer in room.gamers)
                {
                    if (_gamer.UserID == orderController.CurrentAuthority)
                    {
                        HandCardsComponent handCardsComponent = _gamer.GetComponent<HandCardsComponent>();

                        //判断胡
                        if (Logic_NJMJ.getInstance().isHuPai(handCardsComponent.GetAll()))
                        {
                            _gamer.IsCanHu = true;
                            Actor_GamerCanOperation canOperation = new Actor_GamerCanOperation();
                            canOperation.Uid = _gamer.UserID;
                            canOperation.OperationType = 2;
                            room.GamerBroadcast(_gamer, canOperation);
                        }

                        List<MahjongInfo> temp = new List<MahjongInfo>(handCardsComponent.GetAll());
                        Logic_NJMJ.getInstance().RemoveCard(temp, grabMahjong);

                        //暗杠
                        if (Logic_NJMJ.getInstance().isCanGang(grabMahjong, temp))
                        {
                            _gamer.IsCanGang = true;
                            Actor_GamerCanOperation canOperation = new Actor_GamerCanOperation();
                            canOperation.Uid = _gamer.UserID;
                            canOperation.OperationType = 1;
                            room.GamerBroadcast(_gamer, canOperation);
                        }
                    }
                }

                return grabMahjong;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }
    }
}
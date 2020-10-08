using Photon.Hive.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweepersPlugins
{
    public abstract class MineSweepersPlugin : PluginBase
    {
        protected Board board;
        protected TurnManager turnManager;
        protected ScoreManager scoreManager;

        int NumberOfPlayers = 2;
        int startDuration = 200;

        protected bool firstSafe = false;
        protected bool endOnExpload = false;

        public bool IsGameStarted { get; protected set; } = false;

        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            this.PluginHost.LogInfo(string.Format("OnCreateGame {0} by user {1}", info.Request.GameId, info.UserId));
            info.Continue(); // same as base.OnCreateGame(info);

            System.Collections.Hashtable hashtable = info.OperationRequest.Parameters[248] as System.Collections.Hashtable;

            int hight = (int)hashtable["hight"];
            int width = (int)hashtable["width"];
            NumberOfPlayers = (int)hashtable["maxPlayers"];
            firstSafe = (bool)hashtable["firstSafe"];
            endOnExpload = (bool)hashtable["endOnExpload"];


            float mineRate = 0.1f;
            if (hashtable.ContainsKey("mineRate"))
                mineRate = (float)(int)hashtable["mineRate"] / 100;

            board = new Board(hight, width, (int)(hight * width * mineRate));


            this.PluginHost.LogInfo(string.Format("GameCreated {0} by {1} with {2} mines", hight, width, (int)(hight * width * mineRate)));

        }
        
        public override void OnJoin(IJoinGameCallInfo info)
        {
            base.OnJoin(info);

            if (PluginHost.GameActors.Count > NumberOfPlayers)
                PluginHost.RemoveActor(info.ActorNr, "Room is full");
            if (IsGameStarted)
            {
                SyncPlayer(info);
            }
            else
            if (PluginHost.GameActors.Count >= NumberOfPlayers)
            {
                PluginHost.CreateOneTimeTimer(
                () => StartGame(),
                startDuration);
            }


        }

        private void SyncPlayer(IJoinGameCallInfo info)
        {
            scoreManager.AddPlayer(info.ActorNr, 0);

            PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.GameStart, new Dictionary<byte, object>() { { (byte)0, board.Hight }, { (byte)1, board.Width } }, CacheOperations.DoNotCache);

            foreach (Dictionary<byte, object> dic in board.AllReveald())
                PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.SyncFields, dic, CacheOperations.DoNotCache);

            foreach (Dictionary<byte, object> dic in board.AllFlags())
                PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.SyncFlags, dic, CacheOperations.DoNotCache);

        }

        public override void OnLeave(ILeaveGameCallInfo info)
        {
            base.OnLeave(info);
            if (IsGameStarted)
            {
                scoreManager.RemovePlayer(info.ActorNr);
            }
        }

        public abstract void StartGame();

        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            switch ((Event)info.Request.EvCode)
            {
                case Event.Move:
                    OnOpen(info);
                    info.Continue();
                    break;
                case Event.ToggleFlag:
                    OnToggleFlag(info);
                    info.Continue();
                    break;
                default:
                    info.Cancel();
                    break;
            }
        }

        protected abstract void OnOpen(IRaiseEventCallInfo info);

        protected abstract void OnToggleFlag(IRaiseEventCallInfo info);

        protected bool TryOpenRest(int y, int x)
        {
            if (board.GetTile(y, x) == board.NumOfFlags(y, x))
            {
                foreach (Dictionary<byte, object> dic in board.OpenRest(y, x))
                    BroadcastEvent((byte)Event.ReceiveMove, dic);
                return true;
            }
            return false;
        }

        protected void Expand(int y, int x)
        {
            foreach (Dictionary<byte, object> dic in board.Expand(y, x))
                BroadcastEvent((byte)Event.ReceiveMove, dic);

        }
    }
}

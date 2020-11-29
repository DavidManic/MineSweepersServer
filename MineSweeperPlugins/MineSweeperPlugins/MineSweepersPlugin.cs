using Photon.Hive.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
namespace MineSweepersPlugins
{
    /// <summary>
    /// basic implementation of MineSweepers plugins
    /// </summary>
    public abstract class MineSweepersPlugin : PluginBase
    {
        protected Board board;

        int NumberOfPlayers = 2;
        int startDuration = 200;

        protected bool firstSafe = false;
        protected bool endOnExpload = false;
        protected bool joinAfterStart = false;

        public bool IsGameStarted { get; protected set; } = false;

        /// <summary>
        /// Callback when room is created
        /// </summary>
        /// <param name="info"></param>
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
            joinAfterStart = (bool)hashtable["joinAfter"];



            float mineRate = 0.1f;
            if (hashtable.ContainsKey("mineRate"))
                mineRate = (float)(int)hashtable["mineRate"] / 100;
            int mineCount = (int)(hight * width * mineRate);
            board = new Board(hight, width, mineCount);



            this.PluginHost.LogInfo(string.Format("GameCreated {0} by {1} with {2} mines. FirstSafe {3}, endOnExpload {4}, JoinAfterStart {5}", hight, width, board.MineCount,firstSafe,endOnExpload,joinAfterStart));

        }
        /// <summary>
        /// Callback for player joining the room
        /// </summary>
        /// <param name="info"></param>
        public override void OnJoin(IJoinGameCallInfo info)
        {
            base.OnJoin(info);

            if (PluginHost.GameActors.Count > NumberOfPlayers) {
                PluginHost.RemoveActor(info.ActorNr, "Room is Full");
                return;
            }

            if (IsGameStarted)
            {
                if (!joinAfterStart)
                {
                    PluginHost.RemoveActor(info.ActorNr, "Game already started"); return;
                }
                PluginHost.CreateOneTimeTimer(
                    () => SyncPlayer(info),
                    200);
                

            }
            else
            {
                if ((PluginHost.GameActors.Count >= 2 && joinAfterStart) || (PluginHost.GameActors.Count >= NumberOfPlayers && !joinAfterStart))
                    PluginHost.CreateOneTimeTimer(
                    () => StartGame(),
                    startDuration);
            }
            
                


        }


        /// <summary>
        /// Sync player to current game state
        /// </summary>
        /// <param name="info"></param>
        protected virtual void SyncPlayer(IJoinGameCallInfo info)
        {
            PluginHost.LogError("Sync player" + info.ActorNr);
            
            PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.GameStart, new Dictionary<byte, object>() { { (byte)0, board.Hight }, { (byte)1, board.Width }, { 2, board.MineCount } }, CacheOperations.DoNotCache);

            foreach (Dictionary<byte, object> dic in board.AllReveald())
                PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.SyncFields, dic, CacheOperations.DoNotCache);

            foreach (Dictionary<byte, object> dic in board.AllFlags())
                PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.SyncFlags, dic, CacheOperations.DoNotCache);
        }

        public abstract void StartGame();

        public abstract void EndGame();
        protected abstract void OnOpen(IRaiseEventCallInfo info);

        protected abstract void OnToggleFlag(IRaiseEventCallInfo info);

        /// <summary>
        /// Callback for recived events
        /// </summary>
        /// <param name="info"></param>
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

        /// <summary>
        /// Open tile and send it over network
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <param name="val"></param>
        protected void OpenTile(int y, int x, int val)
        {
            if (!board.IsFlag(y, x))
            {
                if (val != 0)
                {
                    if (board.RevealdCount < 1 && firstSafe && val == 10)
                        val = board.SwapMine(y, x);
                    board.Reveal(y, x);
                    BroadcastEvent((byte)Event.ReceiveMove, new Dictionary<byte, object>() { { (byte)0, y }, { (byte)1, x }, { (byte)2, val } });
                    if (val == 10 && endOnExpload)
                        EndGame();
                }
                else
                    Expand(y, x);
            }
        }

        /// <summary>
        /// Open all tiles around if current tile has enough flags around
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        protected bool TryOpenRest(int y, int x)
        {
            if (board.GetTile(y, x) == board.NumOfFlags(y, x))
            {
                foreach (Dictionary<byte, object> dic in board.OpenRest(y, x))
                    if (dic.Count > 0)
                        BroadcastEvent((byte)Event.ReceiveMove, dic);
                    else
                        return false;//Clickd on tile that is already expended or there is no tiles to open near by
                return true;
            }
            return false;
        }

        //Expand all the tiles around
        protected void Expand(int y, int x)
        {
            foreach (Dictionary<byte, object> dic in board.Expand(y, x))
                BroadcastEvent((byte)Event.ReceiveMove, dic);

        }
    }
}

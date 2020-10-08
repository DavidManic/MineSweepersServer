using Photon.Hive.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace MineSweepersPlugins
{
    public class MineSweepersVersus : MineSweepersPlugin
    {

        public override string Name { get { return "MineSweepersVersus"; } }
       // public bool IsGameStarted { get; protected set; } = false;
        

        /*
        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            this.PluginHost.LogInfo(string.Format("OnCreateGame {0} by user {1}", info.Request.GameId, info.UserId));
            info.Continue(); // same as base.OnCreateGame(info);

            System.Collections.Hashtable hashtable = info.OperationRequest.Parameters[248] as System.Collections.Hashtable;

            int hight = (int)hashtable["hight"];
            int width = (int)hashtable["width"];
            NumberOfPlayers = (int)hashtable["maxPlayers"];


            float mineRate = 0.1f;
            if (hashtable.ContainsKey("mineRate"))
                mineRate = (float)(int)hashtable["mineRate"]/100;
            
            board = new Board(hight, width, (int)(hight * width * mineRate));


            this.PluginHost.LogInfo(string.Format("GameCreated {0} by {1} with {2} mines", hight, width, (int)(hight * width * mineRate)));
            
        }*/

       /* public override void OnJoin(IJoinGameCallInfo info)
        {
            base.OnJoin(info);

            if (PluginHost.GameActors.Count > NumberOfPlayers)
                PluginHost.RemoveActor(info.ActorNr, "Room is full");
            if (IsGameStarted)
            {
                scoreManager.AddPlayer(info.ActorNr,0);
                PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.GameStart, new Dictionary<byte, object>() { { (byte)0, board.Hight }, { (byte)1, board.Width } }, CacheOperations.DoNotCache);
                foreach (Dictionary<byte, object> dic in board.AllReveald())
                    PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.SyncFields, dic, CacheOperations.DoNotCache);

                foreach (Dictionary<byte, object> dic in board.AllFlags())
                    PluginHost.BroadcastEvent(new List<int> { info.ActorNr }, 0, (byte)Event.SyncFlags, dic, CacheOperations.DoNotCache);

            }
            else
            if (PluginHost.GameActors.Count >= NumberOfPlayers)
            {
                PluginHost.CreateOneTimeTimer(
                () => StartGame(),
                startDuration);
            }
            

        }*/
        /*
        public override void OnLeave(ILeaveGameCallInfo info)
        {
            base.OnLeave(info);
            if (IsGameStarted)
            {
                scoreManager.RemovePlayer(info.ActorNr);
            }
        }
        */
        public override void StartGame()
        {
            turnManager = new TurnManager(PluginHost.GameActors, PluginHost, 500);
            scoreManager = new ScoreManager(PluginHost.GameActors, PluginHost);

            BroadcastEvent((byte)Event.GameStart, new Dictionary<byte, object>() { { (byte)0, board.Hight }, { (byte)1, board.Width },{ 2, board.MineCount } });
            IsGameStarted = true;

            turnManager.Start();
            scoreManager.Start();
        }
        /*
        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            /*if(turnManager.Current.ActorNr != info.ActorNr)
            {
                PluginHost.LogError("" + info.ActorNr + " Not on his turn");
                info.Continue();
                return;
            }////

            switch ((Event)info.Request.EvCode)
            {
                case Event.Move:
                    OnMove(info);
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
        }*/

        protected override void OnToggleFlag(IRaiseEventCallInfo info)
        {
            PluginHost.LogError("SetFlag");

            Dictionary<byte, object> data = info.Request.Data as Dictionary<byte, object>;

            int y = (int)data[0];
            int x = (int)data[1];

            if (board.IsFlag(y, x) || board.IsReveald(y,x)) return; //Can not remove flag

            if (board.IsMine(y, x))
                board.ToggleFlag(y, x);

            scoreManager.AddPoints(turnManager.Actors.Find(a => a.ActorNr == info.ActorNr), board.IsMine(y, x) ? 5 : -5);

            if (board.IsMine(y, x))
                BroadcastEvent((byte)Event.ReceiveToggleFlag, new Dictionary<byte, object>() { { (byte)0, y }, { (byte)1, x }, { (byte)2, board.IsFlag(y, x) } });
            else
                OpenTile(y, x, board.GetTile(y, x));

        }

        protected override void OnOpen(IRaiseEventCallInfo info)
        {

            //PluginHost.LogError("MOVE");

            Dictionary<byte, object> data = info.Request.Data as Dictionary<byte, object>;

            int y = (int)data[0];
            int x = (int)data[1];
            int val = board.GetTile(y, x);

            PluginHost.LogError(string.Format("y: {0} , x: {1} , val: {2} , IsIsReveald: {3} ", y, x, val, board.IsReveald(y, x)));

            if (!board.IsReveald(y, x))
            {
                OpenTile(y, x, val);
                scoreManager.AddPoints(turnManager.Actors.Find(a => a.ActorNr == info.ActorNr), val == 10? -5: 3);
            }
            else
                if(TryOpenRest(y, x))
                    scoreManager.AddPoints(turnManager.Actors.Find(a => a.ActorNr == info.ActorNr), 2);

            if (board.CheckIfComplete())
                BroadcastEvent((byte)Event.GameEnd, scoreManager.GetResult());

        }

        protected void OpenTile(int y, int x, int val)
        {
            if (!board.IsFlag(y, x))
            {
                if (val != 0)
                {
                    if (board.RevealdCount < 1 && firstSafe)
                        val = board.SwapMine(y, x);

                    BroadcastEvent((byte)Event.ReceiveMove, new Dictionary<byte, object>() { { (byte)0, y }, { (byte)1, x }, { (byte)2, val } });
                    board.Reveal(y, x);

                    if (val == 10 && endOnExpload)
                        BroadcastEvent((byte)Event.GameEnd, scoreManager.GetResult());
                }
                else
                    Expand(y, x);
            }
        }

        

    }
}
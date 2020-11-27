using Photon.Hive.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MineSweepersPlugins
{
    public class MineSweepersVersus : MineSweepersPlugin
    {

        public override string Name { get { return "MineSweepersVersus"; } }


        protected TurnManager turnManager;
        protected ScoreManager scoreManager;
        /// <summary>
        /// Called when game is started
        /// </summary>
        public override void StartGame()
        {
            turnManager = new TurnManager(PluginHost.GameActors, PluginHost, 500);
            scoreManager = new ScoreManager(PluginHost.GameActors, PluginHost);

            BroadcastEvent((byte)Event.GameStart, new Dictionary<byte, object>() { { (byte)0, board.Hight }, { (byte)1, board.Width }, { 2, board.MineCount } });
            IsGameStarted = true;

            turnManager.Start();
            scoreManager.Start();

        }
        /// <summary>
        /// Called when tile is opened
        /// </summary>
        /// <param name="info"></param>
        protected override void OnOpen(IRaiseEventCallInfo info)
        {


            Dictionary<byte, object> data = info.Request.Data as Dictionary<byte, object>;

            int y = (int)data[0];
            int x = (int)data[1];
            int val = board.GetTile(y, x);

            PluginHost.LogError(string.Format("y: {0} , x: {1} , val: {2} , IsIsReveald: {3} ", y, x, val, board.IsReveald(y, x)));

            if (!board.IsReveald(y, x))
            {
                OpenTile(y, x, val);
                if (!board.IsFlag(y, x))
                    scoreManager.AddPoints(turnManager.Actors.Find(a => a.ActorNr == info.ActorNr), val == 10 ? -5 : 3);
            }
            else
                if (TryOpenRest(y, x))
                scoreManager.AddPoints(turnManager.Actors.Find(a => a.ActorNr == info.ActorNr), 2);

            if (board.CheckIfComplete())
                EndGame();

        }

        /// <summary>
        /// Called when flag is toggled
        /// </summary>
        /// <param name="info"></param>
        protected override void OnToggleFlag(IRaiseEventCallInfo info)
        {
            PluginHost.LogError("SetFlag");

            Dictionary<byte, object> data = info.Request.Data as Dictionary<byte, object>;

            int y = (int)data[0];
            int x = (int)data[1];

            if (board.IsFlag(y, x) || board.IsReveald(y, x)) return; //Can not remove flag

            if (board.IsMine(y, x))
                board.ToggleFlag(y, x);

            scoreManager.AddPoints(turnManager.Actors.Find(a => a.ActorNr == info.ActorNr), board.IsMine(y, x) ? 5 : -5);

            if (board.IsMine(y, x))
                BroadcastEvent((byte)Event.ReceiveToggleFlag, new Dictionary<byte, object>() { { (byte)0, y }, { (byte)1, x }, { (byte)2, board.IsFlag(y, x) } });
            else
                OpenTile(y, x, board.GetTile(y, x));

        }
        /// <summary>
        /// Called when player needs to be syncronized
        /// </summary>
        /// <param name="info"></param>
        protected override void SyncPlayer(IJoinGameCallInfo info)
        {
            base.SyncPlayer(info);

            scoreManager.AddPlayer(info.ActorNr, 0);
        }

        /// <summary>
        /// Called when player leaves the game
        /// </summary>
        /// <param name="info"></param>
        public override void OnLeave(ILeaveGameCallInfo info)
        {
            base.OnLeave(info);
            if (IsGameStarted)
            {
                if(scoreManager.ContainPlayer(info.ActorNr))//if player gets kicked on join
                    scoreManager.RemovePlayer(info.ActorNr);
            }
        }
        /// <summary>
        /// Called when game is ended
        /// </summary>
        public override void EndGame()
        {
            BroadcastEvent((byte)Event.GameEnd, scoreManager.GetResult());
        }



    }
}
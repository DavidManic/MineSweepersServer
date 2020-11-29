using Photon.Hive.Plugin;
using System.Collections.Generic;

namespace MineSweepersPlugins
{
    public class MineSweepersCoop : MineSweepersPlugin
    {

        public override string Name { get { return "MineSweepersCoop"; } }
        /// <summary>
        /// Called when game is started
        /// </summary>
        public override void StartGame()
        {
            BroadcastEvent((byte)Event.GameStart, new Dictionary<byte, object>() { { (byte)0, board.Hight }, { (byte)1, board.Width }, { 2, board.MineCount } });
            IsGameStarted = true;
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
            }
            else
                TryOpenRest(y, x);

            if (board.CheckIfComplete())
                BroadcastEvent((byte)Event.GameEnd, null);
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

            board.ToggleFlag(y, x);

            BroadcastEvent((byte)Event.ReceiveToggleFlag, new Dictionary<byte, object>() { { (byte)0, y }, { (byte)1, x }, { (byte)2, board.IsFlag(y, x) } });


        }
        /// <summary>
        /// Called when games is ended
        /// </summary>
        public override void EndGame()
        {
            BroadcastEvent((byte)Event.GameEnd, null);
        }

        protected override void SyncPlayer(IJoinGameCallInfo info)
        {
            
            base.SyncPlayer(info);
        }
    }
}
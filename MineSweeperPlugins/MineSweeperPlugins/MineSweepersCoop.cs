using Photon.Hive.Plugin;
using System.Collections.Generic;

namespace MineSweepersPlugins
{
    public class MineSweepersCoop : MineSweepersPlugin
    {

        public override string Name { get { return "MineSweepersCoop"; } }

        public override void StartGame()
        {
            BroadcastEvent((byte)Event.GameStart, new Dictionary<byte, object>() { { (byte)0, board.Hight }, { (byte)1, board.Width }, { 2, board.MineCount } });
            IsGameStarted = true;
        }

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

        protected void OpenTile(int y, int x, int val)
        {
            if (!board.IsFlag(y, x))
            {
                if (val != 0)
                {
                    if (board.RevealdCount < 1 && firstSafe)
                        val = board.SwapMine(y, x);
                    board.Reveal(y, x);
                    BroadcastEvent((byte)Event.ReceiveMove, new Dictionary<byte, object>() { { (byte)0, y }, { (byte)1, x }, { (byte)2, val } });
                    if (val == 10 && endOnExpload)
                        BroadcastEvent((byte)Event.GameEnd, null);
                }
                else
                    Expand(y, x);
            }
        }

        protected override void OnToggleFlag(IRaiseEventCallInfo info)
        {
            PluginHost.LogError("SetFlag");

            Dictionary<byte, object> data = info.Request.Data as Dictionary<byte, object>;

            int y = (int)data[0];
            int x = (int)data[1];

            board.ToggleFlag(y, x);

            BroadcastEvent((byte)Event.ReceiveToggleFlag, new Dictionary<byte, object>() { { (byte)0, y }, { (byte)1, x }, { (byte)2, board.IsFlag(y, x) } });


        }
    }
}
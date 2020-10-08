using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweepersPlugins
{

    public enum Event { GameStart = 1, GameEnd = 2, StartTurn = 10, EndTurn = 11, Move = 12, ToggleFlag=13, ReceiveMove = 20, ReceiveToggleFlag=21, CurrentPlayerChanged = 30, scoreSet = 31, scoreUpdate = 32, SyncFields =90,SyncFlags=91 }
}

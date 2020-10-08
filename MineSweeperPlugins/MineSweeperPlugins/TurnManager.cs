using Photon.Hive.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Logging;

namespace MineSweepersPlugins
{
    public class TurnManager
    {
        private IPluginHost pluginHost;
        private object timer;
        private int turnDuration;//ms
        int pauseBetweenTurns = 500; //ms
        private int current = 0;
        private List<IActor> actors;

        public List<IActor> Actors { get { return actors; } }
        public delegate void PlayerChangedHandler();
        public event PlayerChangedHandler OnPlayerChanged = delegate { };

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public IActor CurrentActor { get { return actors[current]; } }

        public TurnManager(IList<IActor> list, IPluginHost pluginHost, int turnDuration)
        {
            actors = new List<IActor>();
            actors.AddRange(list.ToArray());
            this.pluginHost = pluginHost;
            this.turnDuration = turnDuration;

        }

        public void Start()
        {
            log.DebugFormat("TurnManager.Start called for game '{0}'.", pluginHost.GameId);

            pluginHost.BroadcastEvent(new List<int> { CurrentActor.ActorNr }, 0, (int)Event.StartTurn, null, 0);
            timer = pluginHost.CreateOneTimeTimer(() => NextTurn(), turnDuration);
            OnPlayerChanged();
        }

        public void Stop()
        {
            pluginHost.BroadcastEvent(new List<int> { Current.ActorNr }, 0, (int)Event.EndTurn, null, CacheOperations.DoNotCache);
            pluginHost.StopTimer(timer);
        }


        public IActor NextTurn()
        {
            //End player turn
            log.DebugFormat("Ending player {0}'s turn.", CurrentActor.ActorNr);
            pluginHost.StopTimer(timer);
            pluginHost.BroadcastEvent(new List<int> { CurrentActor.ActorNr }, 0, (int)Event.EndTurn, null, CacheOperations.DoNotCache);

            //Start player's turn after a delay
            current = (++current) % actors.Count;
            log.DebugFormat("Starting player {0}'s turn.", CurrentActor.ActorNr);
            pluginHost.CreateOneTimeTimer(StartPlayerTurn, pauseBetweenTurns);

            return actors[current];
        }

        //Instigates the start of the player's turn.
        private void StartPlayerTurn()
        {
            pluginHost.BroadcastEvent(new List<int> { CurrentActor.ActorNr }, 0, (int)Event.StartTurn, null, CacheOperations.DoNotCache);
            log.DebugFormat("Sent StartTurn event to clients for player {0}'s turn.", CurrentActor.ActorNr);
            timer = pluginHost.CreateOneTimeTimer(() => NextTurn(), turnDuration);

            OnPlayerChanged();
        }

        public IActor Repeat()
        {
            pluginHost.StopTimer(timer);
            pluginHost.BroadcastEvent(new List<int> { CurrentActor.ActorNr }, 0, (int)Event.EndTurn, null, CacheOperations.DoNotCache);

            pluginHost.BroadcastEvent(new List<int> { CurrentActor.ActorNr }, 0, (int)Event.StartTurn, null, CacheOperations.DoNotCache);
            timer = pluginHost.CreateOneTimeTimer(() => NextTurn(), turnDuration);

            OnPlayerChanged();

            return actors[current];
        }

        private void ChangePlayer()
        {
            OnPlayerChanged.Invoke();

            Dictionary<byte, object> data = new Dictionary<byte, object>();
            data.Add(0, CurrentActor.Nickname.Length == 0 ? "User_" + CurrentActor.ActorNr : CurrentActor.Nickname);
            data.Add(1, actors[current].ActorNr);

            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            parameters.Add(245, data);
            pluginHost.BroadcastEvent(ReciverGroup.All, 0, 0, (int)Event.CurrentPlayerChanged, parameters, CacheOperations.DoNotCache);

            log.DebugFormat("Player changed. Current is Player {0}.", CurrentActor.ActorNr);
        }
        public IActor LastActor
        {
            get
            {
                int i = current - 1 % actors.Count;
                return actors[i < 0 ? i + actors.Count : i];
            }
        }
        public IActor Current
        {
            get { return actors[current]; }
        }

        public IActor NextActor
        {
            get { return actors[(current + 1) % actors.Count]; }
        }

        public void SetCurrent(int index)
        {
            if (index < actors.Count)
                current = index;
        }

        public bool Conteins(int actorNr)
        {
            foreach (IActor actor in actors)
                if (actor.ActorNr == actorNr)
                    return true;
            return false;
        }

        public IActor GetActor(int actorNr)
        {
            foreach (IActor actor in actors)
                if (actor.ActorNr == actorNr)
                    return actor;
            return null;
        }
    }
}

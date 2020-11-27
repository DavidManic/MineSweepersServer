using Photon.Hive.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace MineSweepersPlugins
{
    /// <summary>
    /// Manages players socres and comunicates state between players and server
    /// </summary>
    public class ScoreManager
    {
        Dictionary<int, int> scores;
        Dictionary<int, int> disconects;
        IPluginHost host;

        public ScoreManager(IList<IActor> actors, IPluginHost host)
        {
            scores = new Dictionary<int, int>();
            disconects = new Dictionary<int, int>();
            foreach (IActor a in actors)
                scores.Add(a.ActorNr, 0);

            this.host = host;
        }
        /// <summary>
        /// Add actor with initial score to pool of players.    
        /// </summary>
        /// <param name="actorNr"></param>
        /// <param name="score"></param>
        public void AddPlayer(int actorNr, int score)
        {

            scores.Add(actorNr, disconects.ContainsKey(actorNr) ? disconects[actorNr] : score);
            host.BroadcastEvent(new List<int> { actorNr }, 0, (byte)Event.scoreSet, new Dictionary<byte, object>() { { (byte)0, scores } }, CacheOperations.DoNotCache);
        }
        /// <summary>
        /// Remove actor from pool of players
        /// </summary>
        /// <param name="actorNr"></param>
        public void RemovePlayer(int actorNr)
        {
            disconects.Add(actorNr, scores[actorNr]);
            scores.Remove(actorNr);
        }

        /// <summary>
        /// Give an actor amount of points
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="points"></param>
        public void AddPoints(IActor actor, int points)
        {
            if (scores.ContainsKey(actor.ActorNr))
            {
                scores[actor.ActorNr] += points;
                host.BroadcastEvent(ReciverGroup.All, 0, 0, (byte)Event.scoreUpdate, new Dictionary<byte, object>() { { (byte)0, actor.ActorNr }, { 1, scores[actor.ActorNr] } }, CacheOperations.DoNotCache);
            }
        }

        /// <summary>
        /// Initiate ScoreManager.
        /// </summary>
        public void Start()
        {

            host.BroadcastEvent(ReciverGroup.All, 0, 0, (byte)Event.scoreSet, new Dictionary<byte, object>() { { (byte)0, scores } }, CacheOperations.DoNotCache);
        }

        /// <summary>
        /// Retrive actors and there respective scores in sorted order
        /// </summary>
        /// <returns></returns>
        public Dictionary<byte, object> GetResult()
        {
            Dictionary<byte, object> result = new Dictionary<byte, object>();

            //Sort player by score
            var ranks = scores.OrderByDescending(x=>x.Value).ToList();

            byte i = 0;
            ranks.ForEach(x => { result.Add(i++, x.Key); result.Add(i++, x.Value); });

            return result;


        }

        /// <summary>
        /// Check if player is inside of list.
        /// </summary>
        /// <param name="ActorNr"></param>
        /// <returns></returns>
        public bool ContainPlayer(int ActorNr)
        {
            return scores.Keys.Contains(ActorNr);
        }
    }
}
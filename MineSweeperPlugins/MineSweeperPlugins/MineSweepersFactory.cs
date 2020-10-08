using Photon.Hive.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweepersPlugins
{
    public class MineSweepersFactory : IPluginFactory
    {
        public IGamePlugin Create(IPluginHost gameHost, string pluginName, Dictionary<string, string> config,
       out string errorMsg)
        {
            gameHost.LogInfo("Name" + pluginName);

            PluginBase plugin = new PluginBase();

            switch (pluginName)
            {
               /* case "MineSweepersPlugin":
                    plugin = new MineSweepersPlugin();
                    break;*/
                case "MineSweepersCoop":
                    plugin = new MineSweepersCoop();
                    break;
                case "MineSweepersVersus":
                    plugin = new MineSweepersVersus();
                    break;
                default:
                    break;

            }
            if (plugin.SetupInstance(gameHost, config, out errorMsg))
            {
                gameHost.LogError(config);
                gameHost.LogError(config.Keys);
                return plugin;
            }

            return null;
        }
    }
}

using RevBridge.Framework.SilkroadSecurityApi;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RevBridge.Timers
{
    internal sealed class RandomWeather
    {
        private static Timer randomWeather;

        public static void _Start()
        {
            randomWeather = new Timer(new TimerCallback(OnCallbackAsync), 0, 0, Properties.Settings.Default.RevBridge_RandomWeather_Interval * 60 * 1000);
        }

        private static async void OnCallbackAsync(object a)
        {
            if (Definitions.List.AgentConnections.Count > 0)
            {
                await Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Random weather geldi?");
                    Random rndWeather = new Random();

                    int weatherType = rndWeather.Next(1, 4);
                    int weatherAmount = rndWeather.Next(1, 256);

                    Packet newWeatherPacket = new Packet(Opcodes.Agent.Server.ENVIRONMENT_WEATHER);
                    newWeatherPacket.WriteUInt8(weatherType);
                    newWeatherPacket.WriteUInt8(weatherAmount);

                    Functions.RevBridge.Packets.SendPacketToPlayerAsServer(newWeatherPacket);

                    if (Definitions.List.RegisteredPackets.Any(x => x.Opcode == Opcodes.Agent.Server.ENVIRONMENT_WEATHER))
                        Definitions.List.RegisteredPackets[Definitions.List.RegisteredPackets.IndexOf(Definitions.List.RegisteredPackets.Single(x => x.Opcode == Opcodes.Agent.Server.ENVIRONMENT_WEATHER))] = newWeatherPacket;
                    else
                        Definitions.List.RegisteredPackets.Add(newWeatherPacket);

                    Functions.RevBridge.Packets.SendPacketToPlayerAsServer(Functions.PacketCreators.Chat.Notice($"New weather: {weatherType} - Weather amount: {weatherAmount}"));

                    System.Diagnostics.Debug.WriteLine($"[randomWeather] New weather type: {weatherType} - Amount: {weatherAmount}");
                }).ConfigureAwait(false);
            }
        }

        public static void _Stop()
        {
            randomWeather?.Dispose();
        }
    }
}
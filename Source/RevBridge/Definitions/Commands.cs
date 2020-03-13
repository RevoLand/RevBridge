namespace RevBridge.Definitions
{
    internal sealed class Commands
    {
        [Command(".gm", "Kullanımı: .gm")]
        public static void GMKontrol(Character Character)
        {
            Character.SendNotice($"You are {(Character.IsGm ? "a" : "not a")} GM");
        }

        [Command(".greedy", "Kullanımı: .greedy <Uzun yazı>", GreedyArg = true)]
        public static void GMKontrol(Character Character, string yazi)
        {
            Character.SendNotice($"You are {(Character.IsGm ? "a" : "not a")} GM - text: " + yazi);
        }
    }
}
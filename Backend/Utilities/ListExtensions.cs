namespace MusicPlayerApp
{
    public static class ListExtensions
    {
        // Fisher–Yates shuffle
        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = Random.Shared;
            
            // Loop from n - 1 to 0
            for(int i = list.Count - 1; i >= 1; i--)
            {
                // Generate random number such that 0 <= j <= i
                int j = random.Next(i + 1);

                // Swap list[i] and list[j]
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
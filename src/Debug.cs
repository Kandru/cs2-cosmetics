namespace Cosmetics
{
    public partial class Cosmetics
    {

        private void DebugPrint(string message)
        {
            if (Config.Debug)
            {
                Console.WriteLine(Localizer["core.debugprint"].Value.Replace("{message}", message));
            }
        }
    }
}
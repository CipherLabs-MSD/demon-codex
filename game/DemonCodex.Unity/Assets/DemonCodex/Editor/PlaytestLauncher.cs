using System;
using DemonCodex.Development;
using DemonCodex.Presentation;

namespace DemonCodex.Editor
{
    // Shared by the M001.1 window and PlayMode tests. A variant only sets the existing
    // bot presentation delay, then starts an ordinary seeded match through StartMatch.
    public static class PlaytestLauncher
    {
        public static PlaytestRecorder Start(LocalMatchController controller, PlaytestVariant variant, uint seed,
            string tester, Func<double> clock, Func<DateTimeOffset> utcNow)
        {
            controller.BotDelaySeconds = PlaytestExperiment.BotDelaySeconds(variant);
            controller.StartMatch(seed);
            return new PlaytestRecorder(controller.Session, variant, tester, clock, utcNow);
        }

        public static void RestoreControlPacing(LocalMatchController controller)
        {
            if (controller != null) controller.BotDelaySeconds = PlaytestExperiment.ControlBotDelaySeconds;
        }
    }
}

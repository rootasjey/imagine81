using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;

namespace BackgroundTaskQuote
{
    public sealed class TheTask : IBackgroundTask
    {
        // Only one method to implement here.
        public async void Run(IBackgroundTaskInstance instance)
        {
            // Get a deferral if we're doing async work.
            var deferral = instance.GetDeferral();

            // Update App Tile
            TileSetter.CreateTiles(null, null, "test");

            // Should do something to handle cancellation.
            instance.Canceled += (s, e) => { };

            // Report progress to a foreground app if there and listening.
            instance.Progress = 0;

            // More properties avaible on IBackgroundTaskInstance -
            // IntanceId, TriggerDetails,SuspendedCount, Task

            deferral.Complete();
        }
    }
}

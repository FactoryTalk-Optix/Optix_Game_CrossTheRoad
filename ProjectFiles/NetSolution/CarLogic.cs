#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.WebUI;
#endregion

public class CarLogic : BaseNetLogic
{
    public override void Start()
    {
        // Get the current car object
        thisCar = (Image)Owner;
        // Get the size of the screen
        screenSize = ((Screen)Owner.Owner).Width;
        // Start the bounce logic
        BounceCarTask = new PeriodicTask(Bounce, 500, LogicObject);
        BounceCarTask.Start();
        // Detect starting position
        startPos = thisCar.LeftMargin > 10;
        // Start the movement
        MoveCarTask = new PeriodicTask(Move, 10, LogicObject);
        MoveCarTask.Start();
    }   

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        BounceCarTask?.Dispose();
        MoveCarTask?.Dispose();
    }

    private void Move()
    {
        if (startPos)
            thisCar.LeftMargin -= 2;
        else
            thisCar.LeftMargin += 2;

        if (thisCar.LeftMargin <= -1 * thisCar.Width || thisCar.LeftMargin >= screenSize)
        {
            Log.Debug("Disposing car");
            BounceCarTask?.Dispose();
            MoveCarTask?.Dispose();
            thisCar.Delete();
        }
    }

    private void Bounce()
    {
        bounce = !bounce;
        if (bounce)
            thisCar.TopMargin++;
        else
            thisCar.TopMargin--;
    }

    private PeriodicTask BounceCarTask;
    private PeriodicTask MoveCarTask;
    private bool bounce = false;
    private Image thisCar;
    private bool startPos = false;
    private float screenSize = 0;
}

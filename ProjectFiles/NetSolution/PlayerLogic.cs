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
using System.Linq;
using FTOptix.WebUI;
#endregion

public class PlayerLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        thisPlayer = (Panel)Owner;
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void MoveUp()
    {
        thisPlayer.TopMargin -= 50;
    }

    [ExportMethod]
    public void MoveDown()
    {
        thisPlayer.TopMargin += 50;
    }

    [ExportMethod]
    public void MoveLeft()
    {
        thisPlayer.LeftMargin -= 50;
    }

    [ExportMethod]
    public void MoveRight()
    {
        thisPlayer.LeftMargin += 50;
    }

    private Panel thisPlayer;
}

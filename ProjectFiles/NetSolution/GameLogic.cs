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
using System.Threading;
#endregion

public class GameLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        rand = new Random();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        gameTickTask?.Dispose();
        carsGeneratorTask?.Dispose();
    }

    [ExportMethod]
    public void StartGame(NodeId mainWindow)
    {
        // Get to the MainWindow
        var mainWindowObject = InformationModel.Get<Window>(mainWindow);
        // Change panel
        var gamePanelType = Project.Current.Get("UI/Screens/StreetCrossing");
        var gamePanelLoader = mainWindowObject.Get<PanelLoader>("ScaleLayout/PanelLoader");
        gamePanelLoader.ChangePanel(gamePanelType);
        // Set current game screen
        gameScreen = InformationModel.Get<Screen>(gamePanelLoader.CurrentPanel);
        // Cleanup game screen
        gameScreen.Children.Where(t => t.BrowseName != "Background").ToList().ForEach(t => t.Delete());
        // Generate lanes directions
        for (int i = 0; i < lanesDirection.Length; i++)
        {
            lanesDirection[i] = rand.Next(11) % 2 == 0;
        }
        // Start cars generator
        carsGeneratorTask?.Dispose();
        carsGeneratorTask = new PeriodicTask(CarsGenerator, 500, LogicObject);
        carsGeneratorTask.Start();
        // Generate player
        thisPlayer = PlayerGenerator();
        // Start game timer
        gameTickTask?.Dispose();
        gameTickTask = new PeriodicTask(GameTick, 50, LogicObject);
        gameTickTask.Start();
    }

    private NodeId RandomCar()
    {
        var carsList = Project.Current.Get<Folder>("UI/Templates/Cars").Children.OfType<ImageType>().ToList();
        return carsList[rand.Next(carsList.Count)].NodeId;
    }

    private Player PlayerGenerator()
    {
        var newPlayer = InformationModel.Make<Player>($"Player-{RandomString()}");
        newPlayer.TopMargin = 630;
        newPlayer.LeftMargin = 540;
        gameScreen.Add(newPlayer);
        return newPlayer;
    }

    private void CarsGenerator() {
        var newCar = InformationModel.MakeObject($"Car-{RandomString()}", RandomCar());
        // Choose a new lane to spawn
        int spawnLane = rand.Next(spawnCoords.Length);
        int counter = 0;
        while (spawnLane == lastLane)
        {
            // Make sure lanes are different
            spawnLane = rand.Next(spawnCoords.Length);
            Thread.Sleep(1);
            counter++;
            if (counter >= 10)
            {
                spawnLane = (spawnLane + 2) % spawnCoords.Length;
            }
        }
        lastLane = spawnLane;
        var newCarImage = (Image)newCar;
        // Check direction for the car
        if (lanesDirection[spawnLane])
        {
            newCarImage.Rotation = 180;
            newCarImage.LeftMargin = gameScreen.Width;
        }
        else
        {
            newCarImage.LeftMargin = 0;
        }
        newCarImage.TopMargin = spawnCoords[spawnLane];
        // Add the new car
        gameScreen.Add(newCarImage);
    }

    private string RandomString()
    {
        Guid g = Guid.NewGuid();
        return g.ToString().Replace("-", "");
    }

    private void GameTick()
    {
        // Check if we won the game
        if (thisPlayer.TopMargin <= 140)
        {
            Stop();
            var gameOverImage = InformationModel.Make<YouWon>("YouWon");
            gameOverImage.HorizontalAlignment = HorizontalAlignment.Center;
            gameOverImage.VerticalAlignment = VerticalAlignment.Center;
            gameScreen.Add(gameOverImage);
            return;
        }
        // Check for impact
        try
        {
            var carsList = gameScreen.Children.Where(t => t.BrowseName.StartsWith("Car-")).ToList();
            foreach (var _ in carsList.Where(car => DetectCollision(thisPlayer, (Image)car)).Select(car => new { }))
            {
                Stop();
                var gameOverImage = InformationModel.Make<GameOver>("GameOver");
                gameOverImage.HorizontalAlignment = HorizontalAlignment.Center;
                gameOverImage.VerticalAlignment = VerticalAlignment.Center;
                gameScreen.Add(gameOverImage);
            }
        }
        catch (UAManagedCore.CoreException ex)
        {
            // If car gets out of the screen this function will fail, handle it safely
            Log.Debug($"Trying to handle a non existing car: {ex.Message}");
        }
    }

    public bool DetectCollision(Panel player, Image car)
    {
        if (player.LeftMargin < car.LeftMargin + car.Width && player.LeftMargin + player.Width > car.LeftMargin && player.TopMargin < car.TopMargin + car.Height && player.TopMargin + player.Height > car.TopMargin)
        {
            return true; // Collision detected!
        }
        return false; // No collision
    }

    readonly float[] spawnCoords = new float[8] {190, 240, 290, 340, 380, 430, 480, 520 };
    readonly bool[] lanesDirection = new bool[8];
    private PeriodicTask carsGeneratorTask;
    private Screen gameScreen;
    Random rand;
    private Panel thisPlayer;
    private PeriodicTask gameTickTask;
    private int lastLane = -1;
}

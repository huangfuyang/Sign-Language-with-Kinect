using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Kinect.Toolkit.Controls;

namespace EducationSystem.SignNumGame
{
    /// <summary>
    /// Interaction logic for GameSignNumPlayPage.xaml
    /// </summary>
    public partial class GameSignNumPlayPage : Page
    {

        private GameManager gameManager;
        private GameSignNumPlayFramesHandler framesHandler;

        public GameSignNumPlayPage()
        {
            InitializeComponent();
        }

        private void performMove(GameManager.Direction direction)
        {
            if (gameManager.Move(direction))
            {
                gameManager.UpdateGame(true);
                GameBoardPanel.Items.Refresh();
            }
        }

        private void Page_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GameManager.Direction direction = GameManager.Direction.LEFT;

            switch (e.Key)
            {
                case Key.Left:
                    direction = GameManager.Direction.LEFT;
                    break;
                case Key.Right:
                    direction = GameManager.Direction.RIGHT;
                    break;
                case Key.Up:
                    direction = GameManager.Direction.UP;
                    break;
                case Key.Down:
                    direction = GameManager.Direction.DOWN;
                    break;
            }

            performMove(direction);
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            const int BOARD_SIZE = 4;
            Grid grid = new Grid(BOARD_SIZE);
            gameManager = new GameManager(grid);
            this.DataContext = gameManager;
            GameBoardPanel.ItemsSource = gameManager.Board.TileCollection;

            System.Windows.Application.Current.MainWindow.KeyDown += new KeyEventHandler(Page_KeyUp);

            framesHandler = new GameSignNumPlayFramesHandler(this, GameBoardGrid);
            framesHandler.RegisterCallbackToSensor(KinectState.Instance.CurrentKinectSensor);
        }

        private class GameSignNumPlayFramesHandler : AbstractKinectFramesHandler
        {
            private GameSignNumPlayPage page;

            public GameSignNumPlayFramesHandler(GameSignNumPlayPage page, UIElement element)
                : base(element)
            {
                this.page = page;
            }

            public override void OnHandPointerGripRelease(HandPointer grippedHandpointer, System.Windows.Point startGripPoint, System.Windows.Point endGripPoint)
            {
                System.Windows.Point diffVector = (System.Windows.Point)(endGripPoint - startGripPoint);

                if (Math.Abs(diffVector.X / diffVector.Y) > 2.0)
                {
                    page.performMove(diffVector.X < 0 ? GameManager.Direction.LEFT : GameManager.Direction.RIGHT);
                }
                else if (Math.Abs(diffVector.Y / diffVector.X) > 2.0)
                {
                    page.performMove(diffVector.Y < 0 ? GameManager.Direction.UP : GameManager.Direction.DOWN);
                }
            }
        }
    }
}

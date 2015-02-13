using System.Windows.Controls;
using System.Windows.Input;

namespace EducationSystem.SignNumGame
{
    /// <summary>
    /// Interaction logic for GameSignNumPlayPage.xaml
    /// </summary>
    public partial class GameSignNumPlayPage : Page
    {

        private GameManager gameManager;

        public GameSignNumPlayPage()
        {
            InitializeComponent();
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

            if (gameManager.Move(direction))
            {
                gameManager.UpdateGame(true);
                GameBoardPanel.Items.Refresh();
            }
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            const int BOARD_SIZE = 4;
            Grid grid = new Grid(BOARD_SIZE);
            gameManager = new GameManager(grid);
            this.DataContext = gameManager;
            GameBoardPanel.ItemsSource = gameManager.Board.TileCollection;

            System.Windows.Application.Current.MainWindow.KeyDown += new KeyEventHandler(Page_KeyUp);
        }
    }
}

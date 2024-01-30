namespace FontAutoScalingMaui
{
    public partial class MainPage : ContentPage
    {

        public MainPage(ViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }
    }

}

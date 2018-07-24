using RemoteTaskManager.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RemoteProcessManager
{
	public partial class MainPage : ContentPage
	{
        MainViewModel vm;
		public MainPage()
		{
			InitializeComponent();
            vm = new MainViewModel();
            this.BindingContext = vm;

        }
	}
}
